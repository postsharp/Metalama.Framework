// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Conflict behavior of introduction advices.
    /// </summary>
    public enum ConflictBehavior
    {
        /// <summary>
        /// Advice fails with a compilation error if the member already exists in the target declaration. Same as <see cref="Fail"/>.
        /// </summary>
        Default = Fail,

        /// <summary>
        /// Advice fails with a compilation error if the member exists in the target declaration.
        /// </summary>
        Fail = 0,

        /// <summary>
        /// Advice is ignored if the member already exists in the target declaration.
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// Advice attempts to override the existing member or fails with a compilation error if that is not possible.
        /// </summary>
        Override = 2,

        /// <summary>
        /// If the member already exists, the advice attempts to redefine it using <c>new</c> or fails with a compilation error if that is not possible.
        /// </summary>
        New = 3,

        /// <summary>
        /// Advice introduces attempts to introduce template's members into the target type. For non-type members the behavior is the same as <see cref="Merge"/>.
        /// </summary>
        Merge = 4
    }
}
