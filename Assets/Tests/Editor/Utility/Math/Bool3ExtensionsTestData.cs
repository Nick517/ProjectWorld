using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;

namespace Tests.Editor.Utility.Math
{
    public static class Bool3ExtensionsTestData
    {
        public static IEnumerable<TestCaseData> ToSignTest_Data()
        {
            return new[]
            {
                new TestCaseData(new bool3(false, false, false)).Returns(new int3(-1, -1, -1)),
                new TestCaseData(new bool3(true, false, false)).Returns(new int3(1, -1, -1)),
                new TestCaseData(new bool3(false, true, false)).Returns(new int3(-1, 1, -1)),
                new TestCaseData(new bool3(false, false, true)).Returns(new int3(-1, -1, 1)),
                new TestCaseData(new bool3(true, true, false)).Returns(new int3(1, 1, -1)),
                new TestCaseData(new bool3(true, false, true)).Returns(new int3(1, -1, 1)),
                new TestCaseData(new bool3(false, true, true)).Returns(new int3(-1, 1, 1)),
                new TestCaseData(new bool3(true, true, true)).Returns(new int3(1, 1, 1)),
                new TestCaseData(new bool3()).Returns(new int3(-1, -1, -1)),
                new TestCaseData(new bool3(true)).Returns(new int3(1, 1, 1)),
                new TestCaseData(new bool3(false)).Returns(new int3(-1, -1, -1))
            };
        }

        public static IEnumerable<TestCaseData> ToIndexTest_Data()
        {
            return new[]
            {
                new TestCaseData(new bool3(false, false, false)).Returns(0),
                new TestCaseData(new bool3(true, false, false)).Returns(1),
                new TestCaseData(new bool3(false, true, false)).Returns(2),
                new TestCaseData(new bool3(true, true, false)).Returns(3),
                new TestCaseData(new bool3(false, false, true)).Returns(4),
                new TestCaseData(new bool3(true, false, true)).Returns(5),
                new TestCaseData(new bool3(false, true, true)).Returns(6),
                new TestCaseData(new bool3(true, true, true)).Returns(7),
                new TestCaseData(new bool3()).Returns(0),
                new TestCaseData(new bool3(true)).Returns(7),
                new TestCaseData(new bool3(false)).Returns(0)
            };
        }
    }
}