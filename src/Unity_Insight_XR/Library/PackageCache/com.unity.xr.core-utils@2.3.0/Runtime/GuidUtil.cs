using System;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility for dealing with <see cref="System.Guid"/> objects.
    /// </summary>
    public static class GuidUtil
    {
        /// <summary>
        /// Reconstructs a <see cref="Guid"/> from two <see cref="ulong"/> values representing the low and high bytes.
        /// </summary>
        /// <remarks>
        /// Use <see cref="GuidExtensions.Decompose(Guid, out ulong, out ulong)"/> to separate the `Guid`
        /// into its low and high components.
        /// </remarks>
        /// <param name="low">The low 8 bytes of the `Guid`.</param>
        /// <param name="high">The high 8 bytes of the `Guid`.</param>
        /// <returns>The `Guid` composed of <paramref name="low"/> and <paramref name="high"/>.</returns>
        public static Guid Compose(ulong low, ulong high)
        {
            return new Guid(
                (uint)((low   & 0x00000000ffffffff) >> 0),
                (ushort)((low & 0x0000ffff00000000) >> 32),
                (ushort)((low & 0xffff000000000000) >> 48),
                (byte)((high  & 0x00000000000000ff) >> 0),
                (byte)((high  & 0x000000000000ff00) >> 8),
                (byte)((high  & 0x0000000000ff0000) >> 16),
                (byte)((high  & 0x00000000ff000000) >> 24),
                (byte)((high  & 0x000000ff00000000) >> 32),
                (byte)((high  & 0x0000ff0000000000) >> 40),
                (byte)((high  & 0x00ff000000000000) >> 48),
                (byte)((high  & 0xff00000000000000) >> 56));
        }
    }
}
