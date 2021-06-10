using Ical.Net.Serialization.DataTypes;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ical.Net.CoreUnitTests
{
    [TestFixture]
    public class QuotedPrintableStringSerializerTests
    {
        [Test, Category("Serialization")]
        public void MixedQuotedPrintable()
        {
            var serializer = new QuotedPrintableStringSerializer();

            var sourceString   = "<p>Normal   0         false   false   false                             MicrosoftInternetExplorer4</p>=0D=0A<br class=3D\"clear\" />";
            var expectedString = "<p>Normal   0         false   false   false                             MicrosoftInternetExplorer4</p>\r\n<br class=\"clear\" />";
            var reader = new StringReader(sourceString);

            var decodedString = serializer.Deserialize(reader);

            Assert.AreEqual(expectedString, decodedString);
        }

        [Test, Category("Serialization")]
        public void QuotedPrintableNewLine()
        {
            var serializer = new QuotedPrintableStringSerializer();

            var sourceString = "=20=0D=0A";
            var expectedString = " \r\n";
            var reader = new StringReader(sourceString);

            var decodedString = serializer.Deserialize(reader);

            Assert.AreEqual(expectedString, decodedString);
        }

        [Test, Category("Serialization")]
        public void QuotedPrintable_Test1()
        {
            var serializer = new QuotedPrintableStringSerializer();

            var sourceString = "If you believe that truth=3Dbeauty, then surely mathematics is the most bea=\r\nutiful branch of philosophy.";
            var expectedString = "If you believe that truth=beauty, then surely mathematics is the most beautiful branch of philosophy.";
            var reader = new StringReader(sourceString);

            var decodedString = serializer.Deserialize(reader);

            Assert.AreEqual(expectedString, decodedString);
        }

        [Test, Category("Serialization")]
        public void QuotedPrintable_Test2()
        {
            var serializer = new QuotedPrintableStringSerializer();

            var sourceString = "fooI=C3=B1t=C3=ABrn=C3=A2ti=C3=B4n=C3=A0liz=C3=A6ti=C3=B8n=E2=98=83=F0=9F=\r\n=92=A9bar";
            var expectedString = "fooI\xF1t\xEBrn\xE2ti\xF4n\xE0liz\xE6ti\xF8n\u2603\uD83D\uDCA9bar";
            var reader = new StringReader(sourceString);

            var decodedString = serializer.Deserialize(reader);

            Assert.AreEqual(expectedString, decodedString);
        }

        [Test, Category("Serialization")]
        public void QuotedPrintable_Test3()
        {
            var serializer = new QuotedPrintableStringSerializer();

            var sourceString = "foo=0D=0Abar";
            var expectedString = "foo\r\nbar";
            var reader = new StringReader(sourceString);

            var decodedString = serializer.Deserialize(reader);

            Assert.AreEqual(expectedString, decodedString);
        }

        [Test, Category("Serialization")]
        public void QuotedPrintable_Trailing_Whitespace_Test()
        {
            var serializer = new QuotedPrintableStringSerializer();

            var sourceString = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=                     \r\nxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=\t\t    \r\nxx";
            var expectedString = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
            var reader = new StringReader(sourceString);

            var decodedString = serializer.Deserialize(reader);

            Assert.AreEqual(expectedString, decodedString);
        }

        [Test, Category("Serialization")]
        public void QuotedPrintable_SJIS_Test3()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // the SJIS code page is NOT included in .netstandard by default.
            //in order to use these code pages they must be registered.

            var serializer = new QuotedPrintableStringSerializer("SJIS");

            var sourceString = "=83z=81[=83=80=83Y=83X=83^=83W=83A=83=80=90_=8C=CB";
            var expectedString = "ホームズスタジアム神戸";
            var reader = new StringReader(sourceString);

            var decodedString = serializer.Deserialize(reader);

            Assert.AreEqual(expectedString, decodedString);
        }

    }
}
