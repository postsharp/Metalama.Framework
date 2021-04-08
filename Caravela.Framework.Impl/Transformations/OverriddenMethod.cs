// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{

    internal class OverriddenMethod : INonObservableTransformation, IMemberIntroduction, IOverriddenElement
    {
        public Advice Advice { get; }

        ICodeElement IOverriddenElement.OverriddenElement => this.OverriddenDeclaration;

        public IMethod OverriddenDeclaration { get; }

        public IMethod TemplateMethod { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, IMethod templateMethod, AspectLinkerOptions? linkerOptions = null )
        {
            Invariant.Assert( advice != null );
            Invariant.Assert( overriddenDeclaration != null );
            Invariant.Assert( templateMethod != null );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
            this.TemplateMethod = templateMethod;
            this.LinkerOptions = linkerOptions;
        }

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree =>
            this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
            ? introduction.TargetSyntaxTree
            :
            ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultLocation( this.OverriddenDeclaration.DiagnosticLocation ) )
            {
                var methodName = context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    this.OverriddenDeclaration,
                    this.OverriddenDeclaration.Compilation,
                    context.ProceedImplementationFactory.Get( this.Advice.AspectLayerId, this.OverriddenDeclaration ),
                    context.LexicalScope,
                    context.DiagnosticSink );

                var compiledTemplateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;

                var newMethodBody = new TemplateDriver(
                        this.Advice.Aspect.Aspect.GetType().GetMethod( compiledTemplateMethodName ).AssertNotNull() )
                    .ExpandDeclaration( expansionContext );

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
                        this.LinkerOptions )
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