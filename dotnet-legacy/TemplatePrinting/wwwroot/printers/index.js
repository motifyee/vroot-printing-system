let eventSource = null;
let lastPrinterState = new Map();

function showToast(message, type = 'info') {
	const container = document.getElementById('toastContainer');
	if (!container) return;

	let icon = '✨';
	if (type === 'success') icon = '✅';
	if (type === 'error') icon = '❌';

	const toast = document.createElement('div');
	toast.className = `toast ${type}`;
	toast.innerHTML = `
		<span class="toast-icon">${icon}</span>
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

		<div class="spec-grid jobs-container">
			<div class="spec-item">
				<span class="spec-label">Driver</span>
				<span class="spec-value">${p.driverName || 'Generic'}</span>
			</div>
			<div class="spec-item printable-jobs">
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
				${(() => {
					const defaultSize = p.defaultPaperSize;
					const otherSizes = p.paperSizes.filter(
						s => s.name !== defaultSize?.name
					);
					const sortedSizes = defaultSize
						? [
								{ ...defaultSize, isDefault: true },
								...otherSizes.map(s => ({ ...s, isDefault: false })),
						  ]
						: otherSizes.map(s => ({ ...s, isDefault: false }));

					return (
						sortedSizes
							.slice(0, 5)
							.map(size => {
								const dims = `<span style="font-size: 0.65rem; opacity: 0.7; margin-left: 4px;">(${size.width}x${size.height})</span>`;
								return size.isDefault
									? `<span class="tag tag-default"><span class="tag-default-label">DEFAULT</span> ${size.name}${dims}</span>`
									: `<span class="tag">${size.name}${dims}</span>`;
							})
							.join('') +
						(sortedSizes.length > 5
							? `<span class="tag" title="${sortedSizes
									.slice(5)
									.map(s => `${s.name} (${s.width}x${s.height})`)
									.join(', ')}">+ ${sortedSizes.length - 5} more</span>`
							: '')
					);
				})()}
			</div>
		</div>

		<button class="btn-test-print" onclick="testPrinter('${p.name.replace(
			/'/g,
			"\\'"
		)}', this)">
			<div class="spinner"></div>
			<span class="btn-text">Test</span>
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
			showToast(result.message);
		} else {
			showToast(result.error || 'Failed to print test page', 'error');
		}
	} catch (err) {
		console.error('Test print failed:', err);
		showToast('Connection error. Failed to reach the server.', 'error');
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
			if (p.jobCount > 0) div.classList.add('updated-highlight');
			div.innerHTML = createPrinterCardContent(p);
			container.appendChild(div);
		} else if (hasChanged) {
			// Update existing content
			card.innerHTML = createPrinterCardContent(p);

			if (p.jobCount > 0) {
				card.classList.add('updated-highlight');
				showToast(`Printer "${p.name}" updated (Active Jobs: ${p.jobCount})`);

				const removeHighlight = () => {
					card.classList.remove('updated-highlight');
					card.removeEventListener('mouseenter', removeHighlight);
				};
				card.addEventListener('mouseenter', removeHighlight);
			} else {
				card.classList.remove('updated-highlight');
				if (prevState && prevState.jobCount > 0) {
					showToast(`Printer "${p.name}" jobs cleared`);
				} else {
					showToast(`Printer "${p.name}" updated`);
				}
			}
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
						<button class="btn-retry" style="background: var(--text-dim); margin-top: 1rem;" onclick="loadDummyData()">Try Dummy Data</button>
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
				<div style="display: flex; gap: 1rem; justify-content: center; margin-top: 1rem;">
					<button class="btn-retry" onclick="startMonitoring()">Reconnect Now</button>
					<button class="btn-retry" style="background: var(--text-dim);" onclick="loadDummyData()">Try Dummy Data</button>
				</div>
			</div>`;
		eventSource.close();
	};
}

async function loadDummyData() {
	try {
		const response = await fetch('/Printers/Dummy');
		const data = await response.json();
		updatePrinterUI(data);
		showToast('Loaded dummy printer data for preview');
	} catch (err) {
		console.error('Failed to load dummy data:', err);
		showToast('Failed to load dummy data', 'error');
	}
}

function initViewToggle() {
	const container = document.getElementById('printerContainer');
	const toggle = document.getElementById('viewToggle');
	if (!toggle || !container) return;

	const buttons = toggle.querySelectorAll('.toggle-btn');

	// Load saved preference
	const savedView = localStorage.getItem('vroot-printer-view') || 'compact';
	setView(savedView);

	buttons.forEach(btn => {
		btn.addEventListener('click', () => {
			const view = btn.dataset.view;
			setView(view);
		});
	});

	function setView(view) {
		if (view === 'compact') {
			container.classList.add('compact-mode');
		} else {
			container.classList.remove('compact-mode');
		}

		buttons.forEach(b => {
			b.classList.toggle('active', b.dataset.view === view);
		});

		localStorage.setItem('vroot-printer-view', view);
	}
}

function initThemeToggle() {
	const toggle = document.getElementById('themeToggle');
	if (!toggle) return;

	const buttons = toggle.querySelectorAll('.toggle-btn');

	// Load saved preference
	const savedTheme = localStorage.getItem('vroot-printer-theme') || 'light';
	setTheme(savedTheme);

	buttons.forEach(btn => {
		btn.addEventListener('click', () => {
			const theme = btn.dataset.theme;
			setTheme(theme);
		});
	});

	function setTheme(theme) {
		if (theme === 'light') {
			document.body.classList.add('light-theme');
		} else {
			document.body.classList.remove('light-theme');
		}

		buttons.forEach(b => {
			b.classList.toggle('active', b.dataset.theme === theme);
		});

		localStorage.setItem('vroot-printer-theme', theme);
	}
}

// Initial start
initThemeToggle();
initViewToggle();
startMonitoring();
