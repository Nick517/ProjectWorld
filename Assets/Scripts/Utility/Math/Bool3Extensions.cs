using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Utility.Math
{
    /// <summary>Provides extension methods for <see cref="bool3" /> type.</summary>
    public static class Bool3Extensions
    {
        /// <summary>
        ///     Converts a <see cref="bool3" /> to an <see cref="int3" /> where <c>true</c> becomes <c>1</c> and <c>false</c>
        ///     becomes <c>-1</c>.
        /// </summary>
        /// <param name="bool3">The <see cref="bool3" /> value to convert.</param>
        /// <returns>
        ///     An <see cref="int3" /> where each component is <c>1</c> for <c>true</c> values and <c>-1</c> for <c>false</c>
        ///     values.
        /// </returns>
        /// <remarks>
        ///     This is useful for converting boolean flags into sign multipliers.
        ///     <br />For example:
        ///     <list type="bullet">
        ///         <item>
        ///             <description><c>bool3(false, false, false)</c> becomes <c>int3(-1, -1, -1)</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>bool3(true, false, true)</c> becomes <c>int3(1, -1, 1)</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>bool3(true, true, true)</c> becomes <c>int3(1, 1, 1)</c></description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public static int3 ToSign(this bool3 bool3)
        {
            return select(-1, 1, bool3);
        }

        /// <summary>Converts a <see cref="bool3" /> to an integer index using bitwise operations.</summary>
        /// <param name="bool3">The <see cref="bool3" /> value to convert.</param>
        /// <returns>
        ///     An <c>int</c> between <c>0</c> and <c>7</c> representing the unique index for this boolean combination.
        ///     The index is calculated by treating each component as a bit, where:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>X component represents bit 0 (value 1)</description>
        ///         </item>
        ///         <item>
        ///             <description>Y component represents bit 1 (value 2)</description>
        ///         </item>
        ///         <item>
        ///             <description>Z component represents bit 2 (value 4)</description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <remarks>
        ///     For example:
        ///     <list type="bullet">
        ///         <item>
        ///             <description><c>bool3(false, false, false)</c> returns <c>0</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>bool3(true, false, false)</c> returns <c>1</c></description>
        ///         </item>
        ///         <item>
        ///             <description><c>bool3(true, true, true)</c> returns <c>7</c></description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public static int ToIndex(this bool3 bool3)
        {
            return (bool3.x ? 1 : 0) | ((bool3.y ? 1 : 0) << 1) | ((bool3.z ? 1 : 0) << 2);
        }
    }
}