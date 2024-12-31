using Unity.Mathematics;

namespace Utility.Math
{
    /// <summary>Provides extension methods for <see cref="int" /> type.</summary>
    public static class IntExtensions
    {
        /// <summary>
        ///     Converts an integer between 0 and 7 to a <see cref="bool3" />.
        ///     The integer is treated as a 3-bit value where each bit corresponds to one component of the <see cref="bool3" />.
        /// </summary>
        /// <param name="value">The integer value between 0 and 7 to convert.</param>
        /// <returns>
        ///     A <see cref="bool3" /> where each component is <c>true</c> if the corresponding bit is 1, or <c>false</c> if
        ///     the bit is 0.
        /// </returns>
        /// <remarks>
        ///     The integer is treated as a 3-bit number, where:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>Bit 0 (value 1) maps to the <c>x</c> component</description>
        ///         </item>
        ///         <item>
        ///             <description>Bit 1 (value 2) maps to the <c>y</c> component</description>
        ///         </item>
        ///         <item>
        ///             <description>Bit 2 (value 4) maps to the <c>z</c> component</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public static bool3 ToBool3(this int value)
        {
            value = math.clamp(value, 0, 7);

            return new bool3(
                (value & 1) != 0,
                (value & 2) != 0,
                (value & 4) != 0
            );
        }
    }
}