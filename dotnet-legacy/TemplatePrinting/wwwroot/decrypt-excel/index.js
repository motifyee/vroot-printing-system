// DOM Elements
const container = document.querySelector('.container');
const uploadArea = document.getElementById('uploadArea');
const fileInput = document.getElementById('fileInput');
const fileName = document.getElementById('fileName');
const password = document.getElementById('password');
const togglePassword = document.getElementById('togglePassword');
const decryptForm = document.getElementById('decryptForm');
const decryptButton = document.getElementById('decryptButton');
const message = document.getElementById('message');
const previewCard = document.getElementById('previewCard');
const downloadButton = document.getElementById('downloadButton');
const resetButton = document.getElementById('resetButton');

let spreadsheetInstance = null;

let selectedFile = null;
let decryptedBlob = null;
let decryptedFileName = '';

// File upload via click

uploadArea.addEventListener('click', e => {
	// Ignore if clicking the input itself or the label (label triggers input natively)
	if (e.target !== fileInput && !e.target.closest('label')) {
		fileInput.click();
	}
});

fileInput.addEventListener('change', e => {
	handleFileSelect(e.target.files[0]);
});

// Drag and drop functionality
uploadArea.addEventListener('dragover', e => {
	e.preventDefault();
	uploadArea.classList.add('drag-over');
});

uploadArea.addEventListener('dragleave', () => {
	uploadArea.classList.remove('drag-over');
});

uploadArea.addEventListener('drop', e => {
	e.preventDefault();
	uploadArea.classList.remove('drag-over');

	const files = e.dataTransfer.files;
	if (files.length > 0) {
		handleFileSelect(files[0]);
	}
});

// Handle file selection
function handleFileSelect(file) {
	if (!file) return;

	// Validate file type
	const validExtensions = ['.xlsx', '.xls'];
	const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

	if (!validExtensions.includes(fileExtension)) {
		showMessage('Please select a valid Excel file (.xlsx or .xls)', 'error');
		return;
	}

	selectedFile = file;
	fileName.textContent = `📄 ${file.name}`;
	hideMessage();
}

// Toggle password visibility
togglePassword.addEventListener('click', () => {
	const type = password.type === 'password' ? 'text' : 'password';
	password.type = type;
	togglePassword.textContent = type === 'password' ? '👁️' : '🙈';
});

// Form submission
decryptForm.addEventListener('submit', async e => {
	e.preventDefault();

	if (!selectedFile) {
		showMessage('Please select a file to decrypt', 'error');
		return;
	}

	if (!password.value.trim()) {
		showMessage('Please enter the decryption password', 'error');
		password.focus();
		return;
	}

	await decryptFile();
});

