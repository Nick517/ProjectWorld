using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;

namespace Tests.Editor.Utility.TerrainGeneration
{
    public static class SegmentOperationsTestData
    {
        public static IEnumerable<TestCaseData> GetCubeSize_Data()
        {
            return new[]
            {
                // Basic cases
                new TestCaseData(10.0f, 0).Returns(10.0f),
                new TestCaseData(10.0f, 1).Returns(20.0f),
                new TestCaseData(10.0f, -1).Returns(5.0f),

                // Zero baseCubeSize
                new TestCaseData(0.0f, 0).Returns(0.0f),
                new TestCaseData(0.0f, 5).Returns(0.0f),
                new TestCaseData(0.0f, -5).Returns(0.0f),

                // Very small values
                new TestCaseData(float.Epsilon, 0).Returns(float.Epsilon),
                new TestCaseData(float.Epsilon, 1).Returns(float.Epsilon * 2),

                // Large values
                new TestCaseData(float.MaxValue / 2, 1).Returns(float.MaxValue),
                new TestCaseData(float.MinValue / 2, 1).Returns(float.MinValue),

                // Negative baseCubeSize
                new TestCaseData(-10.0f, 0).Returns(-10.0f),
                new TestCaseData(-10.0f, 1).Returns(-20.0f),
                new TestCaseData(-10.0f, -1).Returns(-5.0f),

                // Decimal values
                new TestCaseData(0.5f, 0).Returns(0.5f),
                new TestCaseData(0.5f, 1).Returns(1.0f),
                new TestCaseData(0.5f, -1).Returns(0.25f),

                // Default parameter
                new TestCaseData(15.0f, null).Returns(15.0f)
            };
        }

        public static IEnumerable<TestCaseData> GetSegSize_Data()
        {
            return new[]
            {
                // Basic cases
                new TestCaseData(10.0f, 0).Returns(10.0f),
                new TestCaseData(10.0f, 1).Returns(20.0f),
                new TestCaseData(10.0f, -1).Returns(5.0f),

                // Zero baseSegmentSize
                new TestCaseData(0.0f, 0).Returns(0.0f),
                new TestCaseData(0.0f, 5).Returns(0.0f),
                new TestCaseData(0.0f, -5).Returns(0.0f),

                // Very small values
                new TestCaseData(float.Epsilon, 0).Returns(float.Epsilon),
                new TestCaseData(float.Epsilon, 1).Returns(float.Epsilon * 2),

                // Large values
                new TestCaseData(float.MaxValue / 2, 1).Returns(float.MaxValue),
                new TestCaseData(float.MinValue / 2, 1).Returns(float.MinValue),

                // Negative baseSegmentSize
                new TestCaseData(-10.0f, 0).Returns(-10.0f),
                new TestCaseData(-10.0f, 1).Returns(-20.0f),
                new TestCaseData(-10.0f, -1).Returns(-5.0f),

                // Decimal values
                new TestCaseData(0.5f, 0).Returns(0.5f),
                new TestCaseData(0.5f, 1).Returns(1.0f),
                new TestCaseData(0.5f, -1).Returns(0.25f),

                // Default parameter
                new TestCaseData(15.0f, null).Returns(15.0f)
            };
        }

        public static IEnumerable<TestCaseData> GetClosestSegPos_Data()
        {
            return new[]
            {
                // Basic cases
                new TestCaseData(new float3(5.5f, 5.5f, 5.5f), 10.0f).Returns(new float3(0f, 0f, 0f)),
                new TestCaseData(new float3(15.5f, 15.5f, 15.5f), 10.0f).Returns(new float3(10f, 10f, 10f)),
                new TestCaseData(new float3(-5.5f, -5.5f, -5.5f), 10.0f).Returns(new float3(-10f, -10f, -10f)),

                // Edge cases
                new TestCaseData(new float3(10f, 10f, 10f), 10.0f).Returns(new float3(10f, 10f, 10f)),
                new TestCaseData(new float3(0f, 0f, 0f), 10.0f).Returns(new float3(0f, 0f, 0f)),

                // Mixed coordinates
                new TestCaseData(new float3(5.5f, -5.5f, 15.5f), 10.0f).Returns(new float3(0f, -10f, 10f)),

                // Small segment size
                new TestCaseData(new float3(1.7f, 1.7f, 1.7f), 1.0f).Returns(new float3(1f, 1f, 1f)),

                // Large segment size
                new TestCaseData(new float3(50f, 50f, 50f), 100.0f).Returns(new float3(0f, 0f, 0f))
            };
        }

