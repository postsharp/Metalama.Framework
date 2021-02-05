using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl
{
    internal sealed class AdviceWeaverStage : PipelineStage
    {
        private readonly ImmutableArray<AspectPart> _aspectParts;

        public AdviceWeaverStage( IEnumerable<AspectPart> aspectParts )
        {
            this._aspectParts = aspectParts.ToImmutableArray();
        }

        public override AspectCompilation Transform( AspectCompilation input )
        {
            var compilation = input;

            foreach ( var aspectPart in this._aspectParts )
            {
                compilation = aspectPart.Transform( compilation );
            }

            return compilation;
        }
    }
}
