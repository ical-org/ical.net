using System.Collections;
using System.Collections.Generic;

// Convert between generic and non-generic interfaces. 
// The generic interfaces takes as type T 'object' to make them non-generic.

namespace SilverlightAdapter {
    /// <summary>Converts IComparer to IComparer<object></summary>
    public class NonGenericComparer : IComparer<object> {
        private IComparer comp;
        public int Compare(object o1, object o2) { return comp.Compare(o1, o2); }
        public NonGenericComparer(IComparer comp) { this.comp = comp; }
    }

    /// <summary>Converts IEnumerator to IEnumerator<object></summary>
    public class NonGenericEnumerator : IEnumerator<object> {
        private IEnumerator et;

        public object Current { get { return et.Current;  } }
        public bool MoveNext() { return et.MoveNext(); }
        public void Reset() { et.Reset(); }
        public void Dispose() { }
        public NonGenericEnumerator(IEnumerator et) { this.et = et; }
    }

    /// <summary>Converts IEnumerable to IEnumerable<object></summary>
    public class NonGenericEnumerable : IEnumerable<object> {
        private IEnumerable en;

        public IEnumerator<object> GetEnumerator() {
            return new NonGenericEnumerator(en.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return en.GetEnumerator();
        }

        public NonGenericEnumerable(IEnumerable en) { this.en = en; }
    }

    /// <summary>Converts from IComparer to IEqualityComparer<object></summary>
    public class NonGenericEqualityComparer : IEqualityComparer<object> {
        private IComparer comp;
        public int GetHashCode(object obj) { return obj.GetHashCode(); }
        new public bool Equals(object o1, object o2) {
            return true;
        }

        public NonGenericEqualityComparer(IComparer comp) { this.comp = comp; }
    }

    /// <summary>
    /// Convert from Dictionary<object,object>.Enumerator to IEnumerator
    /// </summary>
    public class NonGenericDictionaryEnumerator : IEnumerator {
        private Dictionary<object, object> dict;
        private Dictionary<object, object>.Enumerator en;

        public object Current { 
            get {
                KeyValuePair<object, object> kv = en.Current;
                var de = new DictionaryEntry(kv.Key, kv.Value);
                return de;
            }
        }
        public bool MoveNext() { return en.MoveNext(); }
        public void Reset() { en = dict.GetEnumerator(); }

        public NonGenericDictionaryEnumerator(Dictionary<object, object> dict) {
            this.dict = dict;
            en = dict.GetEnumerator();
        }
    }
}
