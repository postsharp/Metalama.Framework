using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class OverflowAspectSource : IAspectSource
    {
        private List<(IAspectSource Source, INamedType Type)> _aspectSources = new ();

        public IEnumerable<INamedType> AspectTypes => this._aspectSources.Select( a => a.Type ).Distinct();

        public IEnumerable<AspectInstance> GetAspectInstances( INamedType aspectType )
            => this._aspectSources
                .Where( s => s.Type.Equals( aspectType ) )
                .Select( a => a.Source )
                .Distinct()
                .SelectMany( a => a.GetAspectInstances( aspectType ) );

        public void Add( IAspectSource aspectSource, INamedType aspectType )
        {
            this._aspectSources.Add( (aspectSource, aspectType) );
        }
    }
}