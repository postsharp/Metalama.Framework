// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal abstract class MemberBuilder : MemberOrNamedTypeBuilder, IMemberBuilder, IMemberImpl
    {
        protected MemberBuilder( Advice parentAdvice, INamedType declaringType, IObjectReader tags ) : base( parentAdvice, declaringType )
        {
            this.Tags = tags;
        }

        public bool IsImplicit => false;

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public override string ToString() => this.DeclaringType + "." + this.Name;

        public abstract bool IsExplicitInterfaceImplementation { get; }

        public bool IsVirtual { get; set; }

        public bool IsAsync { get; set; }

        public bool IsOverride { get; set; }

        public override bool IsDesignTime => !this.IsOverride;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.DeclaringType.ToDisplayString( format, context ) + "." + this.Name;

        protected IObjectReader Tags { get; }

        public void ApplyTemplateAttribute( TemplateAttribute templateAttribute )
        {
            if ( templateAttribute.Name != null )
            {
                this.Name = templateAttribute.Name;
            }

            if ( templateAttribute.GetIsSealed().HasValue )
            {
                this.IsSealed = templateAttribute.GetIsSealed()!.Value;
            }

            if ( templateAttribute.GetAccessibility().HasValue )
            {
                this.Accessibility = templateAttribute.GetAccessibility()!.Value;
            }

            if ( templateAttribute.GetIsVirtual().HasValue )
            {
                this.IsVirtual = templateAttribute.GetIsVirtual().HasValue;
            }
        }

        public abstract IMember? OverriddenMember { get; }

        public override bool CanBeInherited => this.IsVirtual && !this.IsSealed && ((IDeclarationImpl) this.DeclaringType).CanBeInherited;

        internal bool GetInitializerExpressionOrMethod<T>(
            in MemberIntroductionContext context,
            IType targetType,
            IExpression? initializerExpression,
            TemplateMember<T> initializerTemplate,
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

            if ( context.SyntaxGenerationContext.IsPartial && (initializerExpression != null || initializerTemplate.IsNotNull) )
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
                initializerExpressionSyntax = ((IUserExpression) initializerExpression).ToRunTimeExpression().Syntax;

                return true;
            }
            else if ( initializerTemplate.IsNotNull )
            {
                initializerExpressionSyntax = null;

                if ( !this.TryExpandInitializerTemplate( context, initializerTemplate, out var initializerBlock ) )
                {
                    // Template expansion error.
                    initializerMethodSyntax = null;
                    initializerExpressionSyntax = null;

                    return false;
                }

                // If the initializer block contains only a single return statement, 
                if ( initializerBlock.Statements.Count == 1
                     && initializerBlock.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement )
                {
                    initializerMethodSyntax = null;
                    initializerExpressionSyntax = returnStatement.Expression;

                    return true;
                }

                var initializerName = context.IntroductionNameProvider.GetInitializerName( this.DeclaringType, this.ParentAdvice.AspectLayerId, this );

                if ( initializerBlock != null )
                {
                    initializerExpressionSyntax = InvocationExpression( IdentifierName( initializerName ) );

                    initializerMethodSyntax =
                        MethodDeclaration(
                            List<AttributeListSyntax>(),
                            TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) ),
                            context.SyntaxGenerator.Type( targetType.GetSymbol() ),
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

                    return true;
                }
            }
            else
            {
                initializerMethodSyntax = null;
                initializerExpressionSyntax = null;

                return true;
            }
        }

        private bool TryExpandInitializerTemplate<T>(
            MemberIntroductionContext context,
            TemplateMember<T> initializerTemplate,
            [NotNullWhen( true )] out BlockSyntax? expression )
            where T : class, IMember
        {
            var metaApi = MetaApi.ForInitializer(
                this,
                new MetaApiProperties(
                    context.DiagnosticSink,
                    initializerTemplate.Cast(),
                    this.Tags,
                    this.ParentAdvice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.ParentAdvice.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default ) );

            var expansionContext = new TemplateExpansionContext(
                this.ParentAdvice.TemplateInstance.Instance,
                metaApi,
                this.Compilation,
                context.LexicalScopeProvider.GetLexicalScope( this ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                default,
                null,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( initializerTemplate.Declaration! );

            return templateDriver.TryExpandDeclaration( expansionContext, Array.Empty<object>(), out expression );
        }

        protected virtual SyntaxList<AttributeListSyntax> GetAttributeLists( in SyntaxGenerationContext syntaxGenerationContext )
        {
            var attributeLists = new List<AttributeListSyntax>();

            foreach ( var attributeBuilder in this.Attributes )
            {
                if ( attributeBuilder.Constructor.DeclaringType.Is( this.Compilation.Factory.GetTypeByReflectionType( typeof(TemplateAttribute) ) ) )
                {
                    // TODO: This is temporary logic - aspect-related attributes should be marked as compile time and all compile time attributes should be skipped.
                    continue;
                }

                attributeLists.Add( AttributeList( SingletonSeparatedList( attributeBuilder.GetSyntax( syntaxGenerationContext ) ) ) );
            }

            return List( attributeLists );
        }
    }
}