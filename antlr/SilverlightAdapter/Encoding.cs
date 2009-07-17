using System;
using System.Text;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightAdapter
{
    public static class MyEncoding
    {
        public static string GetString(this System.Text.Encoding enc, byte[] b)
        {
            return enc.GetString(b, 0, b.GetLength(0));
        }
    }
}
