using System.Collections;
using System.Text;

namespace PdfLexer;

/// <summary>
/// Pdf array object.
/// </summary>
public class PdfArray : PdfObject, IList<IPdfObject>
{
    internal List<IPdfObject> internalList = new List<IPdfObject>();
    internal bool ArrayModified {get;set;}
    internal bool IndirectOnly { get; set; }

    public override bool IsModified
    {
        get
        {
            if (ArrayModified)
            {
                return true;
            }

            foreach (var item in internalList)
            {
                if (item.IsModified)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public override PdfObjectType Type => PdfObjectType.ArrayObj;

    /// <summary>
    /// Creates a new empty array.
    /// </summary>
    public PdfArray()
    {
        ArrayModified = true;
    }

    /// <summary>
    /// Creates an new pdf array from the provided list.
    /// Note: List provided will be modified if Pdf array is modified.
    /// </summary>
    /// <param name="items"></param>
    public PdfArray(List<IPdfObject> items)
    {
        internalList = items;
        ArrayModified = true;
    }

    public IEnumerator<IPdfObject> GetEnumerator() => internalList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => internalList.GetEnumerator();

    public void Add(IPdfObject item)
    {
        ArrayModified = true;
        if (IndirectOnly)
        {
            internalList.Add(item.Indirect());
        } else
        {
            internalList.Add(item);
        }
        
    }

    public void Clear()
    {
        ArrayModified = true;
        internalList.Clear();
    }

    public bool Contains(IPdfObject item) => internalList.Contains(item);

    public void CopyTo(IPdfObject[] array, int arrayIndex) => internalList.CopyTo(array, arrayIndex);

    public bool Remove(IPdfObject item)
    {            
        var result = internalList.Remove(item);
        if (result)
        {
            ArrayModified = true;
        }
        return result;
    }

    public int Count => internalList.Count;
    public bool IsReadOnly => false;

    public int IndexOf(IPdfObject item) => internalList.IndexOf(item);

    public void Insert(int index, IPdfObject item)
    {
        ArrayModified = true;
        if (IndirectOnly)
        {
            internalList.Insert(index, item.Indirect());
        }
        else
        {
            internalList.Insert(index, item);
        }
    }

    public void RemoveAt(int index)
    {
        ArrayModified = true;
        internalList.RemoveAt(index);
    }

    public IPdfObject this[int index]
    {
        get => internalList[index];
        set {
            ArrayModified = true;
            if (IndirectOnly)
            {
                internalList[index] = value.Indirect();
            }
            else
            {
                internalList[index] = value;
            }
        }
    }


    /// <summary>
    /// Creates a new dictionary with the same contents.
    /// </summary>
    /// <returns></returns>
    public PdfArray CloneShallow()
    {
        return new PdfArray(internalList.ToList());
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var val in internalList)
        {
            sb.Append(val.ToString());
            sb.Append(' ');
        }
        sb.Append(']');
        return sb.ToString();
    }
}
