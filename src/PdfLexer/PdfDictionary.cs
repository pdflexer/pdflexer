using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PdfLexer;

/// <summary>
/// Pdf dictionary object.
/// </summary>
public class PdfDictionary : PdfObject, IDictionary<PdfName, IPdfObject>
{
    internal static readonly byte[] start = new byte[2] {(byte) '<', (byte) '<'};
    internal static readonly byte[] end = new byte[2] {(byte) '>', (byte) '>'};
    internal readonly IDictionary<PdfName, IPdfObject> _dictionary;
    internal bool DictionaryModified { get; set; }
    internal bool IndirectOnly { get; set; }

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
        if (IndirectOnly && item.Value.Type != PdfObjectType.IndirectRefObj)
        {
            _dictionary.Add(new KeyValuePair<PdfName, IPdfObject>(item.Key, item.Value.Indirect()));
        } else
        {
            _dictionary.Add(item);
        }
        
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
        if (IndirectOnly)
        {
            _dictionary.Add(key, value.Indirect());
        } else
        {
            _dictionary.Add(key, value);
        }
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

    /// <summary>
    /// Tries to get the stored object without resolving indirect references.
    /// </summary>
    /// <param name="key">Dictionary key.</param>
    /// <param name="value">The stored object when found.</param>
    /// <returns><see langword="true"/> when the key exists; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(PdfName key, [NotNullWhen(true)]out IPdfObject? value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets and resolves a required typed value.
    /// </summary>
    /// <typeparam name="T">Expected resolved PDF object type.</typeparam>
    /// <param name="key">Dictionary key.</param>
    /// <returns>The resolved value.</returns>
    /// <exception cref="PdfLexerException">The key is absent or resolves to PDF null.</exception>
    /// <exception cref="PdfLexerObjectMismatchException">The resolved value has the wrong type.</exception>
    /// <remarks>
    /// When <typeparamref name="T"/> is <see cref="IPdfObject"/>, a PDF null is assignable to the requested
    /// type and is returned as <see cref="PdfNull"/>.
    /// </remarks>
    public T GetRequired<T>(PdfName key) where T : IPdfObject
    {
        if (!TryGetTyped(key, out T value, true))
        {
            throw MissingRequiredValue(key);
        }
        return value;
    }

    /// <summary>
    /// Compatibility alias for <see cref="GetRequired{T}(PdfName)"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetRequiredValue<T>(PdfName key) where T : IPdfObject
    {
        return GetRequired<T>(key);
    }

    /// <summary>
    /// Gets and resolves a typed value, or creates, stores, and returns a new value when the key is absent
    /// or resolves to PDF null.
    /// </summary>
    /// <typeparam name="T">Expected resolved PDF object type and type to create.</typeparam>
    /// <param name="key">Dictionary key.</param>
    /// <returns>The existing resolved value or the newly stored value.</returns>
    /// <exception cref="PdfLexerObjectMismatchException">The resolved existing value has the wrong type.</exception>
    public T GetOrCreateValue<T>(PdfName key) where T : IPdfObject, new()
    {
        if (!TryGetTyped(key, out T value, true))
        {
            var val = new T();
            Add(key, val);
            return val;
        }
        return value;
    }

    /// <summary>
    /// Gets and resolves a typed value, or stores and returns <paramref name="def"/> when the key is absent
    /// or resolves to PDF null.
    /// </summary>
    /// <typeparam name="T">Expected resolved PDF object type.</typeparam>
    /// <param name="key">Dictionary key.</param>
    /// <param name="def">Value to store when no value is present.</param>
    /// <returns>The existing resolved value or <paramref name="def"/>.</returns>
    /// <exception cref="PdfLexerObjectMismatchException">The resolved existing value has the wrong type.</exception>
    public T GetOrSetValue<T>(PdfName key, T def) where T : IPdfObject
    {
        if (!TryGetTyped(key, out T value, true))
        {
            Add(key, def);
            return def;
        }
        return value;
    }

    /// <summary>
    /// Preferred mismatch-tolerant optional typed accessor. Gets and resolves a typed value,
    /// returning <see langword="null"/> when the key is absent, resolves to PDF null, or has the wrong type.
    /// </summary>
    /// <typeparam name="T">Expected resolved PDF object type.</typeparam>
    /// <param name="key">Dictionary key.</param>
    /// <returns>The resolved value, or <see langword="null"/>.</returns>
    /// <remarks>
    /// When <typeparamref name="T"/> is <see cref="IPdfObject"/>, a PDF null is assignable to the requested
    /// type and is returned as <see cref="PdfNull"/>.
    /// </remarks>
    public T? Get<T>(PdfName key) where T : IPdfObject
    {
        _ = TryGetTyped(key, out T value, false);
        return value;
    }

    /// <summary>
    /// Gets and resolves an optional typed value.
    /// </summary>
    /// <typeparam name="T">Expected resolved PDF object type.</typeparam>
    /// <param name="key">Dictionary key.</param>
    /// <returns>The resolved value, or <see langword="null"/> when absent or resolved as PDF null.</returns>
    /// <exception cref="PdfLexerObjectMismatchException">The resolved value has the wrong type.</exception>
    /// <remarks>
    /// When <typeparamref name="T"/> is <see cref="IPdfObject"/>, a PDF null is assignable to the requested
    /// type and is returned as <see cref="PdfNull"/>.
    /// </remarks>
    public T? GetOptional<T>(PdfName key) where T : IPdfObject
    {
        _ = TryGetTyped(key, out T value, true);
        return value;
    }

