using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    class ImpossibleException : Exception
    {
        public string Method { get; private set; }
        public ImpossibleException(string message, string methodName) : base(message)
        {
            Method = methodName;
        }
    }
}
