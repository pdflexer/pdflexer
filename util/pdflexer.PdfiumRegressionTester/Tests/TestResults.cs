using PdfLexer;
using PdfLexer.Content;
using PdfLexer.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pdflexer.PdfiumRegressionTester.Tests;

internal class TestResults : IDisposable
{
    private FileStream lo; 
    private FileStream er;
    public TestResults(string namePrefix)
    {
        // lo = File.OpenWrite(namePrefix + ".log");
        // lo.Seek(0, SeekOrigin.End);
        // er = File.OpenWrite(namePrefix + ".err.jsonl");
        // er.Seek(0, SeekOrigin.End);
    }

    public void Dispose()
    {
        // lo.Dispose();
        // er.Dispose();
    }

    public void Log(string msg)
    {
        // lo.Write(Encoding.UTF8.GetBytes(msg));
        // lo.WriteByte((byte)'\n');
        // lo.Flush();
    }

    public void WriteError(ErrInfo err)
    {
        // lo.Write(JsonSerializer.SerializeToUtf8Bytes(err));
        // lo.WriteByte((byte)'\n');
        // lo.Flush();
    }
}


internal interface ITest
{
    (List<string>, int) RunTest(string inputPath, string outputPath);
}

public enum RunResult
{
    NewTest,
    Regression,
    MatchErrorIncrease,
    Improvement,
    Match
}

internal class TestRunner
{
    private Dictionary<string, ErrInfo> existing;

    public TestRunner(Dictionary<string, ErrInfo> errorInfo)
    {
        existing = errorInfo;
    }

    public (RunResult Status, ErrInfo Info, string Message) RunTest(ITest test, string filePath, string output)
    {
        var nm = Path.GetFileName(filePath);
        
        ErrInfo result;

        try
        {
            result = GetTestResults(test, filePath, output);
        } catch (Exception ex)
        {
            result = new ErrInfo
            {
                PdfName = nm,
                Status = TestStatus.PdfLexerError,
                FailureMsg = ex.Message
            };
        }
         
        if (!existing.TryGetValue(nm, out var ei))
        {
            // new file
            return (RunResult.NewTest, result, "");
        }

        if (ei.Failure)
        {
            ei.Status = TestStatus.PdfLexerError;
        }
        if (ei.FailedPages != null && ei.FailedPages.Count > 0  && ei.Status == TestStatus.Match)
        {
            ei.Status = TestStatus.Differences;
        }

        if (ei.Status == result.Status)
        {
            ei.FailedPages ??= new List<int>();
            result.FailedPages ??= new List<int>();
            if (!ei.FailedPages.SequenceEqual(result.FailedPages))
            {
                if (result.ErrCount > ei.ErrCount)
                {
                    return (RunResult.Regression, result, "Failed pages and err count increased.");
                }
                if (result.FailedPages.Count < ei.FailedPages.Count)
                {
                    return (RunResult.Improvement, result, "Failed pages reduced.");
                }
                return (RunResult.Regression, result, "Failed pages changed.");
            }

            if (result.ErrCount > ei.ErrCount)
            {
                return (RunResult.MatchErrorIncrease, result, "Error count increased");
            } else if (result.ErrCount < ei.ErrCount)
            {
                return (RunResult.Improvement, result, "Error count decreased");
            }

            return (RunResult.Match, result, "");
        }

        if (result.Status > ei.Status)
        {
            return (RunResult.Improvement, result, $"Status to {result.Status} from {ei.Status}");
        } else if (result.Status < ei.Status)
        {
            return (RunResult.Regression, result, $"Status to {result.Status} from {ei.Status}");
        }

        return (RunResult.Match, result, "");
    }

