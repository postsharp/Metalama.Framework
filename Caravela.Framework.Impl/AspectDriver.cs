using System.Collections.Immutable;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    class AspectDriver : IAspectDriver
    {
        public INamedType AspectType { get; }

        public AspectDriver( INamedType aspectType ) => this.AspectType = aspectType;

        internal AspectInstanceResult EvaluateAspect( AspectInstance aspectInstance )
        {
            var aspect = aspectInstance.Aspect;

            return aspectInstance.CodeElement switch
            {
                INamedType type => this.EvaluateAspect( type, aspect ),
                IMethod method => this.EvaluateAspect( method, aspect )
            };
        }

        private AspectInstanceResult EvaluateAspect<T>( T codeElement, IAspect aspect )
            where T : ICodeElement
        {
            if (aspect is not IAspect<T> aspectOfT)
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?
                var diagnostic = Diagnostic.Create(
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectElement, codeElement.GetSyntaxNode().GetLocation(), this.AspectType, codeElement, codeElement.Kind );

                return new( ImmutableArray.Create( diagnostic ), ImmutableArray.Create<AdviceInstance>(), ImmutableArray.Create<AspectInstance>() );
            }

            var aspectBuilder = new AspectBuilder<T>( codeElement );

            aspectOfT.Initialize( aspectBuilder );

            return aspectBuilder.ToResult();
        }
    }
}