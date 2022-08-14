using PdfLexer.Fonts;
using PdfLexer.Fonts.Files;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.TestCaseGen;

internal class GenStdGlyph
{
    public static Command CreateCommand()
    {
        var cmd = new Command("std-fonts", "Code gen for std fonts.")
        {
        };
        return cmd;
    }


    public static void Run()
    {
        var root = PathUtil.GetPathFromSegmentOfCurrent("pdflexer.TestCaseGen");
        var proj = PathUtil.GetPathFromSegmentOfCurrent("pdflexer");
        var fo = Path.Combine(proj, "src", "PdfLexer", "Fonts", "Predefined");
        var afm = Path.Combine(root, "AFM");
        foreach (var file in Directory.GetFiles(afm, "*.afm"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            using var fs = File.OpenRead(file);
            var all = new Dictionary<string, Glyph>();
            var def = new Glyph[256];

            var glyphs = AFMReader.GetGlyphsFromAFM(fs);
            
            var output = new StringBuilder();
            output.Append("using System.Collections.Generic;\n\n");
            output.Append("namespace PdfLexer.Fonts.Predefined {\n");
            output.Append($"\tinternal class {name}Glyphs {{\n");
            foreach (var g in glyphs)
            {
                if (g.CodePoint.HasValue)
                {
                    def[(int)g.CodePoint] = g;
                }
                output.Append($"\t\tstatic readonly Glyph {g.Name} = new () {{ Char = (char){(int)(g.Char)}, w0 = {g.w0}F, ");
                output.Append($"IsWordSpace = {g.IsWordSpace.ToString().ToLower()}, ");
				if (g.CodePoint != null) { output.Append($"CodePoint = {g.CodePoint}, "); }
                output.Append($"Name = \"{g.Name}\", ");
                output.Append($"Undefined = {g.Undefined.ToString().ToLower()}, ");
                output.Append($"BBox = new decimal[] {{ {g.BBox[0].ToString()}m, {g.BBox[1].ToString()}m, {g.BBox[2].ToString()}m, {g.BBox[3].ToString()}m }}");
                if (g.Kernings != null)
                {
                    output.Append($", Kernings = new Dictionary<char,float> {{ ");
                    foreach (var kvp in g.Kernings)
                    {
                        output.Append($" ['\\u{((int)kvp.Key):X04}'] = {kvp.Value}f,");
                    }
                    output.Append($" }} }};\n");
                } else
                {
                    output.Append("};\n");
                }
                
            }

            output.Append("\t\tpublic static Glyph[] AllGlyphs = new Glyph[] {\n");
            foreach (var g in glyphs)
            {
                output.Append($"\t\t\t{g.Name},\n");
            }
            output.Append("\t\t};\n");

            output.Append("\t\tpublic static Glyph?[] DefaultEncoding = new Glyph?[] {\n");
            for (var i = 0; i < def.Length; i++)
            {
                var g = def[i];
                if (g == null) { output.Append($"\t\t\tnull,\n"); continue; }
                output.Append($"\t\t\t{g.Name},\n");
            }
            output.Append("\t\t};\n");

            output.Append("\t};\n");
            output.Append("}\n");
            var str = output.ToString();
            File.WriteAllText(Path.Combine(fo, name + ".cs"), str);
        }
    }
}

public static class PathUtil
{
    public static string GetPathFromSegmentOfCurrent(string segment)
    {
        return GetPathFromSegment(segment, Environment.CurrentDirectory);
    }
    public static string GetPathFromSegment(string segment, string path)
    {
        var split = path.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        int index = Array.FindLastIndex(split, t => t.Equals(segment, StringComparison.InvariantCultureIgnoreCase));
        if (index == -1)
        {
            throw new FileNotFoundException("Folder to set relative to not found");
        }
        return string.Join(Path.DirectorySeparatorChar.ToString(), split.Take(index + 1));

    }
}
