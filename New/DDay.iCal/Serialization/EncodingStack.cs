using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public class EncodingStack :
        IEncodingStack
    {
        #region Private Fields

        Stack<Encoding> m_Stack;

        #endregion

        #region Constructors

        public EncodingStack()
        {
            m_Stack = new Stack<Encoding>();
        }

        #endregion

        #region IEncodingStack Members

        public Encoding Current
        {
            get
            {
                if (m_Stack.Count > 0)
                    return m_Stack.Peek();

                // Default to Unicode encoding
                return Encoding.Unicode;
            }
        }

        public void Push(Encoding encoding)
        {
            if (encoding != null)
                m_Stack.Push(encoding);
        }

        public Encoding Pop()
        {
            if (m_Stack.Count > 0)
                return m_Stack.Pop();
            return null;
        }

        #endregion
    }
}
