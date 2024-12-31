using NUnit.Framework;
using Unity.Mathematics;
using static Tests.Editor.Utility.TerrainGeneration.SegmentOperationsTestData;
using static Utility.TerrainGeneration.SegmentOperations;

namespace Tests.Editor.Utility.TerrainGeneration
{
    [TestFixture]
    public class SegmentOperationsTests
    {
        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetCubeSize_Data))]
        public static float TestGetCubeSize(float baseCubeSize, int segScale)
        {
            return GetCubeSize(baseCubeSize, segScale);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetSegSize_Data))]
        public static float TestGetSegSize(float baseSegSize, int segScale)
        {
            return GetSegSize(baseSegSize, segScale);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetClosestSegPos_Data))]
        public static float3 TestGetClosestSegPos(float3 pos, float segSize)
        {
            return GetClosestSegPos(pos, segSize);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(GetClosestSegCenter_Data))]
        public static float3 TestGetClosestSegCenter(float3 pos, float segSize)
        {
            return GetClosestSegCenter(pos, segSize);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(PointWithinSeg_Data))]
        public static bool TestPointWithinSeg(float3 point, float3 segPos, float segSize)
        {
            return PointWithinSeg(point, segPos, segSize);
        }

        [Test]
        [TestCaseSource(typeof(SegmentOperationsTestData), nameof(ScaleMultiplier_Data))]
        public static float TestScaleMultiplier(int scale)
        {
            return ScaleMultiplier(scale);
        }
    }
}