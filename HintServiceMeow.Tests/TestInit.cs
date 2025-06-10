using HintServiceMeow.Core.Utilities.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests
{
    [TestClass]
    internal static class TestInit
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Logger.Instance = new TestLogger(); // Use test logger for unit tests
        }
    }
}
