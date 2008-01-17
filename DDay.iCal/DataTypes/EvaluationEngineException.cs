using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.DataTypes
{
    public class EvaluationEngineException : Exception
    {
        public EvaluationEngineException() : base("An error occurred during the evaluation process that could not be automatically handled. Check the evaluation mode and restrictions.")
        {
        }
    }
}
