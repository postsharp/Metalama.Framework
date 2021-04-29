// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Specifies behavior of the aspect linker on the advice.
    /// </summary>
    public class AspectLinkerOptions
    {
        /// <summary>
        /// Default options.
        /// </summary>
        public static readonly AspectLinkerOptions Default = new();

        /// <summary>
        /// Gets a value indicating whether the linker inlining of this member is forcefully disabled, even if it would be otherwise possible.
        /// </summary>
        public bool ForceNotInlineable { get; }

        private AspectLinkerOptions( bool forceNotInlineable = false )
        {
            this.ForceNotInlineable = forceNotInlineable;
        }

        /// <summary>
        /// Creates linker options.
        /// </summary>
        /// <param name="forceNotInlineable">Forces the result of the advice not to be inlineable by the aspect linker.</param>
        /// <returns>AspecTLinkerOptions object.</returns>
        public static AspectLinkerOptions Create( bool forceNotInlineable ) => new( forceNotInlineable );
    }
}