using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer
{
    /// <summary>
    /// Pdf array object.
    /// </summary>
    public class PdfArray : PdfObject, IList<IPdfObject>
    {
        internal List<IPdfObject> internalList = new List<IPdfObject>();
        internal bool ArrayModified {get;set;}

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
        /// Note: List provided will be modified is Pdf array is modified.
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
            internalList.Add(item);
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
            internalList.Insert(index, item);
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
                internalList[index] = value;
            }
        }
    }
}
