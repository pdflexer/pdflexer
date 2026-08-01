namespace PdfLexer.Remediation;

/// <summary>
/// Adapts the compiled program template to the existing structural materializer without
/// adapting the program bindings into legacy rules.
/// </summary>
internal static class RemediationProgramTemplateAdapter
{
    internal static RemediationStructuralTemplate Create(CompiledRemediationProgram compiled)
    {
        ArgumentNullException.ThrowIfNull(compiled);

        var document = LowerNode(compiled, compiled.Program.Template.Document, "/", isRoot: true);
        return new RemediationStructuralTemplate(document, RemediationStructuralTemplateMode.Prescriptive)
        {
            Id = compiled.Program.Template.Id,
            Version = compiled.Program.Template.Version
        };
    }

    private static RemediationStructuralTemplateNode LowerNode(
        CompiledRemediationProgram compiled,
        RemediationTemplateNode node,
        string path,
        bool isRoot)
    {
        var children = node.Children
            .Select(child =>
            {
                var childPath = path == "/" ? $"/{child.Name}" : $"{path}/{child.Name}";
                return LowerNode(compiled, child, childPath, isRoot: false);
            })
            .ToArray();

        var slot = isRoot ? null : SlotRef.Absolute(path);
        var producer = slot == null
            ? null
            : compiled.Program.Bindings
                .Where(x => x.Target is BindingTarget.Slot target && target.Reference == slot)
                .Select(x => x.Id)
                .FirstOrDefault();

        return new RemediationStructuralTemplateNode(
            node.Tag,
            children,
            id: isRoot ? null : path[1..],
            occurrence: (RemediationStructuralOccurrence)node.Occurrence,
            properties: node.Properties,
            orderPolicy: node.OrderPolicy)
        {
            ProgramSlot = slot,
            IdentitySegment = isRoot ? "Document" : node.Name,
            BindingId = producer
        };
    }
}
