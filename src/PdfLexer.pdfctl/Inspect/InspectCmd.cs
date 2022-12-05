using DotNext.Collections.Generic;
using System.CommandLine;
using System.Data;
using Terminal.Gui;
using Application = Terminal.Gui.Application;

namespace PdfLexer.pdfctl.Inspect;

internal class InspectCmd
{
    public string File { get; set; } = null;
    public static System.CommandLine.Command Create()
    {
        var cmd = new System.CommandLine.Command("inspect", "Inspects internal pdf structure")
        {
            new Option<string>(new[] {"-f", "--file"})
            {
                IsRequired = true,
                Description = "Path to pdf to inspect"
            }
        };
        return cmd;
    }

    public static int Handler(InspectCmd cmd)
    {
        var name = Path.GetFileName(cmd.File);

        using var pdf = PdfDocument.Open(cmd.File);

        Application.Init();


        List<PdfObject> stack = new List<PdfObject> { pdf.Trailer };
        List<string> names = new List<string> { "/Trailer" };

        var top = new Toplevel()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };


        var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_Close", "", () => {
                    Application.RequestStop ();
                })
            }),
        });

        var win = new Window(name)
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1
        };



        var dt = new DataTable();
        dt.Columns.Add("Key");
        dt.Columns.Add("Value");
        pdf.Trailer.ForEach(x => dt.Rows.Add(x.Key.Value, x.Value.ToString()));

        var label = new Label(string.Join("->", names))
        {
            X = 0,
            Y = 0,
            Height = 1,
        };


        var tv = new TableView()
        {
            X = 0,
            Y = 1,
            Width = win.Width,
            Height = win.Height - 1,
        };
        tv.KeyPress += (View.KeyEventEventArgs obj) =>
        {
            if (obj.KeyEvent.Key == Key.Esc)
            {
                obj.Handled = true;
                if (stack.Count < 2) { return; }
                stack.RemoveAt(stack.Count - 1);
                names.RemoveAt(names.Count - 1);
                var last = stack.Last();
                if (last.Type == PdfObjectType.ArrayObj)
                {
                    SetArray((PdfArray)last);
                }
                else if (last.Type == PdfObjectType.DictionaryObj)
                {
                    SetObj((PdfDictionary)last);
                }
                label.Text = string.Join("->", names);
                return;
            }
            if (obj.KeyEvent.Key != Key.Enter) { return; }

            var current = stack.Last();
            var nm = (string)dt.Rows[tv.SelectedRow].ItemArray[0];
            IPdfObject next;
            if (current.Type == PdfObjectType.ArrayObj)
            {
                var arr = (PdfArray)current;
                next = arr[int.Parse(nm)];
            }
            else if (current.Type == PdfObjectType.DictionaryObj)
            {
                var dict = (PdfDictionary)current;
                next = dict[nm];
            }
            else { return; }

            var t = next.GetPdfObjType();
            if (t == PdfObjectType.DictionaryObj)
            {
                var pdfDict = next.GetAs<PdfDictionary>();
                names.Add(nm);
                stack.Add(pdfDict);
                SetObj(pdfDict);
            }
            else if (t == PdfObjectType.ArrayObj)
            {
                var pdfArr = next.GetAs<PdfArray>();
                names.Add(nm);
                stack.Add(pdfArr);
                SetArray(pdfArr);
            }

        };

        void SetObj(PdfDictionary obj)
        {
            dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Value");
            obj.ForEach(x => dt.Rows.Add(x.Key.Value, x.Value.ToString()));
            tv.Table = dt;
            tv.SelectedRow = 0;
            tv.Update();
            label.Text = string.Join("->", names);
        }

        void SetArray(PdfArray obj)
        {
            dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Value");
            for (var i = 0; i < obj.Count; i++)
            {
                dt.Rows.Add(i.ToString(), obj[i].ToString());
            }
            tv.Table = dt;
            tv.SelectedRow = 0;
            tv.Update();
            label.Text = string.Join("->", names);
        }

        tv.Table = dt;
        win.Add(tv, label);

        // Add both menu and win in a single call
        top.Add(win, menu);
        Application.Run(top);

        Application.Shutdown();
        return 0;
    }
}
