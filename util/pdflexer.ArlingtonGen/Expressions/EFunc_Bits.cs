using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_BitClear : EFunBase
{
    public EFunc_BitClear(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("BitsClear(");
        sb.Append(VariableContext.VarSub);
        sb.Append(",");
        var op = Inputs[0].Children[0] as EValue;
        var i = int.Parse(op.Text);
        i = 33 - i;
        sb.Append("0b" + "".PadLeft(i-1,'0') + "1" + "".PadLeft(32-i, '0'));
        sb.Append(")");
    }
}


internal class EFunc_BitsClear : EFunBase
{
    public EFunc_BitsClear(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("BitsClear(");
        sb.Append(VariableContext.VarSub);
        sb.Append(",");
        var sop = Inputs[0].Children[0] as EValue;
        var s = int.Parse(sop.Text);
        s = 33 - s;
        var eop = Inputs[1].Children[0] as EValue;
        var e = int.Parse(eop.Text);
        e = 33 - e;
        sb.Append("0b" + ("".PadLeft(e - 1, '0') + "".PadLeft(s-e+1, '1') + "".PadLeft(32 - s, '0')));
        sb.Append(")");
    }
}

internal class EFunc_BitSet : EFunBase
{
    public EFunc_BitSet(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("BitsSet(");
        sb.Append(VariableContext.VarSub);
        sb.Append(",");
        var op = Inputs[0].Children[0] as EValue;
        var i = int.Parse(op.Text);
        i = 33 - i;
        sb.Append("0b" + "".PadLeft(i - 1, '0') + "1" + "".PadLeft(32 - i, '0'));
        sb.Append(")");
    }
}


internal class EFunc_BitsSet : EFunBase
{
    public EFunc_BitsSet(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("BitsSet(");
        sb.Append(VariableContext.VarSub);
        sb.Append(",");
        var sop = Inputs[0].Children[0] as EValue;
        var s = int.Parse(sop.Text);
        s = 33 - s;
        var eop = Inputs[1].Children[0] as EValue;
        var e = int.Parse(eop.Text);
        e = 33 - e;
        sb.Append("0b" + ("".PadLeft(e - 1, '0') + "".PadLeft(s - e + 1, '1') + "".PadLeft(32 - s, '0')));
        sb.Append(")");
    }
}