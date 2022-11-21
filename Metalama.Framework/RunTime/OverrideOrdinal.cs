// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Metalama.Framework.RunTime
{
    /// <summary>
    /// Contains helper types for ordinals.
    /// </summary>
    public static class OverrideOrdinal
    {
        /// <summary>
        /// Ordinal for digit 0.
        /// </summary>
        public readonly struct _0 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 1.
        /// </summary>
        public readonly struct _1 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 2.
        /// </summary>
        public readonly struct _2 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 3.
        /// </summary>
        public readonly struct _3 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 4.
        /// </summary>
        public readonly struct _4 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 5.
        /// </summary>
        public readonly struct _5 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 6.
        /// </summary>
        public readonly struct _6 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 7.
        /// </summary>
        public readonly struct _7 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 8.
        /// </summary>
        public readonly struct _8 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for digit 9.
        /// </summary>
        public readonly struct _9 : IOverridenByOrdinal { }

        /// <summary>
        /// Ordinal for 2 digits.
        /// </summary>
        /// <typeparam name="TOrdinal1">First digit.</typeparam>
        /// <typeparam name="TOrdinal2">Second digit.</typeparam>
        public readonly struct C<TOrdinal1, TOrdinal2> : IOverridenByOrdinal
            where TOrdinal1 : struct, IOverridenByOrdinal
            where TOrdinal2 : struct, IOverridenByOrdinal { }
    }
}