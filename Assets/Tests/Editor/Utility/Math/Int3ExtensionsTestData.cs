using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;

namespace Tests.Editor.Utility.Math
{
    public class Int3ExtensionsTestData
    {
        public static IEnumerable<TestCaseData> ToBool3Test_Data()
        {
            return new[]
            {
                new TestCaseData(0).Returns(new bool3(false, false, false)),
                new TestCaseData(1).Returns(new bool3(true, false, false)),
                new TestCaseData(2).Returns(new bool3(false, true, false)),
                new TestCaseData(3).Returns(new bool3(true, true, false)),
                new TestCaseData(4).Returns(new bool3(false, false, true)),
                new TestCaseData(5).Returns(new bool3(true, false, true)),
                new TestCaseData(6).Returns(new bool3(false, true, true)),
                new TestCaseData(7).Returns(new bool3(true, true, true)),
                new TestCaseData(-1).Returns(new bool3(false, false, false)),
                new TestCaseData(8).Returns(new bool3(true, true, true))
            };
        }
    }
}