        public static IEnumerable<TestCaseData> GetClosestSegCenter_Data()
        {
            return new[]
            {
                // Basic cases
                new TestCaseData(new float3(5.5f, 5.5f, 5.5f), 10.0f).Returns(new float3(5f, 5f, 5f)),
                new TestCaseData(new float3(15.5f, 15.5f, 15.5f), 10.0f).Returns(new float3(15f, 15f, 15f)),
                new TestCaseData(new float3(-5.5f, -5.5f, -5.5f), 10.0f).Returns(new float3(-5f, -5f, -5f)),

                // Edge cases
                new TestCaseData(new float3(10f, 10f, 10f), 10.0f).Returns(new float3(15f, 15f, 15f)),
                new TestCaseData(new float3(0f, 0f, 0f), 10.0f).Returns(new float3(5f, 5f, 5f)),

                // Mixed coordinates
                new TestCaseData(new float3(5.5f, -5.5f, 15.5f), 10.0f).Returns(new float3(5f, -5f, 15f)),

                // Small segment size
                new TestCaseData(new float3(1.7f, 1.7f, 1.7f), 1.0f).Returns(new float3(1.5f, 1.5f, 1.5f)),

                // Large segment size
                new TestCaseData(new float3(50f, 50f, 50f), 100.0f).Returns(new float3(50f, 50f, 50f))
            };
        }

        public static IEnumerable<TestCaseData> PointWithinSeg_Data()
        {
            return new[]
            {
                // Basic cases - point inside segment
                new TestCaseData(new float3(5f, 5f, 5f), new float3(0f, 0f, 0f), 10f).Returns(true),
                new TestCaseData(new float3(9.9f, 9.9f, 9.9f), new float3(0f, 0f, 0f), 10f).Returns(true),

                // Point outside segment
                new TestCaseData(new float3(10f, 10f, 10f), new float3(0f, 0f, 0f), 10f).Returns(false),
                new TestCaseData(new float3(-0.1f, 5f, 5f), new float3(0f, 0f, 0f), 10f).Returns(false),

                // Point on segment boundaries
                new TestCaseData(new float3(0f, 0f, 0f), new float3(0f, 0f, 0f), 10f).Returns(true),
                new TestCaseData(new float3(10f, 5f, 5f), new float3(0f, 0f, 0f), 10f).Returns(false),

                // Mixed coordinates
                new TestCaseData(new float3(5f, -5f, 15f), new float3(0f, -10f, 10f), 10f).Returns(true),

                // Small segment size
                new TestCaseData(new float3(1.5f, 1.5f, 1.5f), new float3(1f, 1f, 1f), 1f).Returns(true),

                // Large segment size
                new TestCaseData(new float3(75f, 75f, 75f), new float3(0f, 0f, 0f), 100f).Returns(true)
            };
        }

        public static IEnumerable<TestCaseData> ScaleMultiplier_Data()
        {
            return new[]
            {
                // Basic cases
                new TestCaseData(0).Returns(1.0f),
                new TestCaseData(1).Returns(2.0f),
                new TestCaseData(2).Returns(4.0f),
                new TestCaseData(3).Returns(8.0f),

                // Negative scales
                new TestCaseData(-1).Returns(0.5f),
                new TestCaseData(-2).Returns(0.25f),
                new TestCaseData(-3).Returns(0.125f),

                // Large scales
                new TestCaseData(10).Returns(1024.0f),
                new TestCaseData(-10).Returns(1.0f / 1024.0f),

                // Edge cases
                new TestCaseData(int.MinValue).Returns(0.0f), // Due to floating point limitations
                new TestCaseData(int.MaxValue).Returns(float.PositiveInfinity) // Due to floating point limitations
            };
        }
    }
}