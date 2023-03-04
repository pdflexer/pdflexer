using PdfLexer.Parsers.Structure;
using System.Runtime.CompilerServices;

namespace PdfLexer.Validation;

internal interface ISpecification
{
    static abstract bool RuleGroup();
    static abstract string Name { get; }
    static abstract bool AppliesTo(decimal version, List<string> extensions);
}

internal interface ISpecification<T> : ISpecification where T : IPdfObject
{
    static abstract void Validate(PdfValidator ctx, CallStack stack, T obj, IPdfObject? parent);
    static abstract bool MatchesType(PdfValidator ctx, T obj);
    static virtual bool Remediate(PdfValidator ctx, T obj, IPdfObject? parent) { return false; }
}


internal enum IndirectRequirement
{
    Either,
    MustBeDirect,
    MustBeIndirect
}