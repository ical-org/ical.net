using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace DDay.iCal.Validator
{
    public class ZipExtractor : IDisposable
    {
        FileStream _ZipFileStream = null;
        ZipFile _ZipFile = null;

        public ZipExtractor(string zipFilepath)
        {
            _ZipFileStream = File.OpenRead(zipFilepath);
            if (_ZipFileStream != null)
            {
                _ZipFile = new ZipFile(_ZipFileStream);
            }
        }

        virtual public string GetFileContents(string pathToFileWithinZip)
        {
            if (_ZipFile != null)
            {
                int index = _ZipFile.FindEntry(pathToFileWithinZip, true);
                if (index >= 0)
                {
                    StringBuilder sb = new StringBuilder();
                    StringWriter sw = new StringWriter(sb);

                    string result = null;
                         
                    Stream zis = _ZipFile.GetInputStream(index);
                    if (zis != null)
                    {
                        StreamReader sr = new StreamReader(zis);
                        result = sr.ReadToEnd();
                        sr.Close();
                    }

                    return result;
                }
            }
            return null;
        }

        #region IDisposable Members

        virtual public void Dispose()
        {
            if (_ZipFile != null)
            {
                _ZipFile.Close();
                _ZipFile = null;
            }
            if (_ZipFileStream != null)
            {
                _ZipFileStream.Close();
                _ZipFileStream.Dispose();
                _ZipFileStream = null;
            }
        }

        #endregion
    }
}