    /// <summary>
    /// Compatibility alias for <see cref="GetOptional{T}(PdfName)"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T? GetOptionalValue<T>(PdfName key) where T : IPdfObject => GetOptional<T>(key);

    /// <summary>
    /// Sets a value, or removes the key when <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="key">Dictionary key.</param>
    /// <param name="value">Value to store, or <see langword="null"/> to remove the key.</param>
    /// <remarks>
    /// Assignment marks the dictionary modified and stores an indirect reference when this dictionary requires
    /// indirect-only values. Removing a missing key is a no-op and does not mark the dictionary modified.
    /// </remarks>
    public void Set(PdfName key, IPdfObject? value)
    {
        if (value == null)
        {
            Remove(key);
            return;
        }
        this[key] = value;
    }

    /// <summary>
    /// Gets and resolves the value for a key.
    /// </summary>
    /// <param name="key">Dictionary key.</param>
    /// <returns>The resolved value, or <see langword="null"/> when the key is absent.</returns>
    /// <remarks>
    /// Unlike the indexer and raw <see cref="TryGetValue(PdfName, out IPdfObject)"/>, this method resolves
    /// indirect references. A stored or resolved <see cref="PdfNull"/> is returned as a PDF object.
    /// </remarks>
    public IPdfObject? Get(PdfName key)
    {
        _ = TryGetTyped(key, out IPdfObject value, false);
        return value;
    }

    /// <summary>
    /// Gets and resolves a required value.
    /// </summary>
    /// <param name="key">Dictionary key.</param>
    /// <returns>The resolved value.</returns>
    /// <exception cref="PdfLexerException">The key is absent.</exception>
    /// <remarks>A resolved <see cref="PdfNull"/> is returned as a PDF object.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IPdfObject GetRequiredValue(PdfName key)
    {
        if (!TryGetTyped(key, out IPdfObject value, true))
        {
            throw MissingRequiredValue(key);
        }
        return value;
    }

    /// <summary>
    /// Tries to get and resolve a typed value without throwing for a type mismatch.
    /// </summary>
    /// <typeparam name="T">Expected resolved PDF object type.</typeparam>
    /// <param name="key">Dictionary key.</param>
    /// <param name="value">The resolved value when found with the expected type.</param>
    /// <returns>
    /// <see langword="true"/> when the key resolves to <typeparamref name="T"/>; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// When <typeparamref name="T"/> is <see cref="IPdfObject"/>, a PDF null is assignable to the requested
    /// type and is returned successfully as <see cref="PdfNull"/>.
    /// </remarks>
    public bool TryGet<T>(PdfName key, [NotNullWhen(true)] out T value) where T : IPdfObject =>
        TryGetTyped(key, out value, false);

    /// <summary>
    /// Compatibility typed try-get accessor with configurable mismatch behavior.
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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool TryGetValue<T>(PdfName key, [NotNullWhen(true)] out T value, bool errorOnMismatch=true) where T : IPdfObject
        => TryGetTyped(key, out value, errorOnMismatch);

    private bool TryGetTyped<T>(PdfName key, [NotNullWhen(true)] out T value, bool errorOnMismatch) where T : IPdfObject
    {
        if (!_dictionary.TryGetValue(key, out var item))
        {
            value = default!;
            return false;
        }

        item = item.Resolve();

        if (item is T retyped)
        {
            value = retyped;
            return true;
        }

        value = default!;

        if (item?.Type == PdfObjectType.NullObj)
        {
            return false;
        }

        if (errorOnMismatch)
        {
            throw new PdfLexerObjectMismatchException($"Unexpected data type in dictionary for key {key.Value}, got {item?.Type} expected {typeof(T)}");
        }
        
        return false;
    }

    private PdfLexerException MissingRequiredValue(PdfName key) =>
        new($"Required value not present in dictionary for key {key.Value}, available keys: "
            + string.Join(", ", Keys.Select(x => x.Value)));

    /// <summary>
    /// Gets or sets the stored object without resolving it.
    /// </summary>
    /// <param name="key">Dictionary key.</param>
    /// <returns>The stored object, which may be an indirect reference or <see cref="PdfNull"/>.</returns>
    /// <exception cref="KeyNotFoundException">The getter is used for a key that is not present.</exception>
    /// <remarks>
    /// Use <see cref="Get(PdfName)"/> for nullable resolved access. Assignment marks the dictionary modified
    /// and stores an indirect reference when this dictionary requires indirect-only values.
    /// </remarks>
    public IPdfObject this[PdfName key]
    {
        get => _dictionary[key];
        set {
            DictionaryModified = true;
            if (IndirectOnly)
            {
                _dictionary[key] = value.Indirect();
            } else
            {
                _dictionary[key] = value;
            }
         }
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

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("<<");
        foreach (var (key, value) in _dictionary)
        {
            sb.Append(key.ToString());
            sb.Append(' ');
            sb.Append(value.ToString());
            sb.Append(' ');
        }
        sb.Append(">>");
        return sb.ToString();
    }
}