    private ErrInfo GetTestResults(ITest test, string filePath, string output)
    {
        var results = new TestResults("");
        var nm = Path.GetFileName(filePath);
        var outputPdf = Path.Combine(output, nm);

        var thisRun = new ErrInfo { PdfName = nm };

        try
        {
            var (errs, errcnt) = test.RunTest(filePath, outputPdf);
            thisRun.Errs = errs;
            thisRun.ErrCount = errcnt;
        }
        catch (PdfLexerPasswordException)
        {
            thisRun.Status = TestStatus.PdfLexerSkip;
            thisRun.FailureMsg = "PdfLexerPasswordException";
            return thisRun;
        }
        catch (NotSupportedException ex)
        {
            // for compressed object streams
            if (ex.Message.Contains("encryption"))
            {
                thisRun.Status = TestStatus.PdfLexerSkip;
                thisRun.FailureMsg = "NotSupportedException - encryption";
            } else
            {
                thisRun.Status = TestStatus.PdfLexerError;
                thisRun.FailureMsg = ex.Message;
            }
            
            return thisRun;
        }
        catch (Exception ex)
        {
            thisRun.Status = TestStatus.PdfLexerError;
            thisRun.FailureMsg = ex.Message;
            return thisRun;
        }


        thisRun.Errs ??= new List<string>();

        bool changes = false;

        
        using var fb = PdfDocument.Open(File.ReadAllBytes(filePath));
        using var fc = PdfDocument.Open(File.ReadAllBytes(outputPdf));

        var comparer = new Compare(Path.Combine(output, Path.GetFileNameWithoutExtension(filePath)), 1);
        var pgs = comparer.CompareAllPages(filePath, outputPdf);
        var changedpages = new List<int>();
        for (var i = 0; i < pgs.Count; i++)
        {
            var pg = pgs[i];
            if (pg.HadChanges)
            {
                if (pg.Type == ChangeType.ErrorBaseline)
                {
                    thisRun.Status = TestStatus.PdfiumError; 
                    results.Log($"[{nm}] pdfium failed to open pg on baseline{i}");
                }
                else
                {
                    changes = true;
                    changedpages.Add(i);
                    results.Log($"[{nm}] failed pg {i} -> {pg.Type.ToString()}");


                    // pdfium seems slow to unlock
                    var start = DateTime.UtcNow;
                    while (DateTime.UtcNow - start < TimeSpan.FromSeconds(5))
                    {
                        try
                        {
                            using (Stream stream = new FileStream(filePath, FileMode.Open)) { }
                            using (Stream stream = new FileStream(outputPdf, FileMode.Open)) { }
                            break;
                        }
                        catch { Thread.Sleep(25); }
                    }


                    var bn = Path.GetFileNameWithoutExtension(filePath);
                    using var fco = File.Create(Path.Combine(output, $"{bn}_c{i}.txt"));

                    DumpPageContent(fc, i, fco);
                    using var fbo = File.Create(Path.Combine(output, $"{bn}_b{i}.txt"));

                    DumpPageContent(fb, i, fbo);
                    using var fbr = File.Create(Path.Combine(output, $"{bn}_b{i}_raw.txt"));
                    DumpRawPageContent(fb, i, fbr);
                }
            }
        }

        if (thisRun.Status != TestStatus.PdfiumError)
        {
            thisRun.Status = changes ? TestStatus.Differences : TestStatus.Match;
        }
        thisRun.FailedPages = changedpages;
        return thisRun;
    }

    void DumpPageContent(PdfDocument doc, int pg, Stream output)
    {
        if (doc.Pages.Count <= pg) { return; }
        // using var str = doc.Pages[pg].Contents.First().Contents.GetDecodedStream();
        // str.CopyTo(output);
        // return;
        var scanner = new PageContentScanner(doc.Context, doc.Pages[pg]);
        while (scanner.Advance())
        {
            var data = scanner.GetCurrentData();
            output.Write(data);
            output.WriteByte((byte)'\n');
        }
    }

    void DumpRawPageContent(PdfDocument doc, int pg, Stream output)
    {
        if (doc.Pages == null || doc.Pages.Count <= pg) { return; }
        var page = doc.Pages[pg];
        var contents = page.NativeObject.Get(PdfName.Contents)?.Resolve();
        switch (contents)
        {
            case PdfArray arr:
                foreach (var item in arr)
                {
                    var str = item.GetValueOrNull<PdfStream>();
                    if (str != null)
                    {
                        using var wo = str.Contents.GetDecodedStream();
                        wo.CopyTo(output);
                        output.WriteByte((byte)'\n');
                    }
                }
                break;
            case PdfStream stream:
                {
                    using var wo = stream.Contents.GetDecodedStream();
                    wo.CopyTo(output);
                    output.WriteByte((byte)'\n');
                }
                break;
        }
    }
}

