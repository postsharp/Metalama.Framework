// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    internal class AspectDriver : IAspectDriver
    {
        private readonly IReadOnlyList<(IAttribute Attribute, IMethod Method)> _declarativeAdviceAttributes;

        public INamedType AspectType { get; }

        public AspectDriver( INamedType aspectType, CompilationModel compilation )
        {
            this.AspectType = aspectType;

            var iAdviceAttribute = compilation.Factory.GetTypeByReflectionType( typeof( IAdviceAttribute ) ).AssertNotNull();

            this._declarativeAdviceAttributes =
                (from method in aspectType.Methods
                 from attribute in method.Attributes
                 where attribute.Type.Is( iAdviceAttribute )
                 select (attribute, method)).ToList();
        }

        internal AspectInstanceResult EvaluateAspect( AspectInstance aspectInstance )
        {

            return aspectInstance.CodeElement switch
            {
                ICompilation compilation => this.EvaluateAspect( compilation, aspectInstance ),
                INamedType type => this.EvaluateAspect( type, aspectInstance ),
                IMethod method => this.EvaluateAspect( method, aspectInstance ),
                _ => throw new NotImplementedException()
            };
        }

        private AspectInstanceResult EvaluateAspect<T>( T codeElement, AspectInstance aspect )
            where T : class, ICodeElement
        {
            if ( aspect.Aspect is not IAspect<T> aspectOfT )
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?

                // Get the code model type for the reflection type so we have better formatting of the diagnostic.
                var interfaceType = this.AspectType.Compilation.TypeFactory.GetTypeByReflectionType( typeof( IAspect<T> ) );

                var diagnostic =
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectElement.CreateDiagnostic(
                        codeElement.GetLocationForDiagnosticReport(),
                        (this.AspectType, codeElement.ElementKind, codeElement, interfaceType) );

                return new(
                    false,
                    new ImmutableDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<IAdvice>.Empty,
                    ImmutableArray<IAspectSource>.Empty );
            }

            var declarativeAdvices = this._declarativeAdviceAttributes.Select( x => this.CreateDeclarativeAdvice( aspect, codeElement, x.Attribute, x.Method ) );

            var aspectBuilder = new AspectBuilder<T>(
                codeElement, declarativeAdvices, new AdviceFactory( this.AspectType, aspect ) );

            using ( DiagnosticContext.WithDefaultLocation( aspectBuilder.DefaultScope?.LocationForDiagnosticReport ) )
            {
                aspectOfT.Initialize( aspectBuilder );
            }

            return aspectBuilder.ToResult();
        }

        public const string OriginalMemberSuffix = "_Original";

        private IAdvice CreateDeclarativeAdvice<T>( AspectInstance aspect, T codeElement, IAttribute attribute, IMethod templateMethod )
            where T : ICodeElement
        {
            return AdviceAttributeFactory.CreateAdvice( attribute, aspect, codeElement, templateMethod );
        }
    }
}