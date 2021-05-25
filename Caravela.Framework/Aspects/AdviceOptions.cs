// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using System.ComponentModel;

namespace Caravela.Framework.Aspects
{
    public class AdviceOptions
    {
        public static AdviceOptions Default { get; } = new( null, ImmutableDictionary<string, object?>.Empty );

        internal AspectLinkerOptions? LinkerOptions { get; }

        public ImmutableDictionary<string, object?> Tags { get; }

        internal AdviceOptions( AspectLinkerOptions? linkerOptions, ImmutableDictionary<string, object?> tags )
        {
            this.LinkerOptions = linkerOptions;
            this.Tags = tags;
        }

        // This cannot be internal because this is used from compile-time code in tests.
        [EditorBrowsable( EditorBrowsableState.Never )]
        public AdviceOptions WithLinkerOptions( bool forceNotInlineable ) => new( AspectLinkerOptions.Create( forceNotInlineable ), this.Tags );

        public AdviceOptions AddTag( string name, object? value ) => new( this.LinkerOptions, this.Tags.Add( name, value ) );

        public AdviceOptions WithTags( ImmutableDictionary<string, object?> tags ) => new( this.LinkerOptions, tags );
    }
}