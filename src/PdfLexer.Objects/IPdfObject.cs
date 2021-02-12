using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Objects
{
    public class PdfObject : IPdfObject
    {
        public bool IsIndirect { get; set; }
        public PdfObjectType Type { get; set; }
        public IPdfDataSource Source { get; set; }
        public IParsedPdfObject Parsed { get; set; }
    }

    public interface IParsedPdfObject
    {
        IPdfObject Wrapper { get; }
        bool IsModified { get; }
        void WriteObject(Stream stream);
    }

    public interface IPdfObject
    {
        public bool IsIndirect { get; }
        public PdfObjectType Type { get; }
        IPdfDataSource Source { get; }
        IParsedPdfObject Parsed { get; set; }
        void WriteObject(Stream stream)
        {
            if (Parsed?.IsModified ?? false)
            {
                Parsed.WriteObject(stream);
            }
            else
            {
                Source.CopyData(this, stream);
            }
        }
    }


    public class PdfDictionary : IDictionary<string, IPdfObject>, IPdfObject, IParsedPdfObject
    {
        static readonly byte[] start = new byte[2] {(byte) '<', (byte) '<'};
        static readonly byte[] end = new byte[2] {(byte) '>', (byte) '>'};

        private bool modified = false;
        private readonly IDictionary<string, IPdfObject> _dictionary;

        public IPdfObject Wrapper { get; }
        public IParsedPdfObject Parsed
        {
            get => this;
            set
            {
                throw new NotSupportedException();
            }
        }
        public IPdfDataSource Source { get; }

        public PdfDictionary()
        {
            modified = true;
            _dictionary = new Dictionary<string, IPdfObject>();
        }

        public PdfDictionary(IPdfObject data, IEnumerable<KeyValuePair<string, IPdfObject>> items)
        {
            Wrapper = data;
            Source = data.Source;
            _dictionary = new Dictionary<string, IPdfObject>(items);
        }
        public IEnumerator<KeyValuePair<string, IPdfObject>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, IPdfObject> item)
        {
            modified = true;
            _dictionary.Add(item);
        }

        public void Clear()
        {
            modified = true;
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, IPdfObject> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IPdfObject>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, IPdfObject> item)
        {
            modified = true;
            return _dictionary.Remove(item);
        }

        public int Count => _dictionary.Count;
        public bool IsReadOnly => _dictionary.IsReadOnly;
        public void Add(string key, IPdfObject value)
        {
            modified = true;
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            var result = _dictionary.Remove(key);
            if (result)
            {
                modified = true;
            }

            return result;
        }

        public bool TryGetValue(string key, out IPdfObject value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public IPdfObject this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<string> Keys => _dictionary.Keys;
        public ICollection<IPdfObject> Values => _dictionary.Values;

        public bool IsModified
        {
            get
            {
                if (modified)
                {
                    return true;
                }

                foreach (var item in Values)
                {
                    if (item.Parsed?.IsModified ?? false)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsIndirect { get; set; }

        public PdfObjectType Type => PdfObjectType.DictionaryObj;


        public void WriteObject(Stream stream)
        {
            if (!IsModified && Wrapper != null)
            {
                Wrapper.WriteObject(stream);
            }
            else
            {
                stream.Write(start);
                foreach (var item in this)
                {
                    stream.WriteByte((byte)'/');
                    stream.Write(Encoding.ASCII.GetBytes(item.Key));
                    if (item.Value.Type != PdfObjectType.NameObj
                        && item.Value.Type != PdfObjectType.ArrayObj
                        && item.Value.Type != PdfObjectType.DictionaryObj
                        && item.Value.Type != PdfObjectType.StreamObj)
                    {
                        stream.WriteByte((byte)' ');
                    }
                    item.Value.WriteObject(stream);
                }
                stream.Write(end);
            }
        }
    }
}
