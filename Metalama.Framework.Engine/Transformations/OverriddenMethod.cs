// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Metalama.Framework.Code.RefKind;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Method override, which expands a template.
    /// </summary>
    internal sealed class OverriddenMethod : OverriddenMember
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public BoundTemplateMethod BoundTemplate { get; }

        public OverriddenMethod( Advice advice, IMethod targetMethod, BoundTemplateMethod boundTemplate, IObjectReader tags )
            : base( advice, targetMethod, tags )
        {
            Invariant.Assert( !boundTemplate.IsNull );

            this.BoundTemplate = boundTemplate;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var proceedExpression = ProceedHelper.CreateProceedDynamicExpression(
                context.SyntaxGenerationContext,
                this.CreateInvocationExpression( context.SyntaxGenerationContext ),
                this.BoundTemplate,
                this.OverriddenDeclaration );

            var metaApi = MetaApi.ForMethod(
                this.OverriddenDeclaration,
                new MetaApiProperties(
                    context.DiagnosticSink,
                    this.BoundTemplate.Template.Cast(),
                    this.Tags,
                    this.Advice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.Advice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.Advice.TemplateInstance.Instance,
                metaApi,
                (CompilationModel) this.OverriddenDeclaration.Compilation,
                context.LexicalScopeProvider.GetLexicalScope( this.OverriddenDeclaration ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                this.BoundTemplate,
                proceedExpression,
                this.Advice.AspectLayerId );

            var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( this.BoundTemplate.Template.Declaration! );

            if ( !templateDriver.TryExpandDeclaration( expansionContext, this.BoundTemplate.TemplateArguments, out var newMethodBody ) )
            {
                // Template expansion error.
                return Enumerable.Empty<IntroducedMember>();
            }

            TypeSyntax? returnType = null;

            var modifiers = this.OverriddenDeclaration.GetSyntaxModifierList();

            if ( !this.OverriddenDeclaration.IsAsync )
            {
                if ( this.BoundTemplate.Template.MustInterpretAsAsyncTemplate() )
                {
                    // If the template is async but the overridden declaration is not, we have to add an async modifier.
                    modifiers = modifiers.Add( Token( SyntaxKind.AsyncKeyword ) );
                }
            }
            else
            {
                if ( !this.BoundTemplate.Template.MustInterpretAsAsyncTemplate() )
                {
                    // If the template is not async but the overridden declaration is, we have to remove the async modifier.
                    modifiers = TokenList( modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) );
                }

                // If the template is async and the target declaration is `async void`, and regardless of the async flag the template, we have to change the type to ValueTask, otherwise
                // it is not awaitable

                if ( TypeExtensions.Equals( this.OverriddenDeclaration.ReturnType, SpecialType.Void ) )
                {
                    returnType = context.SyntaxGenerator.Type(
                        this.OverriddenDeclaration.GetCompilationModel().Factory.GetSpecialType( SpecialType.ValueTask ).GetSymbol() );
                }
            }

            returnType ??= context.SyntaxGenerator.Type( this.OverriddenDeclaration.ReturnType.GetSymbol() );

            var introducedMethod = MethodDeclaration(
                List<AttributeListSyntax>(),
                modifiers,
                returnType,
                null,
                Identifier(
                    context.IntroductionNameProvider.GetOverrideName(
                        this.OverriddenDeclaration.DeclaringType,
                        this.Advice.AspectLayerId,
                        this.OverriddenDeclaration ) ),
                context.SyntaxGenerator.TypeParameterList( this.OverriddenDeclaration ),
                context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration ),
                context.SyntaxGenerator.ConstraintClauses( this.OverriddenDeclaration ),
                newMethodBody,
                null );

            var overrides = new[]
            {
                new IntroducedMember(
                    this,
                    introducedMethod,
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };

            return overrides;
        }

        private ExpressionSyntax CreateInvocationExpression( SyntaxGenerationContext generationContext )
            => InvocationExpression(
                this.CreateMemberAccessExpression( AspectReferenceTargetKind.Self, generationContext ),
                ArgumentList(
                    SeparatedList(
                        this.OverriddenDeclaration.Parameters.Select(
                            p =>
                            {
                                var refKind = p.RefKind switch
                                {
                                    RefKind.None => default,
                                    RefKind.In => default,
                                    RefKind.Out => Token( SyntaxKind.OutKeyword ),
                                    RefKind.Ref => Token( SyntaxKind.RefKeyword ),
                                    _ => throw new AssertionFailedException()
                                };

                                return Argument( null, refKind, IdentifierName( p.Name ) );
                            } ) ) ) );
    }
}