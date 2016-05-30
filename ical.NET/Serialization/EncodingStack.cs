using System.Collections.Generic;
using System.Text;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization
{
    public class EncodingStack : IEncodingStack
    {
        private readonly Stack<Encoding> _mStack;

        public EncodingStack()
        {
            _mStack = new Stack<Encoding>();
        }

        public Encoding Current
        {
            get
            {
                if (_mStack.Count > 0)
                {
                    return _mStack.Peek();
                }

                // Default to Unicode encoding
                return Encoding.Unicode;
            }
        }

        public void Push(Encoding encoding)
        {
            if (encoding != null)
            {
                _mStack.Push(encoding);
            }
        }

        public Encoding Pop()
        {
            if (_mStack.Count > 0)
            {
                return _mStack.Pop();
            }
            return null;
        }
    }
}