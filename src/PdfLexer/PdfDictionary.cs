﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer
{
    public class PdfDictionary : IDictionary<PdfName, IPdfObject>, IParsedLazyObj
    {
        internal static readonly byte[] start = new byte[2] {(byte) '<', (byte) '<'};
        internal static readonly byte[] end = new byte[2] {(byte) '>', (byte) '>'};

        private readonly IDictionary<PdfName, IPdfObject> _dictionary;
        internal bool DictionaryModified { get;set; }
        internal PdfLazyObject LazyWrapper { get;set; }
        PdfLazyObject IParsedLazyObj.Wrapper => LazyWrapper;

        public bool IsIndirect { get; set; }

        public PdfDictionary()
        {
            DictionaryModified = true;
            _dictionary = new Dictionary<PdfName, IPdfObject>();
        }

        internal PdfDictionary(PdfLazyObject wrapper, IEnumerable<KeyValuePair<PdfName, IPdfObject>> items)
        {
            LazyWrapper = wrapper;
            _dictionary = new Dictionary<PdfName, IPdfObject>(items);
        }

        bool IParsedLazyObj.HasLazyIndirect
        {
            get
            {
                foreach (var item in _dictionary.Values)
                {
                    if (item is PdfLazyObject lz && lz.CheckForIndirect())
                    {
                        return true;
                    }
                }

                return false;
            }
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

        public bool TryGetValue<T>(PdfName key, out T value, bool errorOnMismatch=true) where T : IPdfObject
        {
            if (!_dictionary.TryGetValue(key, out var item))
            {
                value = default(T);
                return false;
            }

            if (item is T typed)
            {
                value = typed;
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

        public bool IsModified
        {
            get
            {
                if (DictionaryModified)
                {
                    return true;
                }

                foreach (var item in Values)
                {
                    if (item.IsModified())
                    {
                        return true;
                    }
                }

                return false;
            }
            set => DictionaryModified = value;
        }
        public PdfObjectType Type => PdfObjectType.DictionaryObj;
    }
}
