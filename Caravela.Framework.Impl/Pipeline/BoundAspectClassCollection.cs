// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class BoundAspectClassCollection : IReadOnlyCollection<IBoundAspectClass>
    {
        private readonly ImmutableDictionary<string, IBoundAspectClass> _aspectClassesByName;

        public IBoundAspectClass this[ string typeName ] => this._aspectClassesByName[typeName];

        public BoundAspectClassCollection( IEnumerable<IBoundAspectClass> aspectClasses )
        {
            this._aspectClassesByName = aspectClasses.ToImmutableDictionary( c => c.FullName, c => c );
        }

        public IEnumerator<IBoundAspectClass> GetEnumerator() => this._aspectClassesByName.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this._aspectClassesByName.Count;
    }
}