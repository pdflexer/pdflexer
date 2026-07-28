using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xunit;

namespace PdfLexer.Tests;

public class DictionaryAccessTests
{
    private static readonly PdfName Key = (PdfName)"Key";

    [Fact]
    public void Canonical_typed_accessors_have_distinct_mismatch_semantics()
    {
        var dict = new PdfDictionary { [Key] = new PdfString("value") };

        Assert.Null(dict.Get<PdfDictionary>(Key));
        Assert.Throws<PdfLexerObjectMismatchException>(() => dict.GetOptional<PdfDictionary>(Key));
        Assert.Throws<PdfLexerObjectMismatchException>(() => dict.GetRequired<PdfDictionary>(Key));
        Assert.False(dict.TryGet<PdfDictionary>(Key, out var value));
        Assert.Null(value);
    }

    [Fact]
    public void Canonical_typed_accessors_distinguish_missing_and_pdf_null()
    {
        var missing = new PdfDictionary();
        var withNull = new PdfDictionary { [Key] = PdfNull.Value };

        Assert.Null(missing.Get<PdfDictionary>(Key));
        Assert.Null(missing.GetOptional<PdfDictionary>(Key));
        Assert.Throws<PdfLexerException>(() => missing.GetRequired<PdfDictionary>(Key));
        Assert.False(missing.TryGet<PdfDictionary>(Key, out _));

        Assert.Null(withNull.Get<PdfDictionary>(Key));
        Assert.Null(withNull.GetOptional<PdfDictionary>(Key));
        Assert.Throws<PdfLexerException>(() => withNull.GetRequired<PdfDictionary>(Key));
        Assert.False(withNull.TryGet<PdfDictionary>(Key, out _));
    }

    [Fact]
    public void Typed_access_resolves_indirect_values_while_indexer_and_raw_try_get_do_not()
    {
        var expected = new PdfDictionary();
        var reference = expected.Indirect();
        var dict = new PdfDictionary { [Key] = reference };

        Assert.Same(reference, dict[Key]);
        Assert.True(dict.TryGetValue(Key, out var raw));
        Assert.Same(reference, raw);
        Assert.Same(expected, dict.Get(Key));
        Assert.Same(expected, dict.Get<PdfDictionary>(Key));
        Assert.Same(expected, dict.GetOptional<PdfDictionary>(Key));
        Assert.Same(expected, dict.GetRequired<PdfDictionary>(Key));
        Assert.True(dict.TryGet<PdfDictionary>(Key, out var typed));
        Assert.Same(expected, typed);
    }

    [Fact]
    public void IPdfObject_requests_preserve_pdf_null_compatibility()
    {
        var dict = new PdfDictionary { [Key] = PdfNull.Value.Indirect() };

        Assert.Same(PdfNull.Value, dict.Get<IPdfObject>(Key));
        Assert.Same(PdfNull.Value, dict.GetOptional<IPdfObject>(Key));
        Assert.Same(PdfNull.Value, dict.GetRequired<IPdfObject>(Key));
        Assert.True(dict.TryGet<IPdfObject>(Key, out var value));
        Assert.Same(PdfNull.Value, value);
    }

    [Fact]
    public void Legacy_accessors_preserve_their_existing_behavior()
    {
        var mismatch = new PdfDictionary { [Key] = new PdfString("value") };
        var expected = new PdfDictionary();
        var valid = new PdfDictionary { [Key] = expected.Indirect() };

        Assert.Null(mismatch.Get<PdfDictionary>(Key));
        Assert.Throws<PdfLexerObjectMismatchException>(() => mismatch.GetOptionalValue<PdfDictionary>(Key));
        Assert.Throws<PdfLexerObjectMismatchException>(() => mismatch.GetRequiredValue<PdfDictionary>(Key));
        Assert.False(mismatch.TryGetValue<PdfDictionary>(Key, out _, errorOnMismatch: false));
        Assert.Throws<PdfLexerObjectMismatchException>(() =>
            mismatch.TryGetValue<PdfDictionary>(Key, out _));

        Assert.Same(expected, valid.Get<PdfDictionary>(Key));
        Assert.Same(expected, valid.GetOptionalValue<PdfDictionary>(Key));
        Assert.Same(expected, valid.GetRequiredValue<PdfDictionary>(Key));
    }

    [Fact]
    public void Indexer_documents_and_preserves_idictionary_missing_key_behavior()
    {
        var dict = new PdfDictionary();

        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => dict[Key]);
    }

    [Fact]
    public void Legacy_aliases_are_hidden_from_intellisense_without_obsolete_warnings()
    {
        var legacyNames = new[]
        {
            nameof(PdfDictionary.GetOptionalValue),
            nameof(PdfDictionary.GetRequiredValue)
        };

        var legacyMethods = typeof(PdfDictionary).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method =>
                legacyNames.Contains(method.Name) ||
                method.Name == nameof(PdfDictionary.Get) && method.IsGenericMethod ||
                method.Name == nameof(PdfDictionary.TryGetValue) && method.IsGenericMethod);

        Assert.All(legacyMethods, method =>
        {
            Assert.Equal(
                EditorBrowsableState.Never,
                method.GetCustomAttribute<EditorBrowsableAttribute>()?.State);
            Assert.Null(method.GetCustomAttribute<ObsoleteAttribute>());
        });
    }

    [Fact]
    public void Set_matches_indexer_and_remove_modification_semantics()
    {
        var assigned = new PdfDictionary();
        assigned.DictionaryModified = false;
        assigned.Set(Key, new PdfString("value"));
        Assert.True(assigned.DictionaryModified);

        assigned.DictionaryModified = false;
        assigned.Set(Key, null);
        Assert.True(assigned.DictionaryModified);
        Assert.False(assigned.ContainsKey(Key));

        assigned.DictionaryModified = false;
        assigned.Set(Key, null);
        Assert.False(assigned.DictionaryModified);
    }

    [Fact]
    public void Every_assignment_path_honors_indirect_only()
    {
        AssertStoresIndirect(dict => dict.Add(Key, new PdfDictionary()));
        AssertStoresIndirect(dict => dict.Add(new System.Collections.Generic.KeyValuePair<PdfName, IPdfObject>(
            Key, new PdfDictionary())));
        AssertStoresIndirect(dict => dict[Key] = new PdfDictionary());
        AssertStoresIndirect(dict => dict.Set(Key, new PdfDictionary()));
        AssertStoresIndirect(dict => dict.GetOrCreateValue<PdfDictionary>(Key));
        AssertStoresIndirect(dict => dict.GetOrSetValue(Key, new PdfDictionary()));
    }

    private static void AssertStoresIndirect(Action<PdfDictionary> assign)
    {
        var dict = new PdfDictionary { IndirectOnly = true };
        dict.DictionaryModified = false;

        assign(dict);

        Assert.Equal(PdfObjectType.IndirectRefObj, dict[Key].Type);
        Assert.True(dict.DictionaryModified);
    }
}
