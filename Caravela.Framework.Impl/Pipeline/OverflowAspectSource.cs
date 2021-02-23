using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// An <see cref="IAspectSource"/> that stores aspect sources that are not a part of the current <see cref="PipelineStage"/>.
    /// </summary>
    internal class OverflowAspectSource : IAspectSource
    {
        private List<(IAspectSource Source, INamedType Type)> _aspectSources = new();

        public AspectSourcePriority Priority => AspectSourcePriority.Aggregate;

        public IEnumerable<INamedType> AspectTypes => this._aspectSources.Select( a => a.Type ).Distinct();

        public IEnumerable<ICodeElement> GetExclusions( INamedType aspectType ) => Enumerable.Empty<ICodeElement>();

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