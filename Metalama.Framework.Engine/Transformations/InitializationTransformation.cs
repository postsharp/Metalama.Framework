// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
    internal class InitializationTransformation : INonObservableTransformation, IMemberIntroduction, ICodeTransformationSource, IHierarchicalTransformation
    {
        private readonly IMemberOrNamedType _targetDeclaration;
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
            IMemberOrNamedType targetDeclaration, 
            TypeDeclarationSyntax typeDeclaration, 
            IReadOnlyList<IConstructor> constructors, 
            TemplateMember<IMethod> template, 
            InitializationReason reason )
        {
            this._targetDeclaration = targetDeclaration;
            this._mainTransformation = mainTransformation;
            this._typeDeclaration = typeDeclaration;
            this._constructors = constructors;
            this._template = template;
            this._reason = reason;
            this.Advice = advice;
            this.InsertPosition = new InsertPosition( InsertPositionRelation.Within, typeDeclaration );
        }

        [Memo]
        public IReadOnlyList<IHierarchicalTransformation> Dependencies => this._mainTransformation != null ? new[] { this._mainTransformation } : Array.Empty<IHierarchicalTransformation>();

        public TransformationInitializationResult? Initialize( in InitializationContext context )
        {
            if ( this._mainTransformation == null )
            {
                var targetType = this._targetDeclaration switch
                {
                    INamedType t => t,
                    _ => this._targetDeclaration.DeclaringType.AssertNotNull(),
                };

                return new InitializationResult(
                    context.IntroductionNameProvider.GetInitializationName( targetType, this.Advice.AspectLayerId, this._targetDeclaration, this._reason ) );
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
                    this._targetDeclaration,
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
                    (CompilationModel) this._targetDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( this._targetDeclaration ),
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
                        this._reason.HasFlag(InitializationReason.TypeConstructing)
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
                        new IntroducedMember( this, DeclarationKind.Method, methodDeclaration, this.Advice.AspectLayerId, IntroducedMemberSemantic.Initialization, this._targetDeclaration )
                    };
            }
            else
            {
                return Enumerable.Empty<IntroducedMember>();
            }
        }

        public IEnumerable<ICodeTransformation> GetCodeTransformations( in CodeTransformationSourceContext context )
        {
            string introductionName;

            if (this._mainTransformation == null)
            {
                introductionName = ((InitializationResult) context.InitializationResult.AssertNotNull()).IntroductionName;
            }
            else
            {
                introductionName = ((InitializationResult) context.DependencyInitializationResults[this._mainTransformation].AssertNotNull()).IntroductionName;
            }

            var codeTransformations = new List<ICodeTransformation>();

            foreach (var constructor in this._constructors)
            {
                codeTransformations.Add( new CodeTransformation( this, constructor, introductionName ) );
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

        private class CodeTransformation : ICodeTransformation
        {
            public InitializationTransformation Parent { get; }

            ITransformation ICodeTransformation.Parent => this.Parent;

            public Advice Advice => this.Parent.Advice;

            public IMethodBase TargetDeclaration { get; }

            public string IntroductionName { get; }

            public CodeTransformation( InitializationTransformation parent, IMethodBase targetDeclaration, string introductionName )
            {
                this.Parent = parent;
                this.TargetDeclaration = targetDeclaration;
                this.IntroductionName = introductionName;
            }

            public void EvaluateSyntaxNode( CodeTransformationContext context )
            {
                switch ( context.TargetNode )
                {
                    case null:
                        // This is temporary for implicit constructors.
                        // Constructor without a body.
                        context.AddMark( CodeTransformationOperator.InsertHead, this.GetCallSyntax() );
                        context.Decline();
                        break;

                    case BlockSyntax:
                        // Insert the syntax into the beginning of a body and decline the subtree.
                        context.AddMark( CodeTransformationOperator.InsertHead, this.GetCallSyntax() );
                        context.Decline();
                        break;

                    case EqualsValueClauseSyntax:
                        // Insert the syntax into the beginning of the body and decline the subtree.
                        context.AddMark( CodeTransformationOperator.InsertHead, this.GetCallSyntax() );
                        context.Decline();
                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }

            private SyntaxNode GetCallSyntax()
            {
                if ( this.Parent._reason.HasFlag( InitializationReason.TypeConstructing ) )
                {
                    return
                        ExpressionStatement(
                            InvocationExpression(
                                IdentifierName( this.IntroductionName ),
                                ArgumentList() ) );
                }
                else
                {
                    return
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName( this.IntroductionName ) ),
                                ArgumentList() ) );
                }
            }
        }
    }
}