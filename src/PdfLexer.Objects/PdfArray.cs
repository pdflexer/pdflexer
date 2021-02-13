using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Objects
{
    public class PdfArray : IParsedPdfObject, IList<IPdfObject>
    {
        internal List<IPdfObject> internalList = new List<IPdfObject>();
        public IPdfObject Wrapper { get; set; }
        // TODO check children
        public bool IsModified { get; set; }
        public void WriteObject(Stream stream)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IPdfObject> GetEnumerator() => internalList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => internalList.GetEnumerator();

        public void Add(IPdfObject item)
        {
            IsModified = true;
            internalList.Add(item);
        }

        public void Clear()
        {
            IsModified = true;
            internalList.Clear();
        }

        public bool Contains(IPdfObject item) => internalList.Contains(item);

        public void CopyTo(IPdfObject[] array, int arrayIndex) => internalList.CopyTo(array, arrayIndex);

        public bool Remove(IPdfObject item)
        {            
            var result = internalList.Remove(item);
            if (result)
            {
                IsModified = true;
            }
            return result;
        }

        public int Count => internalList.Count;
        public bool IsReadOnly => false;
        public int IndexOf(IPdfObject item) => internalList.IndexOf(item);

        public void Insert(int index, IPdfObject item)
        {
            IsModified = true;
            internalList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            IsModified = true;
            internalList.RemoveAt(index);
        }

        public IPdfObject this[int index]
        {
            get => internalList[index];
            set {
                IsModified = true;
                internalList[index] = value;
            }
        }
    }
}
