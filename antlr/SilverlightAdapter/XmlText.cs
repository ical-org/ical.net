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
using System.IO;
using System.Text;

namespace System.Xml {
    public enum Formatting { Indented }

    public class XmlTextWriter {
        // Stubs:
        public void WriteString(string s) { }
        public void Close() { }
        public Formatting Formatting { set { } }
        public char IndentChar { set { } }
        public void WriteProcessingInstruction(string a, string b) { }
        public void WriteStartElement(params object[] args) { }
        public void WriteAttributeString(params object[] args) { }
        public void WriteCData(params object[] args) { }
        public void WriteEndElement() { }

        // Constructor:
        public XmlTextWriter(Stream stream, Encoding encoding) { }
    }

    public class XmlTextReader {
    }
}
