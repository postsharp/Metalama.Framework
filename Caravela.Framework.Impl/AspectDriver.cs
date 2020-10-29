using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

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
                INamedType type => EvaluateAspect( type, aspect ),
                IMethod method => EvaluateAspect( method, aspect )
            };
        }

        private static AspectInstanceResult EvaluateAspect<T>( T codeElement, IAspect aspect )
            where T : ICodeElement
        {
            if (aspect is not IAspect<T> aspectOfT)
            {
                // TODO: can this happen? if it can, produce a diagnostic instead
                throw new InvalidOperationException();
            }

            var aspectBuilder = new AspectBuilder<T>( codeElement );

            aspectOfT.Initialize( aspectBuilder );

            return aspectBuilder.ToResult();
        }
    }
}