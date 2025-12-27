let eventSource = null;
let lastPrinterState = new Map();

function showToast(message) {
	const container = document.getElementById('toastContainer');
	if (!container) return;

	const toast = document.createElement('div');
	toast.className = 'toast';
	toast.innerHTML = `
		<span class="toast-icon">✨</span>
		<span>${message}</span>
	`;
	container.appendChild(toast);

	setTimeout(() => {
		toast.classList.add('fade-out');
		setTimeout(() => toast.remove(), 500);
	}, 3000);
}

function createPrinterCardContent(p) {
	return `
		<div class="card-title">
			<span title="${p.fullName}">${
		p.name
	} <span class="status-pill" style="font-size: 0.6rem; vertical-align: middle; background: rgba(63, 185, 80, 0.1); color: var(--success); margin-left: 4px; border: 1px solid rgba(63, 185, 80, 0.2);">LIVE</span></span>
			<div style="display: flex; gap: 0.5rem; flex-wrap: wrap; justify-content: flex-end;">
				${p.isDefault ? '<span class="status-pill status-default">Default</span>' : ''}
				${
					p.isNetwork
						? '<span class="status-pill" style="background: rgba(139, 148, 158, 0.1); color: var(--text-dim);">Network</span>'
						: ''
				}
				${
					p.isOffline
						? '<span class="status-pill status-invalid">Offline</span>'
						: p.isValid
						? '<span class="status-pill status-valid">Online</span>'
						: '<span class="status-pill status-invalid">Invalid</span>'
				}
			</div>
		</div>

		<div class="spec-grid" style="grid-template-columns: 1fr; gap: 0.75rem; margin-bottom: 1.25rem;">
			<div class="spec-item">
				<span class="spec-label">Connection & Location</span>
				<span class="spec-value" style="font-size: 0.8rem; color: #fff;">${
					p.portName || 'Unknown Port'
				} — ${p.location || 'No Location'}</span>
			</div>
		</div>

		<div class="spec-grid">
			<div class="spec-item">
				<span class="spec-label">Driver</span>
				<span class="spec-value">${p.driverName || 'Generic'}</span>
			</div>
			<div class="spec-item">
				<span class="spec-label">Jobs</span>
				<span class="spec-value" style="${
					p.jobCount > 0 ? 'color: var(--accent); font-weight: 700;' : ''
				}">${p.jobCount} Active</span>
			</div>
			<div class="spec-item">
				<span class="spec-label">Color</span>
				<span class="spec-value">${p.supportsColor ? 'Supported' : 'B&W Only'}</span>
			</div>
			 <div class="spec-item">
				<span class="spec-label">Max Copies</span>
				<span class="spec-value">${p.maxCopies}</span>
			</div>
			<div class="spec-item">
				<span class="spec-label">Duplex</span>
				<span class="spec-value">${p.duplex}</span>
			</div>
			<div class="spec-item">
				<span class="spec-label">Is Shared</span>
				<span class="spec-value">${p.isShared ? `Yes (${p.shareName})` : 'No'}</span>
			</div>
		</div>
		
		</div>
		
		<div class="tag-container" style="margin-bottom: 1.25rem;">
			<span class="spec-label">Capabilities</span>
			<div class="tags">
				${p.paperSizes
					.slice(0, 5)
					.map(size => `<span class="tag">${size}</span>`)
					.join('')}
				${
					p.paperSizes.length > 5
						? `<span class="tag" title="${p.paperSizes
								.slice(5)
								.join(', ')}">+ ${p.paperSizes.length - 5} more</span>`
						: ''
				}
			</div>
		</div>

		<button class="btn-test-print" onclick="testPrinter('${p.name.replace(
			/'/g,
			"\\'"
		)}', this)">
			<div class="spinner"></div>
			<span class="btn-text">Test Paper</span>
		</button>
	`;
}

async function testPrinter(printerName, btn) {
	if (btn.classList.contains('loading')) return;

	btn.classList.add('loading');
	btn.disabled = true;

	try {
		const response = await fetch('/Printers/Test', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ printerName }),
		});

		const result = await response.json();

		if (response.ok) {
			showToast(`✨ ${result.message}`);
		} else {
			showToast(`❌ Error: ${result.error || 'Failed to print test page'}`);
		}
	} catch (err) {
		console.error('Test print failed:', err);
		showToast('❌ Connection error. Failed to reach the server.');
	} finally {
		btn.classList.remove('loading');
		btn.disabled = false;
	}
}

function updatePrinterUI(printers) {
	const container = document.getElementById('printerContainer');

	if (printers.length === 0) {
		container.innerHTML = `
			<div class="error-state">
				<h3>No Printers Found</h3>
				<p>No installed printers were detected on this system.</p>
			</div>`;
		lastPrinterState.clear();
		return;
	}

	// Remove skeleton or error-state if present
	if (
		container.querySelector('.skeleton') ||
		container.querySelector('.error-state')
	) {
		container.innerHTML = '';
	}

	const currentPrinterNames = new Set(printers.map(p => p.name));

	// Remove printers that are gone
	for (let name of lastPrinterState.keys()) {
		if (!currentPrinterNames.has(name)) {
			const id = `printer-${name.replace(/\s+/g, '-')}`;
			const el = document.getElementById(id);
			if (el) el.remove();
			lastPrinterState.delete(name);
		}
	}

	printers.forEach(p => {
		const id = `printer-${p.name.replace(/\s+/g, '-')}`;
		let card = document.getElementById(id);
		const prevState = lastPrinterState.get(p.name);
		const hasChanged =
			prevState && JSON.stringify(prevState) !== JSON.stringify(p);

		if (!card) {
			// Fresh entry
			const div = document.createElement('div');
			div.id = id;
			div.className = 'card';
			div.innerHTML = createPrinterCardContent(p);
			container.appendChild(div);
		} else if (hasChanged) {
			// Update existing content
			card.innerHTML = createPrinterCardContent(p);
			card.classList.add('updated-highlight');
			showToast(`Printer "${p.name}" updated`);

			const removeHighlight = () => {
				card.classList.remove('updated-highlight');
				card.removeEventListener('mouseenter', removeHighlight);
			};
			card.addEventListener('mouseenter', removeHighlight);
		}

		lastPrinterState.set(p.name, p);
	});
}

function startMonitoring() {
	const container = document.getElementById('printerContainer');

	if (eventSource) {
		eventSource.close();
	}

	eventSource = new EventSource('/Printers/Data');

	eventSource.onmessage = function (event) {
		let data;
		try {
			data = JSON.parse(event.data);
		} catch (e) {
			console.error(
				'Error parsing printer JSON data:',
				e,
				'Raw data:',
				event.data
			);
			return;
		}

		try {
			if (data.error) {
				container.innerHTML = `
					<div class="error-state">
						<h3 style="color: var(--error)">System Limitation</h3>
						<p>${data.error}</p>
					</div>`;
				eventSource.close();
				return;
			}
			updatePrinterUI(data);
		} catch (e) {
			console.error('Error updating printer UI with data:', e, data);
		}
	};

	eventSource.onerror = function (err) {
		console.error('EventSource failed:', err);
		container.innerHTML = `
			<div class="error-state">
				<h3 style="color: var(--error)">Connection Lost</h3>
				<p>Real-time monitoring interrupted. Attempting to reconnect...</p>
				<button class="btn-retry" onclick="startMonitoring()">Reconnect Now</button>
			</div>`;
		eventSource.close();
	};
}

// Initial start
startMonitoring();
