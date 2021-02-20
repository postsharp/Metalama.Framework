// unset

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// An implementation of <see cref="IAspectSource"/> backed by delegates.
    /// </summary>
    internal class AspectSource : IAspectSource
    {
        private readonly ImmutableMultiValueDictionary<string, Func<IEnumerable<AspectInstance>>> _sources;

        public AspectSource( ImmutableMultiValueDictionary<string, Func<IEnumerable<AspectInstance>>> sources )
        {
            this._sources = sources;
        }

        public IEnumerable<INamedType> AspectTypes => throw new NotImplementedException();

        public IEnumerable<AspectInstance> GetAspectInstances( INamedType aspectType )
        {
            return this._sources[aspectType.Name].SelectMany( s => s() );
        }
    }
}