using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace DDay.iCal.Validator.Xml
{
    public class LocalXmlDocumentProvider :
        IXmlDocumentProvider
    {
        #region Private Fields

        private string _LocalDirectoryName;        

        #endregion

        #region Public Properties

        public string LocalDirectoryName
        {
            get { return _LocalDirectoryName; }
            set { _LocalDirectoryName = value; }
        }

        #endregion

        #region Constructors

        public LocalXmlDocumentProvider(string localDirectoryName)
        {
            LocalDirectoryName = localDirectoryName;
        }

        #endregion

        #region IXmlDocumentProvider Members

        public XmlDocument Load(string path)
        {
            string contents = LoadXml(path);
            if (contents != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(contents);
                return xmlDoc;
            }

            return null;
        }

        public string LoadXml(string path)
        {
            if (!Path.IsPathRooted(path))
                path = Path.Combine(LocalDirectoryName, path);

            if (File.Exists(path))
            {
                string contents = null;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {
                    StreamReader sr = new StreamReader(fs);
                    contents = sr.ReadToEnd();
                    sr.Close();
                }

                return contents;
            }

            return null;
        }

        #endregion

        #region IEnumerable<string> Members

        private void GetFilesRecursive(string path, ref List<string> files)
        {
            foreach (string file in Directory.GetFiles(path, "*.*"))
                files.Add(file);

            foreach (string dir in Directory.GetDirectories(path))
                GetFilesRecursive(dir, ref files);
        }

        public IEnumerator<string> GetEnumerator()
        {
            // Since everything should be relative to the "root"
            // of the document tree (i.e. LocalDirectoryName),
            // let's change directories to this first to enumerate,
            // then switch back after we're done enumerating...
            string oldDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, LocalDirectoryName);

            List<string> files = new List<string>();
            GetFilesRecursive(".", ref files);

            Environment.CurrentDirectory = oldDirectory;

            return files.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        #endregion
    }
}
