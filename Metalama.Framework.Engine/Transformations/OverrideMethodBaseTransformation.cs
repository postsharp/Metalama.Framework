// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class OverrideMethodBaseTransformation : OverrideMemberTransformation
{
    protected new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

    protected OverrideMethodBaseTransformation( Advice advice, IMethod targetMethod, IObjectReader tags )
        : base( advice, targetMethod, tags ) { }

    protected SyntaxUserExpression CreateProceedExpression( MemberInjectionContext context, TemplateKind templateKind )
        => ProceedHelper.CreateProceedDynamicExpression(
            context.SyntaxGenerationContext,
            this.CreateInvocationExpression( context.SyntaxGenerationContext, context.AspectReferenceSyntaxProvider ),
            templateKind,
            this.OverriddenDeclaration );

    protected InjectedMember[] GetInjectedMembersImpl( MemberInjectionContext context, BlockSyntax newMethodBody, bool isAsyncTemplate )
    {
        TypeSyntax? returnType = null;

        var modifiers = this.OverriddenDeclaration
            .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Async | ModifierCategories.Unsafe )
            .Insert( 0, SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

        if ( !this.OverriddenDeclaration.IsAsync )
        {
            if ( isAsyncTemplate )
            {
                // If the template is async but the overridden declaration is not, we have to add an async modifier.
                modifiers = modifiers.Add( SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.AsyncKeyword ) );
            }
        }
        else
        {
            if ( !isAsyncTemplate )
            {
                // If the template is not async but the overridden declaration is, we have to remove the async modifier.
                modifiers = TokenList( modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) );
            }

            // If the template is async and the target declaration is `async void`, and regardless of the async flag the template, we have to change the type to ValueTask, otherwise
            // it is not awaitable

            if ( this.OverriddenDeclaration.ReturnType.Equals( SpecialType.Void ) )
            {
                returnType = context.SyntaxGenerator.Type(
                    this.OverriddenDeclaration.GetCompilationModel().CompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ValueTask) ) );
            }
        }

        returnType ??= context.SyntaxGenerator.Type( this.OverriddenDeclaration.ReturnType.GetSymbol() );

        var introducedMethod = MethodDeclaration(
            List<AttributeListSyntax>(),
            modifiers,
            returnType.WithTrailingTriviaIfNecessary( ElasticSpace, context.SyntaxGenerationContext.Options ),
            null,
            Identifier(
                context.InjectionNameProvider.GetOverrideName(
                    this.OverriddenDeclaration.DeclaringType,
                    this.ParentAdvice.AspectLayerId,
                    this.OverriddenDeclaration ) ),
            context.SyntaxGenerator.TypeParameterList( this.OverriddenDeclaration, context.Compilation ),
            context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration, context.Compilation, true ),
            context.SyntaxGenerator.ConstraintClauses( this.OverriddenDeclaration ),
            newMethodBody,
            null );

        return new[]
        {
            new InjectedMember(
                this,
                introducedMethod,
                this.ParentAdvice.AspectLayerId,
                InjectedMemberSemantic.Override,
                this.OverriddenDeclaration )
        };
    }

    private ExpressionSyntax CreateInvocationExpression( SyntaxGenerationContext generationContext, AspectReferenceSyntaxProvider referenceSyntaxProvider )
        => this.OverriddenDeclaration.MethodKind switch
        {
            MethodKind.Default or MethodKind.ExplicitInterfaceImplementation =>
                InvocationExpression(
                    this.CreateMemberAccessExpression( AspectReferenceTargetKind.Self, generationContext ),
                    ArgumentList(
                        SeparatedList(
                            this.OverriddenDeclaration.Parameters.SelectAsReadOnlyList(
                                p => Argument( null, p.RefKind.InvocationRefKindToken(), IdentifierName( p.Name ) ) ) ) ) ),
            MethodKind.Finalizer =>
                referenceSyntaxProvider.GetFinalizerReference( this.ParentAdvice.AspectLayerId ),
            MethodKind.Operator =>
                referenceSyntaxProvider.GetOperatorReference(
                    this.ParentAdvice.AspectLayerId,
                    (IMethod) this.TargetDeclaration,
                    generationContext.SyntaxGenerator ),
            _ => throw new AssertionFailedException( $"Unsupported method kind: {this.OverriddenDeclaration} is {this.OverriddenDeclaration.MethodKind}." )
        };
}