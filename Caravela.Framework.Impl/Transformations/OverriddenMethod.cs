// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverriddenMethod : OverriddenMember
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public IMethod TemplateMethod { get; }

        public OverriddenMethod( Advice advice, IMethod overriddenDeclaration, IMethod templateMethod, AspectLinkerOptions? linkerOptions = null )
            : base( advice, overriddenDeclaration, linkerOptions )
        {
            Invariant.Assert( templateMethod != null );

            this.TemplateMethod = templateMethod;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var proceedExpression = new DynamicExpression(
                    this.CreateInvocationExpression(),
                    this.OverriddenDeclaration.ReturnType,
                    false );

                var metaApi = MetaApi.ForMethod(
                    this.OverriddenDeclaration,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        this.TemplateMethod.GetSymbol(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        context.ServiceProvider.GetService<AspectPipelineDescription>(),
                        proceedExpression ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    metaApi,
                    this.OverriddenDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope(this.OverriddenDeclaration),
                    context.ServiceProvider.GetService<SyntaxSerializationService>(),
                    (ICompilationElementFactory) this.OverriddenDeclaration.Compilation.TypeFactory );

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
                            Identifier( context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration ) ),
                            this.OverriddenDeclaration.GetSyntaxTypeParameterList(),
                            this.OverriddenDeclaration.GetSyntaxParameterList(),
                            this.OverriddenDeclaration.GetSyntaxConstraintClauses(),
                            newMethodBody,
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.Override,
                        this.LinkerOptions,
                        this.OverriddenDeclaration )
                };

                return overrides;
            }
        }

        private ExpressionSyntax CreateInvocationExpression()
        {
            return
                InvocationExpression(
                    CreateInvocationTarget()
                    .WithAspectReferenceAnnotation(
                        this.Advice.AspectLayerId,
                        AspectReferenceOrder.Default,
                        flags: AspectReferenceFlags.Inlineable ),
                    ArgumentList(
                        SeparatedList(
                            this.OverriddenDeclaration.Parameters.Select( p =>
                                Argument(
                                    null,
                                    p.RefKind switch
                                    {
                                        RefKind.None => default,
                                        RefKind.In => default,
                                        RefKind.Out => Token( SyntaxKind.OutKeyword ),
                                        RefKind.Ref => Token( SyntaxKind.RefKeyword ),
                                        _ => throw new AssertionFailedException(),
                                    },
                                    IdentifierName( p.Name ) ) ) ) ) );

            ExpressionSyntax CreateInvocationTarget()
            {
                return this.OverriddenDeclaration.IsStatic
                    ? IdentifierName( this.OverriddenDeclaration.Name )
                    : MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
            }
        }
    }
}