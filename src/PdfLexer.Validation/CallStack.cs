namespace PdfLexer.Validation;

internal class CallStack
{
    public List<string> Stack { get; }
    public List<IPdfObject> Objects { get; }
    public CallStack()
    {
        Stack = new List<string>();
        Objects = new List<IPdfObject>();
    }
    internal CallStack(List<string> stack, List<IPdfObject> objs)
    {
        Stack = stack;
        Objects = objs;
    }

    public void Add<T>() where T : ISpecification
    {
        Stack.Add("R:" + T.Name);
    }
    public void Add(string val)
    {
        Stack.Add(val);
    }

    public void Add(IPdfObject val)
    {
        Objects.Add(val);
    }

    public void PopObj()
    {
        Objects.RemoveAt(Objects.Count - 1);
    }


    public IPdfObject? GetParent(int steps)
    {
        // last obj is current
        var n = Objects.Count - steps - 1;
        if (n >= 0) { return Objects[n]; }
        return null;
    }

    public CallStack Clone()
    {
        return new CallStack(Stack.ToList(), Objects.ToList());
    }

    public CallStack Pop()
    {
        Stack.RemoveAt(Stack.Count - 1);
        return this;
    }
}