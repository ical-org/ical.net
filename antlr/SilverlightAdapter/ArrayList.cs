using System;
using SilverlightAdapter;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections {
    public class ArrayList : ICloneable, IEnumerable, IList {
        private List<object> content;

        // Proxy to content:
        public int Add(object obj) { content.Add(obj); return content.Count - 1; }
        public int Count { get { return content.Count; } }
        public bool Contains(object obj) { return content.Contains(obj); }
        public void Remove(object obj) { content.Remove(obj); }
        public void Clear() { content.Clear(); }
        public void RemoveRange(int startIdx, int n) { content.RemoveRange(startIdx, n); }
        public object this[int idx] {
            get { return content[idx]; }
            set { content[idx] = value; }
        }
        public void RemoveAt(int idx) { content.RemoveAt(idx); }
        public void Insert(int idx, object obj) { content.Insert(idx, obj); }
        public int IndexOf(object obj) { return content.IndexOf(obj); }
        public IEnumerator GetEnumerator() { return content.GetEnumerator(); }

        public int BinarySearch(Object value, IComparer comparer) {
            return content.BinarySearch(value, new NonGenericComparer(comparer));
        }

        public ArrayList GetRange(int index, int count) {
            Debug.Assert(false); return null;
        }

        public Array ToArray(Type type) {
            Array arr = content.ToArray();
            int len = arr.GetLength(0);
            Array res = Array.CreateInstance(type, len);
            Array.Copy(arr, res, len);
            return res;
        }

        // Stub implementations:
        public bool IsReadOnly { get { return false; } }
        public bool IsFixedSize { get { return false; } }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return this; } }

        public void AddRange(IEnumerable e) {
            content.AddRange(new NonGenericEnumerable(e));
        }

        public void CopyTo(System.Array arr, int idx) {
            content.CopyTo((object[])arr, idx);
        }

        public object Clone() {
            var newContent = content.GetRange(0, content.Count);
            var res = new ArrayList(newContent);
            return res;
        }


        public void CopyTo(object[] arr) {
            content.CopyTo(arr);
        }

        /// <summary>Constructor</summary>
        public ArrayList() {
            this.content = new List<object>();
        }

        /// <summary>Constructor</summary>
        public ArrayList(int n) {
            this.content = new List<object>(n);
        }

        /// <summary>Constructor</summary>
        public ArrayList(List<object> content) {
            this.content = content;
        }
    }
}
