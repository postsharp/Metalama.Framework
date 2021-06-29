// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    /// <summary>
    /// Specifies behavior of the aspect linker on the advice.
    /// </summary>
    [CompileTimeOnly]
    internal sealed class AspectLinkerOptions
    {
        private const string _tagName = "__ForceNotInlineable";

        public bool ForceNotInlineable { get; }

        public static AspectLinkerOptions Default { get; } = new();

        private AspectLinkerOptions( bool forceNotInlineable = false )
        {
            this.ForceNotInlineable = forceNotInlineable;
        }

        public static AspectLinkerOptions FromTags( Dictionary<string, object?>? tags )
        {
            if ( tags != null && tags.ContainsKey( _tagName ) )
            {
                return new AspectLinkerOptions( true );
            }
            else
            {
                return Default;
            }
        }

        public IReadOnlyDictionary<string, object?>? ToTags()
        {
            if ( this.ForceNotInlineable )
            {
                return ImmutableDictionary.Create<string, object?>().Add( _tagName, _tagName );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates linker options.
        /// </summary>
        /// <param name="forceNotInlineable">Forces the result of the advice not to be inlineable by the aspect linker.</param>
        /// <returns>AspectLinkerOptions object.</returns>
        public static AspectLinkerOptions Create( bool forceNotInlineable ) => new( forceNotInlineable );
    }
}