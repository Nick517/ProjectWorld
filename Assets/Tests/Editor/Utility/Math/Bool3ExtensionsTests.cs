using NUnit.Framework;
using Unity.Mathematics;
using Utility.Math;

namespace Tests.Editor.Utility.Math
{
    [TestFixture]
    public class Bool3ExtensionsTests
    {
        [Test]
        [TestCaseSource(typeof(Bool3ExtensionsTestData), nameof(Bool3ExtensionsTestData.ToSignTest_Data))]
        public static int3 TestToSign(bool3 bool3)
        {
            return bool3.ToSign();
        }

        [Test]
        [TestCaseSource(typeof(Bool3ExtensionsTestData), nameof(Bool3ExtensionsTestData.ToIndexTest_Data))]
        public static int TestToIndex(bool3 bool3)
        {
            return bool3.ToIndex();
        }
    }
}