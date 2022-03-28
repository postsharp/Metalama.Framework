﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal class InitializationTransformation : INonObservableTransformation, IMemberIntroduction, IBodyTransformationSource, IHierarchicalTransformation
    {
        private readonly IMemberOrNamedType _initializedDeclaration;
        private readonly TypeDeclarationSyntax _typeDeclaration;
        private readonly IReadOnlyList<IConstructor> _constructors;
        private readonly TemplateMember<IMethod> _template;
        private readonly InitializationReason _reason;
        private readonly InitializationTransformation? _mainTransformation;

        public Advice Advice { get; }

        public InsertPosition InsertPosition { get; }

        public SyntaxTree TargetSyntaxTree => this._typeDeclaration.SyntaxTree;

        public InitializationTransformation(
            Advice advice,
            InitializationTransformation? mainTransformation,
            IMemberOrNamedType initializedDeclaration,
            TypeDeclarationSyntax typeDeclaration,
            IReadOnlyList<IConstructor> constructors,
            TemplateMember<IMethod> template,
            InitializationReason reason )
        {
            this._initializedDeclaration = initializedDeclaration;
            this._mainTransformation = mainTransformation;
            this._typeDeclaration = typeDeclaration;
            this._constructors = constructors;
            this._template = template;
            this._reason = reason;
            this.Advice = advice;
            this.InsertPosition = new InsertPosition( InsertPositionRelation.Within, typeDeclaration );
        }

        [Memo]
        public IReadOnlyList<IHierarchicalTransformation> Dependencies
            => this._mainTransformation != null ? new[] { this._mainTransformation } : Array.Empty<IHierarchicalTransformation>();

        public TransformationInitializationResult? Initialize( in InitializationContext context )
        {
            if ( this._mainTransformation == null )
            {
                var targetType = this._initializedDeclaration switch
                {
                    INamedType t => t,
                    _ => this._initializedDeclaration.DeclaringType.AssertNotNull()
                };

                return new InitializationResult(
                    context.IntroductionNameProvider.GetInitializationName(
                        targetType,
                        this.Advice.AspectLayerId,
                        this._initializedDeclaration,
                        this._reason ) );
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            if ( this._mainTransformation == null )
            {
                var initializationResult = (InitializationResult) context.InitializationResult.AssertNotNull();

                var metaApi = MetaApi.ForDeclaration(
                    this._initializedDeclaration,
                    new MetaApiProperties(
                        context.DiagnosticSink,
                        this._template.Cast(),
                        this.Advice.ReadOnlyTags,
                        this.Advice.AspectLayerId,
                        context.SyntaxGenerationContext,
                        this.Advice.Aspect,
                        context.ServiceProvider,
                        this._reason.HasFlag( InitializationReason.TypeConstructing ) ? MetaApiStaticity.AlwaysStatic : MetaApiStaticity.AlwaysInstance ) );

                var expansionContext = new TemplateExpansionContext(
                    this.Advice.TemplateInstance.Instance,
                    metaApi,
                    (CompilationModel) this._initializedDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( this._initializedDeclaration ),
                    context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                    context.SyntaxGenerationContext,
                    this._template,
                    null,
                    this.Advice.AspectLayerId );

                var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( this._template.Declaration! );

                if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var expandedBody ) )
                {
                    // Template expansion error.
                    return Enumerable.Empty<IntroducedMember>();
                }

                var methodDeclaration =
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        this._reason.HasFlag( InitializationReason.TypeConstructing )
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        PredefinedType( Token( SyntaxKind.VoidKeyword ) ),
                        null,
                        Identifier( initializationResult.IntroductionName ),
                        null,
                        ParameterList(),
                        List<TypeParameterConstraintClauseSyntax>(),
                        expandedBody,
                        null );

                return
                    new[]
                    {
                        new IntroducedMember(
                            this,
                            DeclarationKind.Method,
                            methodDeclaration,
                            this.Advice.AspectLayerId,
                            IntroducedMemberSemantic.Initialization,
                            this._initializedDeclaration )
                    };
            }
            else
            {
                return Enumerable.Empty<IntroducedMember>();
            }
        }

        public IEnumerable<IInsertStatementTransformation> GetBodyTransformations( in CodeTransformationSourceContext context )
        {
            string introductionName;

            if ( this._mainTransformation == null )
            {
                introductionName = ((InitializationResult) context.InitializationResult.AssertNotNull()).IntroductionName;
            }
            else
            {
                introductionName = ((InitializationResult) context.DependencyInitializationResults[this._mainTransformation].AssertNotNull()).IntroductionName;
            }

            var codeTransformations = new List<IInsertStatementTransformation>();

            foreach ( var constructor in this._constructors )
            {
                codeTransformations.Add( new InsertStatementTransformation( this, constructor, introductionName ) );
            }

            return codeTransformations;
        }

        private class InitializationResult : TransformationInitializationResult
        {
            public string IntroductionName { get; }

            public InitializationResult( string introductionName )
            {
                this.IntroductionName = introductionName;
            }
        }

        private class InsertStatementTransformation : IInsertStatementTransformation
        {
            private readonly InitializationTransformation _parent;
            private readonly string _introductionName;

            ITransformation IInsertStatementTransformation.Parent => this._parent;

            public Advice Advice => this._parent.Advice;

            public IMethodBase TargetDeclaration { get; }

            public IMemberOrNamedType ContextDeclaration => this._parent._initializedDeclaration;

            public InsertStatementTransformation( InitializationTransformation parent, IMethodBase targetDeclaration, string introductionName )
            {
                this._parent = parent;
                this.TargetDeclaration = targetDeclaration;
                this._introductionName = introductionName;
            }

            public InsertedStatement GetInsertedStatement( InsertStatementTransformationContext context )
            {
                StatementSyntax statement;

                if ( this._parent._reason.HasFlag( InitializationReason.TypeConstructing ) )
                {
                    statement =
                        ExpressionStatement(
                            InvocationExpression(
                                IdentifierName( this._introductionName ),
                                ArgumentList() ) );
                }
                else
                {
                    statement =
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName( this._introductionName ) ),
                                ArgumentList() ) );
                }

                return new InsertedStatement( InsertedStatementPosition.Beginning, statement );
            }
        }
    }
}