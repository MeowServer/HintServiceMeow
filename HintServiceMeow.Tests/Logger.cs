using HintServiceMeow.Core.Interface;
using System;

namespace HintServiceMeow.Tests
{
    public class TestLogger : ILogger
    {
        public void Info(object message)
        {
            Console.Write("[Test][Info]");
            Console.WriteLine(message);
        }

        public void Error(object message)
        {
            Console.Write("[Test][Error]");
            Console.WriteLine(message);
        }
    }
}
