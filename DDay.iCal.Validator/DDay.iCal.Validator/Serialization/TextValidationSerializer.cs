using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using DDay.iCal.Validator.Xml;

namespace DDay.iCal.Validator.Serialization
{
    public class TextValidationSerializer :
        IValidationSerializer
    {
        #region Constructors

        public TextValidationSerializer()
        {
        }

        public TextValidationSerializer(
            IValidationRuleset ruleset,
            ITestResult[] testResults) :
            this()
        {
            Ruleset = ruleset;
            TestResults = testResults;
        }

        public TextValidationSerializer(
            IValidationRuleset ruleset,
            IValidationResult[] validationResults) :
            this()
        {
            Ruleset = ruleset;
            ValidationResults = validationResults;
        }

        #endregion

        #region IValidationSerializer Members

        public string DefaultExtension
        {
            get { return "txt"; }
        }

        public Encoding DefaultEncoding
        {
            get { return Encoding.Unicode; }
        }

        virtual public IValidationRuleset Ruleset { get; set; }
        virtual public ITestResult[] TestResults { get; set; }
        virtual public IValidationResult[] ValidationResults { get; set; }

        virtual public void Serialize(Stream stream, Encoding encoding)
        {
            using (StreamWriter sw = new StreamWriter(stream, encoding))
            {
                if (Ruleset != null)
                {
                    if (TestResults != null &&
                        TestResults.Length > 0)
                    {
                        int numTests = TestResults.Length;
                        int numTestsRun = 0;
                        foreach (ITestResult result in TestResults)
                        {
                            if (result.Passed != null && result.Passed.HasValue)
                                numTestsRun++;
                        }

                        if (!object.Equals(numTests, numTestsRun))
                        {
                            sw.WriteLine(string.Format(
                                ResourceManager.GetString("notAllTestsPerformed"),
                                numTests,
                                numTestsRun,
                                numTests - numTestsRun
                            ));
                        }

                        int passed = 0;
                        foreach (ITestResult result in TestResults)
                        {
                            if (BoolUtil.IsTrue(result.Passed))
                                passed++;

                            sw.WriteLine(result.ToString());
                        }

                        sw.WriteLine(string.Format(
                            ResourceManager.GetString("passVsFail"),
                            passed,
                            numTestsRun,
                            string.Format("{0:0.0}", ((double)passed / (double)numTestsRun) * 100)
                        ));
                    }
                    else if (TestResults != null)
                    {
                        sw.WriteLine(ResourceManager.GetString("noTestsPerformed"));
                    }

                    if (ValidationResults != null &&
                        ValidationResults.Length > 0)
                    {
                        foreach (IValidationResult result in ValidationResults)
                        {
                            if (!BoolUtil.IsTrue(result.Passed))
                            {
                                foreach (IValidationError error in result.Errors)
                                {
                                    sw.WriteLine(error.ToString());
                                }
                            }                            
                        }
                    }
                    else if (ValidationResults != null)
                    {
                        sw.WriteLine(ResourceManager.GetString("calendarValid"));
                    }
                }
                else
                {
                    sw.WriteLine(ResourceManager.GetString("noValidationRuleset"));
                }
            }
        }

        #endregion
    }
}
