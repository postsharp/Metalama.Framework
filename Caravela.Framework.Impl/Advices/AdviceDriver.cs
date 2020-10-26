using Caravela.Framework.Advices;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    sealed class AdviceDriver
    {
        public AdviceInstanceResult GetResult(AdviceInstance adviceInstance)
        {
            // TODO: this should be better designed
            Transformation transformation = adviceInstance.Advice switch
            {
                OverrideMethodAdvice overrideMethod => new OverriddenMethod(
                    overrideMethod.TargetDeclaration, overrideMethod.Transformation( ((MethodDeclarationSyntax) overrideMethod.TargetDeclaration.GetSyntaxNode()).Body ) )
            };

            return new( ImmutableArray.Create<Diagnostic>(), ImmutableArray.Create( transformation ) );
        }
    }
}
