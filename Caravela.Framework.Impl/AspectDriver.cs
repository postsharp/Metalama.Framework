﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal class AspectDriver : IAspectDriver
    {
        private readonly Compilation _compilation;
        private readonly List<(AttributeData Attribute, ISymbol Member)> _declarativeAdviceAttributes;

        public INamedTypeSymbol AspectType { get; }

        public AspectDriver( INamedTypeSymbol aspectType, Compilation compilation )
        {
            this._compilation = compilation;
            this.AspectType = aspectType;

            this._declarativeAdviceAttributes =
                (from member in aspectType.GetMembers()
                 from attribute in member.GetAttributes()
                 where attribute.AttributeClass?.Is( typeof(IAdviceAttribute) ) ?? false
                 select (attribute, member)).ToList();
        }

        internal AspectInstanceResult EvaluateAspect( AspectInstance aspectInstance )
        {
            return aspectInstance.CodeElement switch
            {
                ICompilation compilation => this.EvaluateAspect( compilation, aspectInstance ),
                INamedType type => this.EvaluateAspect( type, aspectInstance ),
                IMethod method => this.EvaluateAspect( method, aspectInstance ),
                IField field => this.EvaluateAspect( field, aspectInstance ),
                IProperty property => this.EvaluateAspect( property, aspectInstance ),
                IConstructor constructor => this.EvaluateAspect( constructor, aspectInstance ),
                IEvent @event => this.EvaluateAspect( @event, aspectInstance ),
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
                var interfaceType = this._compilation.GetTypeByReflectionType( typeof(IAspect<T>) ).AssertNotNull();

                var diagnostic =
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectElement.CreateDiagnostic(
                        codeElement.GetDiagnosticLocation(),
                        (this.AspectType, codeElement.ElementKind, codeElement, interfaceType) );

                return new AspectInstanceResult(
                    false,
                    new ImmutableDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<IAdvice>.Empty,
                    ImmutableArray<IAspectSource>.Empty );
            }

            var declarativeAdvices =
                this._declarativeAdviceAttributes.Select( x => CreateDeclarativeAdvice( aspect, codeElement, x.Attribute, x.Member ) );

            var compilationModel = (CompilationModel) codeElement.Compilation;

            var aspectBuilder = new AspectBuilder<T>(
                codeElement,
                declarativeAdvices,
                new AdviceFactory( compilationModel, compilationModel.Factory.GetNamedType( this.AspectType ), aspect ) );

            using ( DiagnosticContext.WithDefaultLocation( aspectBuilder.DefaultScope?.DiagnosticLocation ) )
            {
                aspectOfT.Initialize( aspectBuilder );
            }

            return aspectBuilder.ToResult();
        }

        public const string OriginalMemberSuffix = "_Original";

        private static IAdvice CreateDeclarativeAdvice<T>( AspectInstance aspect, T codeElement, AttributeData attribute, ISymbol templateMethod )
            where T : ICodeElement
        {
            return attribute.CreateAdvice( aspect, codeElement, ((CompilationModel) codeElement.Compilation).Factory.GetCodeElement( templateMethod ) );
        }
    }
}