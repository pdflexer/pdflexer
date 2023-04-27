using PdfLexer.DOM;
using PdfLexer.Images;
using PdfLexer.Operators;
using PdfLexer.Tests;
using PdfLexer.Writing;
using SixLabors.ImageSharp;
using System.IO;
using Xunit;

namespace PdfLexer.ImageTests
{
    public class ImageInsertTests
    {
        [InlineData("0TxRvxWo5wUThisVd6EjFw.png")]
        [Theory]
        public void It_Inserts(string name)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var output = Path.Combine(tp, "results", "images");
            var pdfRoot = Path.Combine(tp, "imgs");
            RunSingle(pdfRoot, name, output, 10);
        }



        private void RunSingle(string root, string imgName, string output, int threshhold = 5)
        {
            Directory.CreateDirectory(output);
            var img = Path.Combine(root, imgName);
            using var cl = Image.Load(img);
            using var doc = PdfDocument.Create();
            var xobj = cl.CreatePdfImage(); 
            
            var pg = new PdfPage();
            doc.Pages.Add(pg);
            pg.AddXObj("Im1", xobj.Stream);
            var bx = pg.MediaBox;
            bx.URx = xobj.Width;
            bx.URy = xobj.Height;

            var wr = new ContentWriter(pg.Resources);
            wr.Raw(new cm_Op(0.1m, 0.2m, 0.3m, 0.4m, 0.5m, 0.6m))
              .Image(xobj, 0, 0, (decimal)xobj.Width, (decimal)xobj.Height);

            var content = wr.Complete();

            var cs = new MemoryStream();
            q_Op.WriteLn(cs);
            cm_Op.WriteLn(bx.URx, 0, 0, bx.URy, 0, 0, cs);
            Do_Op.WriteLn("Im1", cs);
            Q_Op.WriteLn(cs);
            var cnt = new PdfStream(new PdfDictionary(), content); // new PdfByteArrayStreamContents(cs.ToArray()));
            pg.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(cnt);
            var pdfOut = Path.Combine(output, Path.GetFileNameWithoutExtension(imgName) + "_ins.pdf");
            using (var fso = File.Create(pdfOut))
            {
                doc.SaveTo(fso);
            }
        }



    }
}
