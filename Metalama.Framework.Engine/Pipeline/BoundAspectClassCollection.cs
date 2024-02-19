// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    internal sealed class BoundAspectClassCollection : IReadOnlyCollection<IBoundAspectClass>, IReadOnlyCollection<IAspectClass>
    {
        public ImmutableDictionary<string, IBoundAspectClass> Dictionary { get; }

        public IBoundAspectClass this[ string typeName ] => this.Dictionary[typeName];

        public BoundAspectClassCollection( IEnumerable<IBoundAspectClass> aspectClasses )
        {
            this.Dictionary = aspectClasses.ToImmutableDictionary( c => c.FullName, c => c );
            
            this.HashCode = HashUtilities.HashStrings( this.Dictionary.Keys );
        }

        IEnumerator<IAspectClass> IEnumerable<IAspectClass>.GetEnumerator() => this.GetEnumerator();

        public IEnumerator<IBoundAspectClass> GetEnumerator() => this.Dictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.Dictionary.Count;

        public ulong HashCode { get; }
    }
}