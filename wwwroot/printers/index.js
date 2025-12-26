let eventSource = null;

function updatePrinterUI(printers) {
	const container = document.getElementById('printerContainer');

	if (printers.length === 0) {
		container.innerHTML = `
			<div class="error-state">
				<h3>No Printers Found</h3>
				<p>No installed printers were detected on this system.</p>
			</div>`;
		return;
	}

	container.innerHTML = printers
		.map(
			p => `
	<div class="card" id="printer-${p.name.replace(/\s+/g, '-')}">
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
		
		<div class="tag-container">
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
	</div>
`
		)
		.join('');
}

function startMonitoring() {
	const container = document.getElementById('printerContainer');

	if (eventSource) {
		eventSource.close();
	}

	eventSource = new EventSource('/Printers/Data');

	eventSource.onmessage = function (event) {
		try {
			const data = JSON.parse(event.data);
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
			console.error('Error parsing printer data', e);
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
