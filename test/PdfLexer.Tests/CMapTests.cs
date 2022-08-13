using PdfLexer.CMaps;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace PdfLexer.Tests
{
    public class CMapTests
    {

        //[Fact]
        public void It_Reads_Cmap()
        {
            var sb = new StringBuilder();
            foreach (var item in KnownCMaps.Names)
            {
                var map = KnownCMaps.GetCMap(item);
                var total = map.Mapping.Count();
                uint last = 0;
                if (total > 0)
                {
                    last = map.Mapping.Keys.Max();
                }
                sb.AppendLine($"{item} {total} / {last}");
            }

            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var fp = Path.Combine(tp, "results", "stats.txt");
            File.WriteAllText(fp, sb.ToString());
        }
    }
}
