using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PdfLexer.Serializers
{
    internal class TreeHasher
    {
        private readonly Serializers Serializers;
        private readonly RefTracker Tracker;

        public TreeHasher()
        {
            Serializers = new Serializers();
            Tracker = new RefTracker();
        }

        private List<PdfIndirectRef> Objs = new List<PdfIndirectRef>();
        public string GetHash(IPdfObject obj)
        {
            Objs.Clear();
            Tracker.Reset();

            if (obj.Type != PdfObjectType.IndirectRefObj)
            {
                obj = PdfIndirectRef.Create(obj);
            }

            CommonUtil.Recurse(obj, new HashSet<PdfIndirectRef>(), x => false, (x, ir) =>
              {
                  // if (object.ReferenceEquals(x, obj)) { return; }
                  if (x.Type == PdfObjectType.IndirectRefObj)
                  {
                      var o = (PdfIndirectRef)x;
                      Tracker.Localize(o);
                      Objs.Add(o);
                  }
              }, true);

            //
            // var irf = (PdfIndirectRef)obj;
            // using var fs = File.Create(@"C:\source\Github\pdflexer\test\results\pighash\ref" + irf.Reference.ObjectNumber + ".txt");

            using (var md5 = MD5.Create())
            using (CryptoStream cs = new CryptoStream(Stream.Null, md5, CryptoStreamMode.Write))
            {
                foreach (var item in Objs.OrderBy(x=>x.Reference.ObjectNumber))
                {
                    Serializers.SerializeObject(cs, item.GetObject(), (s) =>
                    {
                        if (Tracker.TryGetLocalRef(s, out var ns, true))
                        {
                            return ns;
                        }
                        throw new ApplicationException("Unlocalized IR during hashing.");
                    });
                    // Serializers.SerializeObject(fs, item.GetObject(), (s) =>
                    // {
                    //     if (Tracker.TryGetLocalRef(s, out var ns))
                    //     {
                    //         return ns;
                    //     }
                    //     throw new ApplicationException("Unlocalized IR during hashing.");
                    // });
                }
                cs.FlushFinalBlock();
                return Convert.ToBase64String(md5.Hash);
            }
        }

        
    }
}
