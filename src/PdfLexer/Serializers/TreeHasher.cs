using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Serializers
{
    internal class TreeHasher
    {
        private readonly Serializers Serializers;
        private readonly RefTracker Tracker;
        private readonly MemoryStream Stream = new MemoryStream();

        public TreeHasher()
        {
            Serializers = new Serializers();
            Tracker = new RefTracker();
        }

        private List<PdfIndirectRef> Objs = new List<PdfIndirectRef>();
        public PdfStreamHash GetHash(IPdfObject obj)
        {
            Objs.Clear();
            Tracker.Reset();

            if (obj.Type != PdfObjectType.IndirectRefObj)
            {
                obj = PdfIndirectRef.Create(obj);
            }

            CommonUtil.Recurse(obj, new HashSet<PdfIndirectRef>(), x => false, (x, ir) =>
              {
                  if (ir != null)
                  {
                      Tracker.Localize(ir);
                      Objs.Add(ir);
                  }
              }, true);


            Stream.SetLength(0);
            Stream.Position = 0;
            foreach (var item in Objs.OrderBy(x => x.Reference.ObjectNumber))
            {
                Serializers.SerializeObject(Stream, item.GetObject(), (s) =>
                {
                    if (Tracker.TryGetLocalRef(s, out var ns, false))
                    {
                        return ns;
                    }
                    throw new ApplicationException("Unlocalized IR during hashing.");
                });
            }

            return new PdfStreamHash(Stream);

            // using (var md5 = MD5.Create())
            // using (CryptoStream cs = new CryptoStream(Stream.Null, md5, CryptoStreamMode.Write))
            // {
            //     foreach (var item in Objs.OrderBy(x=>x.Reference.ObjectNumber))
            //     {
            //         Serializers.SerializeObject(cs, item.GetObject(), (s) =>
            //         {
            //             if (Tracker.TryGetLocalRef(s, out var ns, false))
            //             {
            //                 return ns;
            //             }
            //             throw new ApplicationException("Unlocalized IR during hashing.");
            //         });
            //     }
            //     cs.FlushFinalBlock();
            //     return Convert.ToBase64String(md5.Hash);
            // }
        }

        
    }
}

class FNVStreamComparison : IEqualityComparer<PdfStreamHash>
{
    public bool Equals(PdfStreamHash x, PdfStreamHash y)
    {
        if (x.Stream.Length != y.Stream.Length)
        {
            return false;
        }

        x.Stream.Seek(0, SeekOrigin.Begin);
        y.Stream.Seek(0, SeekOrigin.Begin);
        
        for (var i = 0; i < x.Stream.Length; i++)
        {
            var a = x.Stream.ReadByte();
            var b = y.Stream.ReadByte();
            if (a != b)
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(PdfStreamHash obj) => obj.Hash;
}

struct PdfStreamHash
{
    public PdfStreamHash(Stream stream)
    {
        Stream = stream;
        Stream.Seek(0, SeekOrigin.Begin);
        var hash = FnvHash.Create();
        int val;
        while ((val = Stream.ReadByte()) != -1)
        {
            hash.Combine((byte)val);
        }
        Hash = hash.HashCode;
    }
    public Stream Stream { get; internal set; }
    public int Hash { get; }
}

/// <summary>
/// A hash combiner that is implemented with the Fowler/Noll/Vo algorithm (FNV-1a). This is a mutable struct for performance reasons.
/// </summary>
struct FnvHash
{
    /// <summary>
    /// The starting point of the FNV hash.
    /// </summary>
    public const int Offset = unchecked((int)2166136261);

    /// <summary>
    /// The prime number used to compute the FNV hash.
    /// </summary>
    private const int Prime = 16777619;

    /// <summary>
    /// Gets the current result of the hash function.
    /// </summary>
    public int HashCode { get; private set; }

    /// <summary>
    /// Creates a new FNV hash initialized to <see cref="Offset"/>.
    /// </summary>
    public static FnvHash Create()
    {
        var result = new FnvHash();
        result.HashCode = Offset;
        return result;
    }

    /// <summary>
    /// Adds the specified byte to the hash.
    /// </summary>
    /// <param name="data">The byte to hash.</param>
    public void Combine(byte data)
    {
        unchecked
        {
            HashCode ^= data;
            HashCode *= Prime;
        }
    }

    /// <summary>
    /// Adds the specified integer to this hash, in little-endian order.
    /// </summary>
    /// <param name="data">The integer to hash.</param>
    public void Combine(int data)
    {
        Combine(unchecked((byte)data));
        Combine(unchecked((byte)(data >> 8)));
        Combine(unchecked((byte)(data >> 16)));
        Combine(unchecked((byte)(data >> 24)));
    }
}
