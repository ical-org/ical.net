using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Validator
{
    public class CalendarTestError :
        ICalendarTestError
    {
        #region Constructors

        public CalendarTestError(string name, string source, IValidationError[] errors)
        {
            Name = name;
            Message = ResourceManager.GetError(name);
            Source = source;
            Errors = errors;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(Message);
            int i = 1;
            foreach (IValidationError error in Errors)
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

            return sb.ToString();
        }

        #endregion

        #region ICalendarTestError Members

        virtual public string Name { get; protected set; }
        virtual public string Message { get; protected set; }
        virtual public string Source { get; protected set; }
        virtual public IValidationError[] Errors { get; protected set; }

        #endregion
    }
}
