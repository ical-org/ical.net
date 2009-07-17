using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;

namespace System.Collections.Generic
{
    public static class ListStub
    {
        public static int FindIndex<T>(this System.Collections.Generic.List<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentException();

            try
            {
                IEnumerator enumerator = list.GetEnumerator();
                int i = 0;
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is T && match((T)enumerator.Current))
                        return i;
                    i++;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        public static IList<T> FindAll<T>(this System.Collections.Generic.List<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentException();

            List<T> newList = new List<T>();
            try
            {
                IEnumerator enumerator = list.GetEnumerator();
                int i = 0;
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is T && match((T)enumerator.Current))
                        newList.Add((T)enumerator.Current);
                    i++;
                }
                return newList;
            }
            catch
            {
                return null;
            }
        }
    }
}
