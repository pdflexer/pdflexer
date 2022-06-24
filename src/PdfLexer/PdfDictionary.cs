using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfLexer
{
    /// <summary>
    /// Pdf dictionary object.
    /// </summary>
    public class PdfDictionary : PdfObject, IDictionary<PdfName, IPdfObject>
    {
        internal static readonly byte[] start = new byte[2] {(byte) '<', (byte) '<'};
        internal static readonly byte[] end = new byte[2] {(byte) '>', (byte) '>'};

        public override PdfObjectType Type => PdfObjectType.DictionaryObj;

        internal readonly IDictionary<PdfName, IPdfObject> _dictionary;
        internal bool DictionaryModified { get;set; }
        internal PdfLazyObject LazyWrapper { get;set; }
        
        /// <summary>
        /// Creates a new empty PDF dictionary.
        /// </summary>
        public PdfDictionary()
        {
            DictionaryModified = true;
            _dictionary = new Dictionary<PdfName, IPdfObject>();
        }

        internal PdfDictionary(IEnumerable<KeyValuePair<PdfName, IPdfObject>> items)
        {
            _dictionary = new Dictionary<PdfName, IPdfObject>(items);
        }

        /// <summary>
        /// Creates a new dictionary with the same contents.
        /// </summary>
        /// <returns></returns>
        public PdfDictionary CloneShallow()
        {
            return new PdfDictionary(_dictionary);
        }


        public IEnumerator<KeyValuePair<PdfName, IPdfObject>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<PdfName, IPdfObject> item)
        {
            DictionaryModified = true;
            _dictionary.Add(item);
        }

        public void Clear()
        {
            DictionaryModified = true;
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<PdfName, IPdfObject> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<PdfName, IPdfObject>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<PdfName, IPdfObject> item)
        {
            DictionaryModified = true;
            return _dictionary.Remove(item);
        }

        public int Count => _dictionary.Count;
        public bool IsReadOnly => _dictionary.IsReadOnly;
        public void Add(PdfName key, IPdfObject value)
        {
            DictionaryModified = true;
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(PdfName key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(PdfName key)
        {
            var result = _dictionary.Remove(key);
            if (result)
            {
                DictionaryModified = true;
            }

            return result;
        }

        public bool TryGetValue(PdfName key, out IPdfObject value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public T GetRequiredValue<T>(PdfName key) where T : IPdfObject
        {
            if (!TryGetValue<T>(key, out T value))
            {
                throw new ApplicationException($"Required value not present in dictionary for key {key.Value}, available keys: " 
                    + string.Join(", ", Keys.Select(x=>x.Value)));
            }
            return value;
        }

        public T GetOrCreateValue<T>(PdfName key) where T : IPdfObject, new()
        {
            if (!TryGetValue<T>(key, out T value))
            {
                var val = new T();
                this.Add(key, val);
                return val;
            }
            return value;
        }

        public T GetOptionalValue<T>(PdfName key) where T : IPdfObject
        {
            _ = TryGetValue<T>(key, out T value);
            return value;
        }

        public bool TryGetValue<T>(PdfName key, out T value, bool errorOnMismatch=true) where T : IPdfObject
        {
            if (!_dictionary.TryGetValue(key, out var item))
            {
                value = default(T);
                return false;
            }

            item = item.Resolve();

            if (item is T retyped)
            {
                value = retyped;
                return true;
            }

            if (errorOnMismatch)
            {
                throw new ApplicationException($"Unexpected data type in dictionary for key {key.Value}, got {item.Type} expected {typeof(T)}");
            }
            value = default(T);
            return false;
        }

        public IPdfObject this[PdfName key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<PdfName> Keys => _dictionary.Keys;
        public ICollection<IPdfObject> Values => _dictionary.Values;

        public override bool IsModified
        {
            get
            {
                if (DictionaryModified)
                {
                    return true;
                }

                foreach (var item in Values)
                {
                    if (item.IsModified)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
