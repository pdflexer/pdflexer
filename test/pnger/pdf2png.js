var Canvas = require("canvas");
var assert = require("assert").strict;
var fs = require("fs");

function NodeCanvasFactory() {}
NodeCanvasFactory.prototype = {
  create: function NodeCanvasFactory_create(width, height) {
    assert(width > 0 && height > 0, "Invalid canvas size");
    var canvas = Canvas.createCanvas(width, height);
    var context = canvas.getContext("2d");
    return {
      canvas,
      context,
    };
  },

  reset: function NodeCanvasFactory_reset(canvasAndContext, width, height) {
    assert(canvasAndContext.canvas, "Canvas is not specified");
    assert(width > 0 && height > 0, "Invalid canvas size");
    canvasAndContext.canvas.width = width;
    canvasAndContext.canvas.height = height;
  },

  destroy: function NodeCanvasFactory_destroy(canvasAndContext) {
    assert(canvasAndContext.canvas, "Canvas is not specified");

    // Zeroing the width and height cause Firefox to release graphics
    // resources immediately, which can greatly reduce memory consumption.
    canvasAndContext.canvas.width = 0;
    canvasAndContext.canvas.height = 0;
    canvasAndContext.canvas = null;
    canvasAndContext.context = null;
  },
};

var pdfjsLib = require("./pdf.js");

// Some PDFs need external cmaps.
var CMAP_URL = "./cmaps/";
var CMAP_PACKED = true;

// Loading file from file system into typed array.
var pdfPath =
  process.argv[2] || "../../../web/compressed.tracemonkey-pldi-09.pdf";
var data = new Uint8Array(fs.readFileSync(pdfPath));

// Load the PDF file.
var loadingTask = pdfjsLib.getDocument({
  data,
  cMapUrl: CMAP_URL,
  cMapPacked: CMAP_PACKED,
});
loadingTask.promise
  .then(function (pdfDocument) {
    console.log("# PDF document loaded.");

    // Get the first page.
    pdfDocument.getPage(1).then(function (page) {
      // Render the page on a Node canvas with 100% scale.
      var viewport = page.getViewport({ scale: 1.0 });
      var canvasFactory = new NodeCanvasFactory();
      var canvasAndContext = canvasFactory.create(
        viewport.width,
        viewport.height
      );
      var renderContext = {
        canvasContext: canvasAndContext.context,
        viewport,
        canvasFactory,
      };

      var renderTask = page.render(renderContext);
      renderTask.promise.then(function () {
        // Convert the canvas to an image buffer.
        var image = canvasAndContext.canvas.toBuffer();
        fs.writeFile("output.png", image, function (error) {
          if (error) {
            console.error("Error: " + error);
          } else {
            console.log(
              "Finished converting first page of PDF file to a PNG image."
            );
          }
        });
      });
    });
  })
  .catch(function (reason) {
    console.log(reason);
  });