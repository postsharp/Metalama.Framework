// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal class OverriddenMethod : INonObservableTransformation, IMemberIntroduction, IOverriddenDeclaration
    {
        public Advice Advice { get; }

        IDeclaration IOverriddenDeclaration.OverriddenDeclaration => this.OverriddenDeclaration;

        public IMethod OverriddenDeclaration { get; }

        public IMethod TemplateMethod { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, IMethod templateMethod, AspectLinkerOptions? linkerOptions = null )
        {
            Invariant.Assert( advice != null! );
            Invariant.Assert( overriddenDeclaration != null! );
            Invariant.Assert( templateMethod != null! );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
            this.TemplateMethod = templateMethod;
            this.LinkerOptions = linkerOptions;
        }

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree
            => this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
                ? introduction.TargetSyntaxTree
                : ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var methodName = context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    this.OverriddenDeclaration,
                    this.OverriddenDeclaration.Compilation,
                    new LinkerOverrideMethodProceedImpl(
                        this.Advice.AspectLayerId,
                        this.OverriddenDeclaration,
                        LinkerAnnotationOrder.Default,
                        context.SyntaxFactory ),
                    context.LexicalScope,
                    context.DiagnosticSink,
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    (ISyntaxFactory) this.OverriddenDeclaration.Compilation.TypeFactory,
                    this.Advice.AspectLayerId,
                    this.Advice.AspectBuilderTags );

                var templateDriver = this.Advice.Aspect.AspectClass.GetTemplateDriver( this.TemplateMethod );

                if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var newMethodBody ) )
                {
                    // Template expansion error.
                    return Enumerable.Empty<IntroducedMember>();
                }

                var overrides = new[]
                {
                    new IntroducedMember(
                        this,
                        MethodDeclaration(
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.GetSyntaxModifierList(),
                            this.OverriddenDeclaration.GetSyntaxReturnType(),
                            null,
                            Identifier( methodName ),
                            this.OverriddenDeclaration.GetSyntaxTypeParameterList(),
                            this.OverriddenDeclaration.GetSyntaxParameterList(),
                            this.OverriddenDeclaration.GetSyntaxConstraintClauses(),
                            newMethodBody,
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.MethodOverride,
                        this.LinkerOptions,
                        this.OverriddenDeclaration )
                };

                return overrides;
            }
        }

        public MemberDeclarationSyntax InsertPositionNode
        {
            get
            {
                // TODO: Select a good syntax reference if there are multiple (partial class, partial method).
                var methodSymbol = (this.OverriddenDeclaration as Method)?.Symbol;

                if ( methodSymbol != null )
                {
                    return methodSymbol.DeclaringSyntaxReferences.Select( x => (MethodDeclarationSyntax) x.GetSyntax() ).First();
                }

                var typeSymbol = ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol;

                return typeSymbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).First();
            }
        }
    }
}