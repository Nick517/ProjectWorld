using NUnit.Framework;
using Unity.Mathematics;
using Utility.SpacialPartitioning;
using static Tests.Editor.Utility.TerrainGeneration.SegmentOperationsTestData;

namespace Tests.Editor.Utility.TerrainGeneration
{
    [TestFixture]
    public class SegmentOperationsTests
    {
        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetCubeSize_Data))]
        public static float TestGetCubeSize(float baseCubeSize, int segScale)
        {
            return SegmentOperations.GetCubeSize(baseCubeSize, segScale);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetSegSize_Data))]
        public static float TestGetSegSize(float baseSegSize, int segScale)
        {
            return SegmentOperations.GetSegSize(baseSegSize, segScale);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetClosestSegPos_Data))]
        public static float3 TestGetClosestSegPos(float3 pos, float segSize)
        {
            return SegmentOperations.GetClosestSegPos(pos, segSize);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetClosestSegCenter_Data))]
        public static float3 TestGetClosestSegCenter(float3 pos, float segSize)
        {
            return SegmentOperations.GetClosestSegCenter(pos, segSize);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(PointWithinSeg_Data))]
        public static bool TestPointWithinSeg(float3 point, float3 segPos, float segSize)
        {
            return SegmentOperations.PointWithinSeg(point, segPos, segSize);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(ScaleMultiplier_Data))]
        public static float TestScaleMultiplier(int scale)
        {
            return SegmentOperations.ScaleMultiplier(scale);
        }
    }
}