// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class FabricResult
    {
        public ImmutableArray<IAspectSource> AspectSources { get; }

        public FabricResult( IFabricBuilderInternal builder ) : this( builder.AspectSources.ToImmutableArray() ) { }

        private FabricResult( ImmutableArray<IAspectSource> sources )
        {
            this.AspectSources = sources;
        }

        public FabricResult() : this( ImmutableArray<IAspectSource>.Empty ) { }

        public FabricResult Merge( FabricResult other ) => new( this.AspectSources.AddRange( other.AspectSources ) );
    }
}