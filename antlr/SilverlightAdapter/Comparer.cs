using System;
using System.Collections;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Collections
{
    public class Comparer : IComparer
    {
        public int Compare(object o1, object o2)
        {
            return ((IComparable)o1).CompareTo(o2);
        }

        public static IComparer Default
        {
            get { return new Comparer(); }
        }
    }
}
