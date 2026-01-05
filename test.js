// Example JavaScript code to send a POST request to the PrintInvoice endpoint with dummy invoice data
// NODE_TLS_REJECT_UNAUTHORIZED=0 node test.js

const { readFileSync } = require('fs');
const https = require('https'); // Add this for SSL bypass

const dummyInvoice = {
	Status: 1,
	PrinterName: 'receipt',
	TemplateName: 'receipt',
	GlobalPrinter: true,
	Company: 'Sample Company',
	Cashier: 'John Doe',
	Branch: 'Main Branch',
	BranchDesc: 'Head Office',
	Date: '2023-10-01',
	Time: '12:00:00',
	ShiftNo: '1',
	InvoiceNo: 'INV-001',
	InvoiceType: 'Sale',
	DeliveryName: 'Express Delivery',
	SectionName: 'Section A',
	ScheduleTime: '2023-10-01 14:00',
	TableNo: 'T1',
	Discount: '5.00',
	Service: '2.00',
	Delivery: '10.00',
	Vat: '14.00',
	Visa: '50.00',
	VisaPer: '10',
	Total: '100.00',
	Note: 'Thank you for your business',
	PrintingDate: '2023-10-01',
	PrintingTime: '12:00:00',
	FooterNote1: 'Note 1',
	FooterNote2: 'Note 2',
	FooterNote3: 'Note 3',
	ImageSrc: 'logo.png',
	// Assuming Items is a list of objects, e.g., [{ Title: "Item1", Price: 10.00 }, ...]
	Items: [
		{ Title: 'Item1', Price: '10.0' },
		{ Title: 'Item2', Price: '20.0' },
		{ Title: 'Item2', Price: '20.0' },
		{ Title: 'Item2', Price: '20.0' },
		{ Title: 'Item2', Price: '20.0' },
	],
};

fetch('https://localhost:7229/PrintingData', {
	method: 'POST',
	headers: {
		'Content-Type': 'application/json',
	},
	body: JSON.stringify(dummyInvoice),
	agent: new https.Agent({ rejectUnauthorized: false }), // Bypass SSL verification for self-signed cert
	tls: {
		// ca: readFileSync('./cert.pem'),
		rejectUnauthorized: false,
	},
})
	.then(() => console.log('Success posting to PrintInvoice endpoint ✅'))
	.catch(error => console.error('❌ Error:', error));
