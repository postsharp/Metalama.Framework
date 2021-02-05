using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    sealed class AdviceDriver
    {
        public AdviceInstanceResult GetResult(AdviceInstance adviceInstance)
        {
            Transformation transformation = adviceInstance.Advice switch
            {
                OverrideMethodAdvice overrideMethod => new OverriddenMethod( overrideMethod.TargetDeclaration, overrideMethod.Transformation.MethodBody ),
                _ => throw new NotImplementedException()
            };

            return new( ImmutableArray.Create<Diagnostic>(), ImmutableArray.Create( transformation ) );
        }
    }
}