// Decrypt file function
async function decryptFile() {
	const formData = new FormData();
	formData.append('file', selectedFile);
	formData.append('password', password.value);

	// Show loading state
	decryptButton.classList.add('loading');
	decryptButton.disabled = true;
	hideMessage();

	try {
		const response = await fetch('/decrypt-excel/decrypt', {
			method: 'POST',
			body: formData,
		});

		if (response.ok) {
			// Get the decrypted file
			decryptedBlob = await response.blob();
			const contentDisposition = response.headers.get('Content-Disposition');
			decryptedFileName = 'decrypted_file.xlsx';

			// Extract filename from Content-Disposition header if available
			if (contentDisposition) {
				const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(
					contentDisposition
				);
				if (matches != null && matches[1]) {
					decryptedFileName = matches[1].replace(/['"]/g, '');
				}
			}

			// Preview the file
			await previewExcelWithSyncfusion(decryptedBlob);

			showMessage('✅ File decrypted successfully!', 'success');
		} else {
			// Handle error response
			const errorData = await response.json();
			showMessage(`❌ ${errorData.error || 'Decryption failed'}`, 'error');
		}
	} catch (error) {
		console.error('Error:', error);
		showMessage(
			'❌ An error occurred while decrypting the file. Please try again.',
			'error'
		);
	} finally {
		// Hide loading state
		decryptButton.classList.remove('loading');
		decryptButton.disabled = false;
	}
}

// Preview Excel using Syncfusion Spreadsheet
async function previewExcelWithSyncfusion(blob) {
	// Clear existing instance if any
	if (spreadsheetInstance) {
		spreadsheetInstance.destroy();
		const container = document.getElementById('spreadsheet');
		if (container) container.innerHTML = '';
	}

	// Initialize Syncfusion Spreadsheet
	spreadsheetInstance = new ej.spreadsheet.Spreadsheet({
		showRibbon: false,
		showFormulaBar: true,
		allowOpen: true,
		allowSave: false,
		allowEditing: false,
		// openUrl: 'https://document.syncfusion.com/web-services/spreadsheet-editor/api/spreadsheet/open',
		// saveUrl: 'https://document.syncfusion.com/web-services/spreadsheet-editor/api/spreadsheet/save',
		created: async () => {
			// Load the file once the component is created
			// spreadsheetInstance.open({ file: file });

			try {
				// Send the decrypted blob to our local endpoint to get JSON data
				const formData = new FormData();
				// Create a File object from the blob to ensure filename is passed
				const file = new File([blob], decryptedFileName, { type: blob.type });
				formData.append('file', file);

				const response = await fetch('/decrypt-excel/open', {
					method: 'POST',
					body: formData,
				});

				if (response.ok) {
					const data = await response.json();

					// Show preview card and expand container
					previewCard.style.display = 'block';
					container.classList.add('expanded');

					// Force layout update to ensure correct rendering dimensions
					spreadsheetInstance.resize();

					// Load the JSON data into spreadsheet using openFromJson
					await spreadsheetInstance.openFromJson({ file: data });

					previewCard.scrollIntoView({ behavior: 'smooth' });
					setTimeout(() => spreadsheetInstance.goTo('A2'), 100);
				} else {
					const errorData = await response.json();
					showMessage(
						`❌ Error loading preview: ${errorData.error || 'Unknown error'}`,
						'error'
					);
				}
			} catch (error) {
				console.error('Error fetching JSON for spreadsheet:', error);
				showMessage(
					'❌ Failed to load the preview. Please try again.',
					'error'
				);
			}
		},
	});

	// Render initialized Spreadsheet component
	spreadsheetInstance.appendTo('#spreadsheet');
}

// Download button handler
downloadButton.addEventListener('click', () => {
	if (!decryptedBlob) return;

	const url = window.URL.createObjectURL(decryptedBlob);
	const a = document.createElement('a');
	a.href = url;
	a.download = decryptedFileName;
	document.body.appendChild(a);
	a.click();
	window.URL.revokeObjectURL(url);
	document.body.removeChild(a);
});

// Reset button handler
resetButton.addEventListener('click', () => {
	resetForm();
});

// Show message
function showMessage(text, type) {
	message.textContent = text;
	message.className = `message show ${type}`;
}

// Hide message
function hideMessage() {
	message.classList.remove('show');
}

// Reset form
function resetForm() {
	selectedFile = null;
	decryptedBlob = null;
	decryptedFileName = '';
	fileName.textContent = '';
	password.value = '';
	fileInput.value = '';
	hideMessage();

	// Hide preview and collapse container
	previewCard.style.display = 'none';
	container.classList.remove('expanded');

	// Destroy spreadsheet instance
	if (spreadsheetInstance) {
		spreadsheetInstance.destroy();
		spreadsheetInstance = null;
	}

	const spreadsheetContainer = document.getElementById('spreadsheet');
	if (spreadsheetContainer) {
		spreadsheetContainer.innerHTML = '';
	}

	// Scroll back to top
	window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Allow Enter key to submit when password field is focused
password.addEventListener('keypress', e => {
	if (e.key === 'Enter') {
		decryptForm.dispatchEvent(new Event('submit'));
	}
});
