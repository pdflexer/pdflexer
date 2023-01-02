using Microsoft.Extensions.Logging;
using PDFiumCore;
using PdfLexer;
using PdfLexer.DOM;
using System.CommandLine;

namespace pdflexer.TestCaseGen;

public class RunTextCmd
{
    public string[] InputPath { get; set; } = null!;
    public string OutputPath { get; set; } = null!;

    public static Command CreateCommand()
    {
        var cmd = new Command("txt", "Runs txt sampling.")
            {
                new Option<string[]>(new[] {"-i", "--input-path"})
                {
                    IsRequired = true,
                    Description = "Directory containing pdfs to sample."
                },
                new Option<string>(new[] {"-o", "--output-path"})
                {
                    IsRequired = true,
                    Description = "Storage for test artifacts."
                }
            };
        return cmd;
    }
}

internal class TextSampler
{
    private ILogger<ImageSampler> _logger;

    public TextSampler(ILogger<ImageSampler> logger)
    {
        _logger = logger;

        fpdfview.FPDF_InitLibrary();
    }

    public void Run(RunTextCmd cmd)
    {
        var hashes = new HashSet<string>();

        foreach (var loc in cmd.InputPath)
        {
            foreach (var pdf in Directory.GetFiles(loc, "*.pdf"))
            {
                var name = Path.GetFileNameWithoutExtension(pdf);


                var ser = new PdfLexer.Serializers.Serializers();
                var types = new List<string>();
                try
                {
                    var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
                    if (doc.Trailer.ContainsKey(PdfName.Encrypt))
                    {
                        // don't support encryption currently
                        continue;
                    }

                    var read = new HashSet<PdfDictionary>();

                    int i = 0;
                    foreach (var page in doc.Pages)
                    {
                        foreach (var font in ReadFonts(page))
                        {
                            if (read.Contains(font))
                            {
                                continue;
                            }
                            read.Add(font);

                            using var md5 = System.Security.Cryptography.MD5.Create();

                            var ms = new MemoryStream();

                            void WriteDirectly(Stream str, PdfIndirectRef ir)
                            {
                                var obj = ir.Resolve();
                                ser.SerializeObject(str, obj, WriteDirectly);
                            }

                            void WriteIfNotNull(IPdfObject? obj)
                            {
                                if (obj == null) { return; }
                                ser.SerializeObject(ms, obj, WriteDirectly);
                                ms.WriteByte((byte)'\n');
                            }

                            void WriteKVPIfNotNull(PdfDictionary dict, PdfName key)
                            {
                                var val = dict.Get(key);
                                if (val != null)
                                {
                                    WriteIfNotNull(key);
                                    WriteIfNotNull(val);
                                }
                            }

                            void WriteEncoding(IPdfObject? obj)
                            {
                                if (obj == null)
                                {
                                    WriteIfNotNull(PdfName.Encoding);
                                    WriteIfNotNull(PdfNull.Value);
                                    return;
                                }
                                var et = obj.GetPdfObjType();
                                if (et == PdfObjectType.NameObj)
                                {
                                    WriteIfNotNull(PdfName.Encoding);
                                    WriteIfNotNull(obj);
                                }
                                else if (et == PdfObjectType.DictionaryObj)
                                {
                                    var etd = obj.GetAs<PdfDictionary>();
                                    var be = etd.Get(PdfName.BaseEncoding);
                                    WriteIfNotNull(PdfName.BaseEncoding);
                                    WriteIfNotNull(be);
                                }
                            }

                            void WriteDescriptor(PdfDictionary? obj)
                            {
                                if (obj == null)
                                {
                                    WriteIfNotNull(PdfName.FontDescriptor);
                                    WriteIfNotNull(PdfNull.Value);
                                    return;
                                }

                                WriteIfNotNull(PdfName.FontDescriptor);
                                var domFd = (FontDescriptor)obj;
                                if (domFd.FontFile != null)
                                {
                                    WriteIfNotNull(PdfName.FontFile);
                                }
                                if (domFd.FontFile2 != null)
                                {
                                    WriteIfNotNull(PdfName.FontFile2);
                                }
                                if (domFd.FontFile3 != null)
                                {
                                    WriteIfNotNull(PdfName.FontFile3);
                                }
                                if (domFd.MissingWidth != null)
                                {
                                    WriteIfNotNull(PdfName.MissingWidth);
                                }
                            }

                            var type = font.Get<PdfName>(PdfName.Subtype);
                            if (type == null)
                            {
                                continue;
                            }
                            WriteIfNotNull(font.Get<PdfName>(PdfName.TYPE));
                            WriteIfNotNull(font.Get<PdfName>(PdfName.Subtype));
                            switch (type.Value)
                            {
                                case "Type3":
                                case "Type1":
                                case "TrueType":
                                    {
                                        var t1 = (FontType1)font;
                                        WriteEncoding(t1.Encoding);
                                        if (t1.ToUnicode != null)
                                        {
                                            WriteIfNotNull(PdfName.ToUnicode);
                                        }
                                        if (t1.Widths != null)
                                        {
                                            WriteIfNotNull(PdfName.Widths);
                                        }
                                        WriteDescriptor(t1.FontDescriptor?.NativeObject);
                                    }
                                    break;
                                case "Type0":
                                    {
                                        var t0 = (FontType0)font;
                                        WriteEncoding(t0.Encoding);
                                        if (t0.Encoding != null)
                                        {
                                            var mt = t0.Encoding.GetPdfObjType();
                                            if (mt == PdfObjectType.NameObj)
                                            {
                                                WriteIfNotNull(PdfName.Encoding);
                                                WriteIfNotNull(t0.Encoding);
                                            } else
                                            {
                                                WriteIfNotNull(PdfName.Encoding);
                                                WriteIfNotNull(new PdfName("CMap"));
                                            }
                                        } else
                                        {
                                            WriteIfNotNull(PdfName.Encoding);
                                            WriteIfNotNull(PdfNull.Value);
                                        }
                                        if (t0.ToUnicode != null)
                                        {
                                            WriteIfNotNull(PdfName.ToUnicode);
                                        }
                                        if (t0.DescendantFont != null)
                                        {
                                            var cid = t0.DescendantFont;
                                            WriteKVPIfNotNull(cid, PdfName.DW);
                                            WriteKVPIfNotNull(cid, PdfName.DW2);
                                            if (cid.W != null)
                                            {
                                                WriteIfNotNull(PdfName.W);
                                            }
                                            if (cid.W2 != null)
                                            {
                                                WriteIfNotNull(PdfName.W2);
                                            }
                                            WriteIfNotNull(cid.CIDSystemInfo?.NativeObject);
                                            if (cid.CIDToGIDMap != null)
                                            {
                                                var mt = cid.CIDToGIDMap.GetPdfObjType();
                                                if (mt == PdfObjectType.NameObj)
                                                {
                                                    WriteIfNotNull(PdfName.CIDToGIDMap);
                                                    WriteIfNotNull(cid.CIDToGIDMap);
                                                } else
                                                {
                                                    WriteIfNotNull(PdfName.CIDToGIDMap);
                                                    WriteIfNotNull(new PdfName("CMap"));
                                                }
                                            }
                                            WriteDescriptor(cid.FontDescriptor);
                                        } else
                                        {
                                            WriteIfNotNull(PdfName.CIDFontType0);
                                            WriteIfNotNull(PdfNull.Value);
                                        }
                                    }
                                    break;
                            }

                            var data = ms.ToArray();
                            var hash = Convert.ToBase64String(md5.ComputeHash(data));
                            if (hashes.Contains(hash))
                            {
                                continue;
                            }
                            hashes.Add(hash);

                            hash = hash.TrimEnd('=').Replace('/', '_').Replace('+', '-');
                            var infoFile = Path.Combine(cmd.OutputPath, hash + ".txt");
                            File.WriteAllBytes(infoFile, data);
                            using var fw = File.AppendText(infoFile);
                            fw.WriteLine($"{Path.GetFileName(pdf)}");
                        }

                    }
                }
                catch (NotSupportedException ex)
                {
                    // for compressed object streams
                    if (ex.Message.Contains("encryption"))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[{PdfName}] Failure: {Message}", name, ex.Message);
                }
            }
        }

    }

    private IEnumerable<PdfDictionary> ReadFonts(PdfDictionary pageOrForm)
    {
        var resources = pageOrForm.Get<PdfDictionary>(PdfName.Resources);
        if (resources == null) { yield break; }
        var included = resources.Get<PdfDictionary>(PdfName.Font);
        if (included != null)
        {
            foreach (var value in included.Values)
            {
                var fnt = value.GetAs<PdfDictionary>();
                if (fnt != null)
                {
                    yield return fnt;
                }
            }
        }
        var xobjs = resources.Get<PdfDictionary>(PdfName.XObject);
        if (xobjs == null) { yield break; }
        foreach (var item in xobjs.Values)
        {
            var xobj = item.Resolve();
            if (xobj.Type != PdfObjectType.DictionaryObj)
            {
                continue;
            }
            var dict = xobj.GetAs<PdfDictionary>();
            var st = dict.Get<PdfName>(PdfName.Subtype);
            if (st == PdfName.Form)
            {
                foreach (var font in ReadFonts(dict))
                {
                    yield return font;
                }
            }
        }
    }
}
