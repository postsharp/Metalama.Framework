using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    class AspectDriver : IAspectDriver
    {
        public INamedType AspectType { get; }

        private readonly ICompilation _compilation;

        private readonly IReactiveCollection<(IAttribute attribute, IMethod method)> _declarativeAdviceAttributes;

        public AspectDriver( INamedType aspectType, ICompilation compilation )
        {
            this.AspectType = aspectType;

            this._compilation = compilation;

            var iAdviceAttribute = compilation.GetTypeByReflectionType( typeof( IAdviceAttribute ) ).AssertNotNull();

            this._declarativeAdviceAttributes =
                from method in aspectType.Methods
                from attribute in method.Attributes
                where attribute.Type.Is( iAdviceAttribute )
                select (attribute, method);
        }

        internal AspectInstanceResult EvaluateAspect( AspectInstance aspectInstance )
        {
            var aspect = aspectInstance.Aspect;

            return aspectInstance.CodeElement switch
            {
                ICompilation compilation => this.EvaluateAspect( compilation, aspect ),
                INamedType type => this.EvaluateAspect( type, aspect ),
                IMethod method => this.EvaluateAspect( method, aspect ),
                _ => throw new NotImplementedException()
            };
        }

        private AspectInstanceResult EvaluateAspect<T>( T codeElement, IAspect aspect )
            where T : class, ICodeElement
        {
            if (aspect is not IAspect<T> aspectOfT)
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?
                var diagnostic = Diagnostic.Create(
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectElement, codeElement.GetSyntaxNode()?.GetLocation(), this.AspectType, codeElement, codeElement.Kind );

                return new( ImmutableList.Create( diagnostic ), ImmutableList.Create<AdviceInstance>(), ImmutableList.Create<AspectInstance>() );
            }

            var declarativeAdvices = this._declarativeAdviceAttributes.GetValue().Select( x => this.CreateDeclarativeAdvice( codeElement, x.attribute, x.method ) );

            var aspectBuilder = new AspectBuilder<T>( 
                codeElement, declarativeAdvices, new AdviceFactory( this._compilation, this.AspectType, aspect ) );

            aspectOfT.Initialize( aspectBuilder );

            return aspectBuilder.ToResult();
        }

        public const string OriginalMemberSuffix = "_Original";

        private AdviceInstance CreateDeclarativeAdvice<T>( T codeElement, IAttribute attribute, IMethod templateMethod ) where T : ICodeElement
        {
            throw new NotImplementedException();
        }
    }
}