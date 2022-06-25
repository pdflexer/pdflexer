using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer
{
    /// <summary>
    /// Pdf dictionary object.
    /// </summary>
    public class PdfDictionary : PdfObject, IDictionary<PdfName, IPdfObject>
    {
        internal static readonly byte[] start = new byte[2] {(byte) '<', (byte) '<'};
        internal static readonly byte[] end = new byte[2] {(byte) '>', (byte) '>'};
        internal readonly IDictionary<PdfName, IPdfObject> _dictionary;
        internal bool DictionaryModified { get; set; }
        internal PdfLazyObject LazyWrapper { get; set; }

        /// <inheritdoc/>
        public override PdfObjectType Type => PdfObjectType.DictionaryObj;

        
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

        internal PdfDictionary(int initCapacity)
        {
            _dictionary = new Dictionary<PdfName, IPdfObject>(initCapacity);
        }

        /// <summary>
        /// Creates a new dictionary with the same contents.
        /// </summary>
        /// <returns></returns>
        public PdfDictionary CloneShallow()
        {
            return new PdfDictionary(_dictionary);
        }


        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<PdfName, IPdfObject>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<PdfName, IPdfObject> item)
        {
            DictionaryModified = true;
            _dictionary.Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            DictionaryModified = true;
            _dictionary.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<PdfName, IPdfObject> item)
        {
            return _dictionary.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<PdfName, IPdfObject>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<PdfName, IPdfObject> item)
        {
            DictionaryModified = true;
            return _dictionary.Remove(item);
        }

        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => _dictionary.IsReadOnly;

        /// <inheritdoc/>
        public void Add(PdfName key, IPdfObject value)
        {
            DictionaryModified = true;
            _dictionary.Add(key, value);
        }

        /// <inheritdoc/>
        public bool ContainsKey(PdfName key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Remove(PdfName key)
        {
            var result = _dictionary.Remove(key);
            if (result)
            {
                DictionaryModified = true;
            }

            return result;
        }

        /// <inheritdoc/>
        public bool TryGetValue(PdfName key, out IPdfObject value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public T GetRequiredValue<T>(PdfName key) where T : IPdfObject
        {
            if (!TryGetValue<T>(key, out T value))
            {
                throw new PdfLexerException($"Required value not present in dictionary for key {key.Value}, available keys: " 
                    + string.Join(", ", Keys.Select(x=>x.Value)));
            }
            return value;
        }

        /// <summary>
        /// Gets value for key if exists, otherwise returns
        /// new object and sets value in dict.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns value for key if exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns>Result or null if not found</returns>
        public T GetOptionalValue<T>(PdfName key) where T : IPdfObject
        {
            _ = TryGetValue<T>(key, out T value);
            return value;
        }

        /// <summary>
        /// Returns value for key if exists and matches type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns>Result or null if not found or does not match</returns>
        public T Get<T>(PdfName key) where T : IPdfObject
        {
            _ = TryGetValue<T>(key, out T value, false);
            return value;
        }

        /// <summary>
        /// Returns value for key if exists and matches type.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Result or null if not found or does not match</returns>
        public IPdfObject Get(PdfName key)
        {
            _ = TryGetValue(key, out IPdfObject value, false);
            return value;
        }

        /// <summary>
        /// Returns value for key if exists and matches type.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Result or null if not found or does not match</returns>
        public IPdfObject GetRequiredValue(PdfName key)
        {
            if (!TryGetValue(key, out IPdfObject value, true))
            {
                throw new PdfLexerException($"Required value not present in dictionary for key {key.Value}, available keys: "
                    + string.Join(", ", Keys.Select(x => x.Value)));
            }
            return value;
        }

        /// <summary>
        /// Tries to get the PdfObject with the given key.
        /// </summary>
        /// <typeparam name="T">Expected type of object</typeparam>
        /// <param name="key">PdfName of dictionary key</param>
        /// <param name="value">Resulting PdfObject</param>
        /// <param name="errorOnMismatch">
        /// If true, exception thrown if key exists but object type is not <see cref="T"/>.
        /// If false, no exception thrown and false returned.
        /// </param>
        /// <returns>If object was found.</returns>
        /// <exception cref="PdfLexerObjectMismatchException"></exception>
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
                throw new PdfLexerObjectMismatchException($"Unexpected data type in dictionary for key {key.Value}, got {item.Type} expected {typeof(T)}");
            }
            value = default(T);
            return false;
        }

        /// <inheritdoc/>
        public IPdfObject this[PdfName key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        /// <inheritdoc/>
        public ICollection<PdfName> Keys => _dictionary.Keys;
        /// <inheritdoc/>
        public ICollection<IPdfObject> Values => _dictionary.Values;

        /// <inheritdoc/>
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
