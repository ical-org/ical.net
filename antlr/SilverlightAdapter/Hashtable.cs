using SilverlightAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Collections {
    public class Hashtable : ICloneable, IDictionary {
        private Dictionary<object, object> map;

        public void Clear() { map.Clear(); }
        public void Add(object key, object val) { map.Add(key, val); }
        public bool Contains(object key) { return map.ContainsKey(key); }
        public bool ContainsKey(object key) { return map.ContainsKey(key); }
        public int Count { get { return map.Count; } }
        public ICollection Keys { get { return map.Keys; } }
        public void Remove(object obj) { map.Remove(obj); }
        public ICollection Values { get { return map.Values; } }
        IEnumerator IEnumerable.GetEnumerator() {
            return new NonGenericDictionaryEnumerator(map);
        }
        IDictionaryEnumerator IDictionary.GetEnumerator() { return map.GetEnumerator(); }

        public void CopyTo(Array arr, int idx) { 
            foreach (var de in map) {
                arr.SetValue(de, idx);
                idx += 1;
            }
        }

        public bool IsReadOnly { get { return false; } }
        public bool IsFixedSize { get { return false; } }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return this; } }

        public object this[object key] {
            get {
                if (map.ContainsKey(key)) {
                    return map[key];
                } else {
                    // Generic version gives an exception, non generic doesn't
                    return null;
                }
            }
            set { 
                map[key] = value; 
            }
        }


        public object Clone() {
            var res = new Hashtable();
            foreach (var kv in map) {
                res[kv.Key] = kv.Value;
            }
            return res;
        }

        /// <summary>Constructor</summary>
        public Hashtable() { 
            map = new Dictionary<object, object>();
        }
        /// <summary>Constructor</summary>
        public Hashtable(int n, float f) {
            map = new Dictionary<object, object>();
        }
        /// <summary>Constructor</summary>
        public Hashtable(int n, float f, object hcp, IComparer comparer) {
            Debug.Assert(hcp == null);
            var genComparer = new NonGenericEqualityComparer(comparer);
            map = new Dictionary<object, object>(genComparer);
        }
    }
}
