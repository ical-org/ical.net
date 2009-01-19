using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace DDay.iCal.Validator
{
    public class ValidatorActivator
    {
        static public IValidator Create(Type validatorType, iCalendar iCalendar)
        {
            return Create(validatorType, iCalendar, null);
        }

        static public IValidator Create(Type validatorType, string iCalendarText)
        {
            return Create(validatorType, null, iCalendarText);
        }

        static public IValidator Create(Type validatorType, iCalendar iCalendar, string iCalendarText)
        {
            IValidator validator = null;

            if (validatorType != null)
            {
                ConstructorInfo ci = null;

                if (iCalendar != null)
                    ci = validatorType.GetConstructor(new Type[] { typeof(iCalendar) });
                if (ci != null)
                    validator = ci.Invoke(new object[] { iCalendar }) as IValidator;
                else
                {
                    if (iCalendarText != null)
                        ci = validatorType.GetConstructor(new Type[] { typeof(string) });
                    if (ci != null)
                        validator = ci.Invoke(new object[] { iCalendarText }) as IValidator;
                }
            }

            return validator;
        }
    }
}
