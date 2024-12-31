using NUnit.Framework;
using Unity.Mathematics;
using Utility.Math;

namespace Tests.Editor.Utility.Math
{
    public static class Int3ExtensionsTests
    {
        [Test]
        [TestCaseSource(typeof(Int3ExtensionsTestData), nameof(Int3ExtensionsTestData.ToBool3Test_Data))]
        public static bool3 TestToBool3(int value)
        {
            return value.ToBool3();
        }
    }
}