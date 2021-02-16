using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{

    internal interface IAspectSource
    {
        /// <summary>
        /// Returns a set of <see cref="AspectInstance"/> of a given type. This method is called when the given aspect
        /// type is being processed, not before.
        /// </summary>
        IEnumerable<AspectInstance> GetAspectInstances( INamedType aspectType );
    }

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

        public IEnumerable<AspectInstance> GetAspectInstances( INamedType aspectType )
        {
            return this._sources[aspectType.Name].SelectMany( s => s() );
        }
    }
}