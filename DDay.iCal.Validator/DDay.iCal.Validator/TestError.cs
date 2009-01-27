using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Validator
{
    public class TestError :
        ITestError
    {
        #region Constructors

        public TestError(string name, string source, IValidationResult[] validationResults)
        {
            Name = name;
            Message = ResourceManager.GetError(name);
            Source = source;
            ValidationResults = validationResults;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(Message);
            int i = 1;
            foreach (IValidationResult result in ValidationResults)
            {
                if (!BoolUtil.IsTrue(result.Passed))
                {
                    foreach (IValidationError error in result.Errors)
                    {
                        sb.Append(Environment.NewLine + i + ": ");
                        StringReader sr = new StringReader(error.ToString());
                        string s = sr.ReadLine();
                        bool isFirstLine = true;
                        while (!string.IsNullOrEmpty(s))
                        {
                            if (!isFirstLine)
                                sb.Append(Environment.NewLine);
                            sb.Append("\t" + s);
                            isFirstLine = false;
                            s = sr.ReadLine();
                        }
                        sr.Close();

                        i++;
                    }
                }
            }

            return sb.ToString();
        }

        #endregion

        #region ICalendarTestError Members

        virtual public string Name { get; protected set; }
        virtual public string Message { get; protected set; }
        virtual public string Source { get; protected set; }
        virtual public IValidationResult[] ValidationResults { get; protected set; }

        #endregion
    }
}
