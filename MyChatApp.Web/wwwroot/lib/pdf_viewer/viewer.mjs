import { GlobalWorkerOptions } from '../pdfjs-dist/dist/build/pdf.min.mjs';
import { EventBus, PDFLinkService, PDFFindController, PDFViewer } from '../pdfjs-dist/dist/web/pdf_viewer.mjs';

GlobalWorkerOptions.workerSrc = '../pdfjs-dist/dist/build/pdf.worker.min.mjs';

// Extract the file path from the URL query string.
const url = new URL(window.location);
// alert(url);
var fileUrl = url.searchParams.get('file');

// const sv = url.searchParams.get('sv');
const se = url.searchParams.get('se');
const sr = url.searchParams.get('sr');
const sp = url.searchParams.get('sp');
const sig = url.searchParams.get('sig');

fileUrl = fileUrl + "&se=" + se + "&sr=" + sr + "&sp=" + sp +"&sig=" + sig;
// alert(fileUrl);
// alert(sp);

// fileUrl = fileUrl + "?sv=" + sv + "&se=" + se + "&sr=" + sr + "&sig=" + sig;
if (!fileUrl) {
  throw new Error('File not specified in the URL query string');
}

const container = document.getElementById('viewerContainer');
const eventBus = new EventBus();

// Enable hyperlinks within PDF files.
const pdfLinkService = new PDFLinkService({
  eventBus,
});

// Enable the find controller.
const pdfFindController = new PDFFindController({
  eventBus,
  linkService: pdfLinkService,
});

// Create the PDF viewer.
const pdfViewer = new PDFViewer({
  container,
  eventBus,
  linkService: pdfLinkService,
  findController: pdfFindController,
});
pdfLinkService.setViewer(pdfViewer);

// Allow navigation to a citation from the URL hash.
eventBus.on('pagesinit', function () {
  pdfLinkService.setHash(window.location.hash.substring(1));
});

// Define how the "search" query parameter is handled.
eventBus.on('findfromurlhash', function(evt) {
  eventBus.dispatch('find', {
    source: evt.source,
    type: '',
    query: evt.query,
    caseSensitive: false,
    entireWord: false,
    highlightAll: false,
    findPrevious: false,
    matchDiacritics: true,
  });
});

// Load and initialize the document.
const pdfDocument = await pdfjsLib.getDocument({
  url: fileUrl,
  enableXfa: true,
}).promise;

pdfViewer.setDocument(pdfDocument);
pdfLinkService.setDocument(pdfDocument, null);
