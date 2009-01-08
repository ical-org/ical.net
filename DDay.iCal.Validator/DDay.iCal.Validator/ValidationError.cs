using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationError : 
        IValidationError
    {
        #region IValidationError Members

        public int ErrorNumber { get; set; }
        public ValidationErrorType Type { get; set; }
        public string Message { get; set; }
        public string[] Resolutions { get; set; }
        public bool IsFatal { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }

        #endregion

        #region Protected Methods

        protected void LoadResolutions()
        {
            if (!string.IsNullOrEmpty(Message))
            {
                List<string> resolutions = new List<string>();
                try
                {
                    for (int i = 0; ; i++)
                        resolutions.Add(ResourceManager.GetString(Message + "_Resolution" + i));
                }
                catch { }
                finally
                {
                    Resolutions = resolutions.ToArray();
                }
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ResourceManager.GetString(Type.ToString() + "String"));
            if (ErrorNumber != default(int))
                sb.Append(" " + ErrorNumber);

            if (!string.IsNullOrEmpty(Message))
            {
                sb.Append(": ");
                sb.Append(Message);
            }
            
            if (Line != default(int))
            {
                sb.Append(" (" + ResourceManager.GetString("LineString") + " ");
                sb.Append(Line);
                if (Col != default(int))
                {
                    sb.Append(" " + ResourceManager.GetString("ColumnString") + " ");
                    sb.Append(Col);
                }
                sb.Append(")");
            }

            if (Resolutions != null &&
                Resolutions.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Suggested actions:");
                foreach (string ca in Resolutions)
                    sb.AppendLine("    " + ca);
            }

            return sb.ToString();
        }

        #endregion
    }
}
