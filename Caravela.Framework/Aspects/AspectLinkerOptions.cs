// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Specifies behavior of the aspect linker on the advice.
    /// </summary>
    internal class AspectLinkerOptions
    {
        /// <summary>
        /// Default options.
        /// </summary>
        public static readonly AspectLinkerOptions Default = new(false, false);

        /// <summary>
        /// Gets a value indicating whether the linker inlining of this member is forcefully disabled, even if it would be otherwise possible.
        /// </summary>
        public bool ForceNotInlineable { get; }

        /// <summary>
        /// Gets a value indicating whether the linker discarding of this member (in case it not used) is forcefully disabled. If the member is deemed inlineable, this has no effect.
        /// </summary>
        public bool ForceNotDiscardable { get; }

        private AspectLinkerOptions( bool forceNotInlineable, bool forceNotDiscardable )
        {
            this.ForceNotInlineable = forceNotInlineable;
            this.ForceNotDiscardable = forceNotDiscardable;
        }

        /// <summary>
        /// Creates linker options.
        /// </summary>
        /// <param name="forceNotInlineable">Forces the result of the advice not to be inlineable by the aspect linker.</param>
        /// <param name="forceNotDiscardable">Forces the result of the advice not to be discardable by the aspect linker.</param>
        /// <returns>AspectLinkerOptions object.</returns>
        public static AspectLinkerOptions Create( bool forceNotInlineable = false, bool forceNotDiscardable = false ) => new( forceNotInlineable, forceNotDiscardable );
    }
}