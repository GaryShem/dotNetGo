using System;

namespace Engine.Misc
{
    public class GameEngineException : Exception
    {
        public string Method { get; private set; }
        public GameEngineException(string message, string methodName) : base(message)
        {
            Method = methodName;
        }
    }
}
