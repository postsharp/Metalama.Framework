// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class MemberBuilder : MemberOrNamedTypeBuilder, IMemberBuilder, IMemberImpl
{
    private bool _isVirtual;
    private bool _isAsync;
    private bool _isOverride;

    protected MemberBuilder( INamedType declaringType, string name, Advice advice ) : base( advice, declaringType, name ) { }

    public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

    public override string ToString() => this.DeclaringType + "." + this.Name;

    public abstract bool IsExplicitInterfaceImplementation { get; }

    public bool IsVirtual
    {
        get => this._isVirtual;
        set
        {
            this.CheckNotFrozen();

            this._isVirtual = value;
        }
    }

    public bool IsAsync
    {
        get => this._isAsync;
        set
        {
            this.CheckNotFrozen();

            this._isAsync = value;
        }
    }

    public bool IsOverride
    {
        get => this._isOverride;
        set
        {
            this.CheckNotFrozen();

            this._isOverride = value;
        }
    }

    public bool HasImplementation => true;

    public bool IsDesignTime => !this.IsOverride && !this.IsNew;

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        => this.DeclaringType.ToDisplayString( format, context ) + "." + this.Name;

    public abstract IMember? OverriddenMember { get; }

    public override bool CanBeInherited => this.IsVirtual && !this.IsSealed && ((IDeclarationImpl) this.DeclaringType).CanBeInherited;

    private bool TryExpandInitializerTemplate<T>(
        Advice advice,
        MemberInjectionContext context,
        TemplateMember<T> initializerTemplate,
        IObjectReader tags,
        [NotNullWhen( true )] out BlockSyntax? expression )
        where T : class, IMember
    {
        var metaApi = MetaApi.ForInitializer(
            this,
            new MetaApiProperties(
                advice.SourceCompilation,
                context.DiagnosticSink,
                initializerTemplate.Cast(),
                tags,
                advice.AspectLayerId,
                context.SyntaxGenerationContext,
                advice.Aspect,
                context.ServiceProvider,
                MetaApiStaticity.Default ) );

        var expansionContext = new TemplateExpansionContext(
            context.ServiceProvider,
            advice.TemplateInstance.Instance,
            metaApi,
            context.LexicalScopeProvider.GetLexicalScope( this ),
            context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
            context.SyntaxGenerationContext,
            default,
            null,
            advice.AspectLayerId );

        var templateDriver = advice.TemplateInstance.TemplateClass.GetTemplateDriver( initializerTemplate.Declaration );

        return templateDriver.TryExpandDeclaration( expansionContext, Array.Empty<object>(), out expression );
    }

    internal bool GetInitializerExpressionOrMethod<T>(
        Advice advice,
        in MemberInjectionContext context,
        IType targetType,
        IExpression? initializerExpression,
        TemplateMember<T>? initializerTemplate,
        IObjectReader tags,
        out ExpressionSyntax? initializerExpressionSyntax,
        out MethodDeclarationSyntax? initializerMethodSyntax )
        where T : class, IMember
    {
        if ( context is null )
        {
            throw new ArgumentNullException( nameof(context) );
        }

        if ( targetType is null )
        {
            throw new ArgumentNullException( nameof(targetType) );
        }

        if ( context.SyntaxGenerationContext.IsPartial && (initializerExpression != null || initializerTemplate != null) )
        {
            // At design time when generating the partial code for source generators, we do not expand templates.
            // This may cause warnings in the constructor (because some fields will not be initialized)
            // but we will add that later. The main point is that we should not execute the template here.

            initializerMethodSyntax = null;
            initializerExpressionSyntax = null;

            return true;
        }

        if ( initializerExpression != null )
        {
            // TODO: Error about the expression type?
            initializerMethodSyntax = null;
            initializerExpressionSyntax = initializerExpression.ToExpressionSyntax( context.SyntaxGenerationContext );

            return true;
        }
        else if ( initializerTemplate != null )
        {
            if ( !this.TryExpandInitializerTemplate( advice, context, initializerTemplate, tags, out var initializerBlock ) )
            {
                // Template expansion error.
                initializerMethodSyntax = null;
                initializerExpressionSyntax = null;

                return false;
            }

            // If the initializer block contains only a single return statement, 
            if ( initializerBlock.Statements is [ReturnStatementSyntax { Expression: not null } returnStatement] )
            {
                initializerMethodSyntax = null;
                initializerExpressionSyntax = returnStatement.Expression;

                return true;
            }

            var initializerName = context.InjectionNameProvider.GetInitializerName( this.DeclaringType, advice.AspectLayerId, this );

            initializerExpressionSyntax = InvocationExpression( IdentifierName( initializerName ) );

            initializerMethodSyntax =
                MethodDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList(
                        Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ),
                        Token( SyntaxKind.StaticKeyword ).WithTrailingTrivia( Space ) ),
                    context.SyntaxGenerator.Type( targetType.GetSymbol() ).WithTrailingTrivia( Space ),
                    null,
                    Identifier( initializerName ),
                    null,
                    ParameterList(),
                    List<TypeParameterConstraintClauseSyntax>(),
                    initializerBlock,
                    null );

            return true;
        }
        else
        {
            initializerMethodSyntax = null;
            initializerExpressionSyntax = null;

            return true;
        }
    }
}