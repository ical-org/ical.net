using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.ComponentModel {
    public class EventHandlerList {
        private List<object> keys = new List<object>();
        private Dictionary<object, Delegate> map = new Dictionary<object, Delegate>();

        public void RemoveHandler(object key, Delegate value) {
            keys.Remove(key);
            map.Remove(key);
        }

        public void AddHandler(object key, Delegate value) {
            keys.Add(key);
            map.Add(value, value);
        }

        public Delegate this[object key] {
            get {
                return map[key];
            }
        }
    }
}
