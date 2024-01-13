using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PdfLexer.Tests;

internal static class SyntaxValidation
{
    public static List<string> Validate(byte[] pdf)
    {
        var tmp = Path.GetTempPath();
        var pdfPath = Path.Combine(tmp, Guid.NewGuid().ToString() + ".pdf");
        try
        {
            File.WriteAllBytes(pdfPath, pdf);
            return Validate(pdfPath);
        } finally
        {
            File.Delete(pdfPath);
        }

    }
    public static List<string> Validate(string pdfPath)
    {
        var errs = new List<string>();
        var cpu = Environment.GetEnvironmentVariable("PDFCPU_PATH");
        cpu ??= "pdfcpu";

        var fn = Path.GetFileName(pdfPath);

        var startInfo = new ProcessStartInfo();
        startInfo.WorkingDirectory = Path.GetDirectoryName(pdfPath);
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.FileName = cpu;
        startInfo.Arguments = $"validate -m strict \"{fn}\"";
        startInfo.UseShellExecute = false;
        Process pdfcpu = Process.Start(startInfo);
        pdfcpu.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null || string.IsNullOrWhiteSpace(e.Data)) { return; }
            lock (errs)
            {
                errs.Add("pdfcpu stderr: " + e.Data + Environment.NewLine);
            }

        };
        pdfcpu.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null || string.IsNullOrWhiteSpace(e.Data) || e.Data.Trim().StartsWith("validating") || e.Data.Contains("validation ok")) { return; }
            lock (errs)
            {
                errs.Add("pdfcpu stdout: " + e.Data + Environment.NewLine);
            }

        };
        pdfcpu.BeginErrorReadLine();
        pdfcpu.BeginOutputReadLine();

        pdfcpu.WaitForExit();
        if (pdfcpu.ExitCode != 0)
        {
            errs.Add("pdfcpu exit code: " + pdfcpu.ExitCode);
        }
        return errs;
    }

}
