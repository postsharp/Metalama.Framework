// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable RedundantUsingDirective
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Compiles the source code of a template, annotated with <see cref="TemplateAnnotator"/>,
    /// to an executable template.
    /// </summary>
    internal sealed partial class TemplateCompilerRewriter : MetaSyntaxRewriter
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;

        private string? _currentStatementListVariableName;
        private List<StatementSyntax>? _currentMetaStatementList;
        private int _nextStatementListId;
        private ISymbol? _rootTemplateSymbol;

        public TemplateCompilerRewriter( Compilation compilation, SemanticAnnotationMap semanticAnnotationMap ) : base( compilation )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
        }

        private MemberAccessExpressionSyntax TemplateSyntaxFactoryMember( string name )
            => MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                this.MetaSyntaxFactory.Type( typeof( TemplateSyntaxFactory ) ),
                IdentifierName( name ) );

        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            if ( this._rootTemplateSymbol == null )
            {
                if ( node == null )
                {
                    throw new ArgumentNullException( nameof( node ) );
                }

                this._rootTemplateSymbol = this._semanticAnnotationMap.GetDeclaredSymbol( node );
                if ( this._rootTemplateSymbol == null )
                {
                    throw new AssertionFailedException( "Didn't find a symbol for a template method node." );
                }
            }

            return base.Visit( node );
        }

        private static ExpressionSyntax CastFromDynamic( TypeSyntax targetType, ExpressionSyntax expression ) =>
            CastExpression( targetType, CastExpression( PredefinedType( Token( SyntaxKind.ObjectKeyword ) ), expression ) );

        private static string NormalizeSpace( string statementComment )
        {
            // TODO: Replace this with something more GC-friendly.

            statementComment = statementComment.Replace( '\n', ' ' ).Replace( '\r', ' ' );

            while ( true )
            {
                var old = statementComment;
                statementComment = statementComment.Replace( "  ", " " );
                if ( old == statementComment )
                {
                    return statementComment;
                }
            }
        }

        protected override ExpressionSyntax TransformIdentifierName( IdentifierNameSyntax node )
        {
            if ( node.Identifier.Text == "var" )
            {
                // The simplified form does not work.
                return base.TransformIdentifierName( node );
            }
            else if ( node.Identifier.Text == "dynamic" )
            {
                // We change all dynamic into var in the template.
                return base.TransformIdentifierName( IdentifierName( Identifier( "var" ) ) );
            }

            // If the identifier is declared withing the template, then we want to generate a name that is unique in the template expansion context.
            // The new name is generated by calling TemplateSyntaxFactory.TemplateIdentifierName() during template expansion.
            // For identifiers declared outside of the template we just call the regular Roslyn SyntaxFactory.IdentifierName().
            var identifierSymbol = this._semanticAnnotationMap.GetSymbol( node );
            var isDeclaredWithinTemplate = SymbolEqualityComparer.Default.Equals( identifierSymbol?.ContainingSymbol, this._rootTemplateSymbol );
            var identifierFactoryMethodName = isDeclaredWithinTemplate
                ? (ExpressionSyntax) this.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.TemplateIdentifierName ) )
                : this.MetaSyntaxFactory.SyntaxFactoryMethod( nameof( IdentifierName ) );

            // The base implementation is very verbose, so we use this one.
            return InvocationExpression( identifierFactoryMethodName )
                .AddArgumentListArguments( Argument( this.MetaSyntaxFactory.LiteralExpression( node.Identifier.Text ) ) )
                .WithTemplateAnnotationsFrom( node );
        }

        protected override ExpressionSyntax TransformArgument( ArgumentSyntax node )
        {
            // The base implementation is very verbose, so we use this one:
            if ( node.RefKindKeyword.Kind() == SyntaxKind.None )
            {
                return this.MetaSyntaxFactory.Argument( this.Transform( node.Expression ) ).WithTemplateAnnotationsFrom( node );
            }
            else
            {
                return base.TransformArgument( node );
            }
        }

        /// <summary>
        /// Determines how a <see cref="SyntaxNode"/> should be transformed:
        /// <see cref="MetaSyntaxRewriter.TransformationKind.None"/> for compile-time code
        /// or <see cref="MetaSyntaxRewriter.TransformationKind.Transform"/> for run-time code.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override TransformationKind GetTransformationKind( SyntaxNode node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                return TransformationKind.None;
            }

            // Look for annotation on the parent, but stop at 'if' and 'foreach' statements,
            // which have special interpretation.
            for ( var parent = node.Parent;
                parent != null;
                parent = parent.Parent )
            {
                if ( parent.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
                {
                    return parent is IfStatementSyntax || parent is ForEachStatementSyntax || parent is ElseClauseSyntax || parent is WhileStatementSyntax
                        ? TransformationKind.Transform
                        : TransformationKind.None;
                }
            }

            return TransformationKind.Transform;
        }

        /// <summary>
        /// Determines if a symbol represents a call to <c>proceed()</c>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsProceed( SyntaxNode node )
        {
            var symbol = this._semanticAnnotationMap.GetSymbol( node );
            if ( symbol == null )
            {
                return false;
            }

            return symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof( ProceedAttribute ) );
        }

        /// <summary>
        /// Transforms an <see cref="ExpressionSyntax"/>, especially taking care of handling
        /// transitions between compile-time expressions and run-time expressions. At these transitions,
        /// compile-time expressions must be wrapped into literals.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override ExpressionSyntax TransformExpression( ExpressionSyntax expression )
        {
            switch ( expression.Kind() )
            {
                // TODO: We need to transform null and default values though. How to do this right then?
                case SyntaxKind.NullLiteralExpression:
                case SyntaxKind.DefaultLiteralExpression:
                    // new RuntimeExpression(LiteralExpression(Null/DefaultLiteralExpression), true)
                    return ObjectCreationExpression( this.MetaSyntaxFactory.Type( typeof( RuntimeExpression ) ) )
                        .AddArgumentListArguments(
                            Argument( this.MetaSyntaxFactory.LiteralExpression( this.Transform( expression.Kind() ) ) ),
                            Argument( this.Transform( true ) ) );

                case SyntaxKind.DefaultExpression:
                    // case SyntaxKind.NullLiteralExpression:
                    // case SyntaxKind.DefaultLiteralExpression:
                    // Don't transform default or null.
                    // When we do that, we can try to cast a dynamic 'default' or 'null' into a SyntaxFactory.
                    return expression;
            }

            var type = this._semanticAnnotationMap.GetType( expression )!;

            // A local function that wraps the input `expression` into a LiteralExpression.
            ExpressionSyntax CreateLiteralExpressionFactory( SyntaxKind syntaxKind )
            {
                // new RuntimeExpression(LiteralExpression(syntaxKind, Literal(expression)), type)
                return ObjectCreationExpression( this.MetaSyntaxFactory.Type( typeof( RuntimeExpression ) ) )
                    .AddArgumentListArguments(
                        Argument(
                            this.MetaSyntaxFactory.LiteralExpression(
                                this.Transform( syntaxKind ),
                                this.MetaSyntaxFactory.Literal( expression ) ) ),
                        Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( DocumentationCommentId.CreateReferenceId( type ) ) ) ) );
            }

            if ( type is IErrorTypeSymbol )
            {
                // There is a compile-time error. Return default.
                return LiteralExpression(
                    SyntaxKind.DefaultLiteralExpression,
                    Token( SyntaxKind.DefaultKeyword ) );
            }

            switch ( type.Name )
            {
                case "dynamic":
                    if ( this.IsProceed( expression ) )
                    {
                        // TODO: Emit a diagnostic. proceed() cannot be used as a general expression but only in
                        // specifically supported statements, i.e. variable assignments and return.
                        throw new AssertionFailedException();
                    }

                    return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ParenthesizedExpression(
                                CastFromDynamic(
                                    this.MetaSyntaxFactory.Type( typeof( IDynamicMember ) ), expression ) ),
                            IdentifierName( nameof( IDynamicMember.CreateExpression ) ) ) );

                case "String":
                    return CreateLiteralExpressionFactory( SyntaxKind.StringLiteralExpression );

                case "Int32":
                case "Int16":
                case "Int64":
                case "UInt32":
                case "UInt16":
                case "UInt64":
                case "Byte":
                case "SByte":
                case nameof( Single ):
                case nameof( Double ):
                    return CreateLiteralExpressionFactory( SyntaxKind.NumericLiteralExpression );

                case nameof( Char ):
                    return CreateLiteralExpressionFactory( SyntaxKind.CharacterLiteralExpression );

                case nameof( Boolean ):
                    // new RuntimeExpression(LiteralExpression(BooleanKeyword(expression)), "System.Boolean")
                    return ObjectCreationExpression( this.MetaSyntaxFactory.Type( typeof( RuntimeExpression ) ) )
                        .AddArgumentListArguments(
                            Argument( InvocationExpression( this.MetaSyntaxFactory.SyntaxFactoryMethod( nameof( LiteralExpression ) ) )
                                .AddArgumentListArguments( Argument( InvocationExpression( this.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.BooleanKeyword ) ) )
                                    .AddArgumentListArguments( Argument( expression ) ) ) ) ),
                            Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( DocumentationCommentId.CreateReferenceId( type ) ) ) ) );

                default:
                    // TODO: emit an error. We don't know how to serialize this into syntax.
                    // TODO: pluggable syntax serializers must be called here.
                    return expression;
            }
        }

        public override SyntaxNode VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
        {
            var transformationKind = this.GetTransformationKind( node.Expression );

            // Cast to dynamic expressions.
            if ( transformationKind != TransformationKind.Transform &&
                 this._semanticAnnotationMap.GetType( node.Expression ) is IDynamicTypeSymbol )
            {
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ParenthesizedExpression(
                                CastFromDynamic(
                                    this.MetaSyntaxFactory.Type( typeof( IDynamicMember ) ), (ExpressionSyntax) this.Visit( node.Expression )! ) ),
                            IdentifierName( nameof( DynamicMetaMemberExtensions.CreateMemberAccessExpression ) ) ) )
                    .AddArgumentListArguments( Argument( LiteralExpression(
                        SyntaxKind.StringLiteralExpression, Literal( node.Name.Identifier.ValueText ) ) ) );
            }

            return base.VisitMemberAccessExpression( node );
        }

        public override SyntaxNode VisitInvocationExpression( InvocationExpressionSyntax node )
        {
            bool ArgumentIsDynamic( ArgumentSyntax argument ) =>
                this._semanticAnnotationMap.GetParameterSymbol( argument )?.Type is IDynamicTypeSymbol or IArrayTypeSymbol { ElementType: IDynamicTypeSymbol };

            var transformationKind = this.GetTransformationKind( node );

            if ( transformationKind != TransformationKind.Transform && node.ArgumentList.Arguments.Any( ArgumentIsDynamic ) )
            {
                return node.Update(
                    (ExpressionSyntax) this.Visit( node.Expression )!,
                    ArgumentList( SeparatedList( node.ArgumentList.Arguments.Select(
                        a => ArgumentIsDynamic( a ) ? Argument( this.TransformExpression( a.Expression ) ) : this.Visit( a )! ) )! ) );
            }

            // Expand extension methods.
            if ( transformationKind == TransformationKind.Transform )
            {
                var symbol = this._semanticAnnotationMap.GetSymbol( node.Expression );
                if ( symbol is IMethodSymbol { IsExtensionMethod: true } method )
                {
                    List<ArgumentSyntax> arguments = new( node.ArgumentList.Arguments.Count + 1 )
                    {
                        Argument( ((MemberAccessExpressionSyntax) node.Expression).Expression )
                            .WithTemplateAnnotationsFrom( node )
                    };

                    arguments.AddRange( node.ArgumentList.Arguments );

                    var result = this.Transform( InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this.MetaSyntaxFactory.Type( method.ContainingType ),
                            IdentifierName( method.Name ) ),
                        ArgumentList( SeparatedList( arguments ) ) ) );
                    return result;
                }
            }

            return base.VisitInvocationExpression( node );
        }

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            this.Indent( 3 );

            // Generates a template method.

            // TODO: templates may support build-time parameters, which must to the compiled template method.

            // TODO: also compile templates for properties and so on.

            if ( node.Body == null )
            {
                throw new NotImplementedException( "Expression-bodied templates are not supported." );
            }

            var body = (BlockSyntax) this.VisitBlock( node.Body, TransformationKind.None, true );

            var result = MethodDeclaration(
                    this.MetaSyntaxFactory.Type( typeof( SyntaxNode ) ),
                    Identifier( node.Identifier.Text + TemplateCompiler.TemplateMethodSuffix ) )
                .WithModifiers(
                    TokenList(
                        Token( SyntaxKind.PublicKeyword ) ) )
                .WithBody( body );

            this.Unindent( 3 );

            return result;
        }

        public override SyntaxNode VisitBlock( BlockSyntax node )
        {
            var transformationKind = this.GetTransformationKind( node );
            return this.VisitBlock( node, transformationKind, transformationKind == TransformationKind.Transform );
        }

        /// <summary>
        /// Transforms a block (according to a specified <see cref="MetaSyntaxRewriter.TransformationKind"/>)
        /// and specifies if the block should have its own <c>List&lt;StatementSyntax&gt;</c>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="transformationKind"></param>
        /// <param name="withOwnList"><c>true</c> if the block should declare its own List of statements,
        /// <c>false</c> if it should reuse the one of the parent block.</param>
        private SyntaxNode VisitBlock( BlockSyntax node, TransformationKind transformationKind, bool withOwnList )
        {
            if ( withOwnList )
            {
                using ( this.UseStatementList( $"__s{++this._nextStatementListId}", new List<StatementSyntax>() ) )
                {
                    // List<StatementSyntax> statements = new List<StatementSyntax>();
                    var listType = this.MetaSyntaxFactory.Type( typeof( List<StatementSyntax> ) );
                    this._currentMetaStatementList!.Add( LocalDeclarationStatement(
                            VariableDeclaration( listType )
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                                Identifier( this._currentStatementListVariableName! ) )
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    ObjectCreationExpression( listType, ArgumentList(), default ) ) ) ) ) )
                        .WithLeadingTrivia( this.GetIndentation() ) );

                    var metaStatements = this.ToMetaStatements( node.Statements );
                    this._currentMetaStatementList.AddRange( metaStatements );

                    if ( transformationKind == TransformationKind.Transform )
                    {
                        // return statements.ToArray();
                        this._currentMetaStatementList.Add(
                            ReturnStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName( this._currentStatementListVariableName! ),
                                            IdentifierName( "ToArray" ) ) ) )
                                .WithLeadingTrivia( this.GetIndentation() ) );

                        // Wrap in using(OpenTemplateLexicalScope())
                        var usingStatement = UsingStatement(
                                Block( this._currentMetaStatementList ) )
                            .WithExpression( InvocationExpression( this.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.OpenTemplateLexicalScope ) ) ) );

                        // Block( Func<StatementSyntax[]>( delegate { ... } )
                        return this.DeepIndent( this.MetaSyntaxFactory.Block(
                            InvocationExpression(
                                ObjectCreationExpression(
                                        this.MetaSyntaxFactory.GenericType( typeof( Func<> ), ArrayType(
                                                this.MetaSyntaxFactory.Type( typeof( StatementSyntax ) ) )
                                            .WithRankSpecifiers(
                                                SingletonList(
                                                    ArrayRankSpecifier(
                                                        SingletonSeparatedList<ExpressionSyntax>(
                                                            OmittedArraySizeExpression() ) ) ) ) ) )
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    AnonymousMethodExpression()
                                                        .WithBody( Block( usingStatement )
                                                            .AddNoDeepIndentAnnotation() ) ) ) ) ) ) ) );
                    }
                    else
                    {
                        this._currentMetaStatementList.Add(
                            ReturnStatement(
                                this.MetaSyntaxFactory.Block(
                                    IdentifierName( this._currentStatementListVariableName! ) ).WithLeadingTrivia( this.GetIndentation() ) ) );

                        return Block( this._currentMetaStatementList );
                    }
                }
            }
            else
            {
                if ( transformationKind == TransformationKind.Transform )
                {
                    // withOwnList must be true.
                    throw new AssertionFailedException();
                }

                var metaStatements = this.ToMetaStatements( node.Statements );

                // Add the statements to the parent list.
                this._currentMetaStatementList!.AddRange( metaStatements );

                // Returns an empty block intentionally.
                return Block();
            }
        }

        private IEnumerable<StatementSyntax> ToMetaStatements( in SyntaxList<StatementSyntax> statements )
            => statements.Select( this.ToMetaStatement );

        /// <summary>
        /// Transforms a source statement into a statement that instantiates this statement.
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        private StatementSyntax ToMetaStatement( StatementSyntax statement )
        {
            if ( statement is BlockSyntax block )
            {
                return Block( this.ToMetaStatements( block.Statements ) );
            }

            var transformedStatement = this.Visit( statement );

            if ( transformedStatement is StatementSyntax statementSyntax )
            {
                // The statement is already build-time code so there is nothing to transform.
                return statementSyntax.WithLeadingTrivia( this.GetIndentation() );
            }
            else if ( transformedStatement is ExpressionSyntax expressionSyntax )
            {
                // The statement is run-time code and has been transformed into an expression
                // creating the StatementSyntax.

                var statementComment = NormalizeSpace( statement.ToString() );

                if ( statementComment.Length > 120 )
                {
                    // TODO: handle surrogate pairs correctly
                    statementComment = statementComment.Substring( 0, 117 ) + "...";
                }

                var leadingTrivia = TriviaList( CarriageReturnLineFeed ).AddRange( this.GetIndentation() )
                    .Add( Comment( "// " + statementComment ) ).Add( CarriageReturnLineFeed ).AddRange( this.GetIndentation() );
                var trailingTrivia = TriviaList( CarriageReturnLineFeed, CarriageReturnLineFeed );

                // statements.Add( expression )
                var add = ExpressionStatement(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName( this._currentStatementListVariableName! ),
                                IdentifierName( "Add" ) ) )
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument( expressionSyntax ) ) ) ) );

                return add.WithLeadingTrivia( leadingTrivia ).WithTrailingTrivia( trailingTrivia );
            }
            else
            {
                throw new AssertionFailedException();
            }
        }

        public override SyntaxNode VisitInterpolation( InterpolationSyntax node )
        {
            if ( node.Expression.GetScopeFromAnnotation() != SymbolDeclarationScope.CompileTimeOnly &&
                 this._semanticAnnotationMap.GetType( node.Expression )!.Kind != SymbolKind.DynamicType )
            {
                var token = this.MetaSyntaxFactory.Token(
                    LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ),
                    this.Transform( SyntaxKind.InterpolatedStringTextToken ),
                    node.Expression,
                    node.Expression,
                    LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ) );

                return this.DeepIndent(
                    this.MetaSyntaxFactory.InterpolatedStringText( token ) );
            }
            else
            {
                var transformedInterpolation = base.VisitInterpolation( node );
                return transformedInterpolation;
            }
        }

        public override SyntaxNode VisitIfStatement( IfStatementSyntax node )
        {
            if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Run-time if. Just serialize to syntax.
                return this.TransformIfStatement( node );
            }
            else
            {
                var transformedStatement = this.ToMetaStatement( node.Statement );
                var transformedElseStatement = node.Else != null ? this.ToMetaStatement( node.Else.Statement ) : null;
                return IfStatement(
                    node.AttributeLists,
                    node.Condition,
                    transformedStatement,
                    transformedElseStatement != null ? ElseClause( transformedElseStatement ) : null );
            }
        }

        public override SyntaxNode VisitWhileStatement( WhileStatementSyntax node )
        {
            if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Run-time while. Just serialize to syntax.
                return this.TransformWhileStatement( node );
            }
            else
            {
                var transformedStatement = this.ToMetaStatement( node.Statement );
                return WhileStatement(
                    node.AttributeLists,
                    node.Condition,
                    transformedStatement );
            }
        }

        public override SyntaxNode VisitForEachStatement( ForEachStatementSyntax node )
        {
            if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Run-time foreach. Just serialize to syntax.
                return this.TransformForEachStatement( node );
            }

            this.Indent();

            StatementSyntax statement;
            switch ( node.Statement )
            {
                case BlockSyntax block:
                    var metaStatements = this.ToMetaStatements( block.Statements );
                    statement = Block( metaStatements );
                    break;

                default:
                    statement = this.ToMetaStatement( node.Statement );
                    break;
            }

            this.Unindent();

            return ForEachStatement( node.Type, node.Identifier, node.Expression, statement );
        }

        protected override ExpressionSyntax TransformVariableDeclarator( VariableDeclaratorSyntax node )
        {
            this.Indent();

            var result = this.MetaSyntaxFactory.VariableDeclarator(
                this.TransformTemplateDeclaratorIdentifier( node.Identifier ),
                this.Transform( node.ArgumentList ),
                this.Transform( node.Initializer ) );

            this.Unindent();
            return result;
        }

        private ExpressionSyntax TransformTemplateDeclaratorIdentifier( SyntaxToken token )
        {
            if ( token.Kind() != SyntaxKind.IdentifierToken )
            {
                throw new AssertionFailedException();
            }

            return InvocationExpression(
                this.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.TemplateDeclaratorIdentifier ) ) ).WithArgumentList( ArgumentList( SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[]
            {
                Argument( this.MetaSyntaxFactory.LiteralExpression( token.Text ) )
            } ) ) );
        }

        public override SyntaxNode VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
        {
            var proceedAssignments =
                node.Declaration.Variables
                    .Where( n => n.Initializer != null && this.IsProceed( n.Initializer.Value ) )
                    .ToList();

            if ( proceedAssignments.Count == 0 )
            {
                return base.VisitLocalDeclarationStatement( node );
            }
            else if ( proceedAssignments.Count > 1 )
            {
                throw new AssertionFailedException();
            }
            else
            {
                var returnVariableName = proceedAssignments[0].Identifier.Text;

                var callCreateSyntaxType = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression( CastFromDynamic(
                            this.MetaSyntaxFactory.Type( typeof( IProceedImpl ) ),
                            proceedAssignments[0].Initializer!.Value ) ),
                        IdentifierName( nameof( IProceedImpl.CreateTypeSyntax ) ) ),
                    ArgumentList() );

                var returnVariableIdentifier = this.MetaSyntaxFactory.Identifier( this.MetaSyntaxFactory.LiteralExpression( returnVariableName ) );
                var variableDeclarator = this.MetaSyntaxFactory.VariableDeclarator( returnVariableIdentifier );
                var variableDeclaration = this.MetaSyntaxFactory.VariableDeclaration(
                    callCreateSyntaxType,
                    this.MetaSyntaxFactory.SeparatedList2<VariableDeclaratorSyntax>( new[] { variableDeclarator } ) );
                var localDeclarationStatement = this.MetaSyntaxFactory.LocalDeclarationStatement( variableDeclaration );
                var callProceed = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            CastFromDynamic(
                                this.MetaSyntaxFactory.Type( typeof( IProceedImpl ) ),
                                proceedAssignments[0].Initializer!.Value ) ),
                        IdentifierName( nameof( IProceedImpl.CreateAssignStatement ) ) ),
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { Argument( this.MetaSyntaxFactory.LiteralExpression( returnVariableName ) ) } ) ) );

                var createBlock = this.MetaSyntaxFactory.Block( localDeclarationStatement, callProceed );

                createBlock = this.DeepIndent( createBlock );

                // Annotate the block for removal.
                return InvocationExpression(
                    this.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.WithFlattenBlockAnnotation ) ),
                    ArgumentList( SingletonSeparatedList( Argument( createBlock ) ) ) );
            }
        }

        public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
        {
            if ( node.Expression != null && this.IsProceed( node.Expression ) )
            {
                var expressionType = this._semanticAnnotationMap.GetType( node.Expression );
                if ( expressionType == null )
                {
                    // We need the expression type.
                    throw new AssertionFailedException( "The type of the return expression was not found." );
                }

                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            CastFromDynamic(
                                this.MetaSyntaxFactory.Type( typeof( IProceedImpl ) ), node.Expression ) ),
                        IdentifierName( nameof( IProceedImpl.CreateReturnStatement ) ) ),
                    ArgumentList() );
            }
            else
            {
                return InvocationExpression(
                    this.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.TemplateReturnStatement ) ) ).AddArgumentListArguments(
                    Argument( this.Transform( node.Expression ) ) );
            }
        }

        public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
        {
            if ( node.Identifier.Kind() == SyntaxKind.IdentifierToken && !node.IsVar && this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Fully qualifies simple identifiers.

                var symbol = this._semanticAnnotationMap.GetSymbol( node );

                if ( symbol is INamespaceOrTypeSymbol namespaceOrType )
                {
                    return this.Visit( CSharpSyntaxGenerator.Instance.NameExpression( namespaceOrType ) )!;
                }
                else if ( symbol != null && symbol.IsStatic && node.Parent is not MemberAccessExpressionSyntax )
                {
                    switch ( symbol.Kind )
                    {
                        case SymbolKind.Field:
                        case SymbolKind.Property:
                        case SymbolKind.Event:
                        case SymbolKind.Method:
                            // We have an access to a field or method with a "using static", or a non-qualified static member access.
                            return this.MetaSyntaxFactory.MemberAccessExpression( 
                                this.MetaSyntaxFactory.Kind( SyntaxKind.SimpleMemberAccessExpression ),
                                (ExpressionSyntax) this.Visit( CSharpSyntaxGenerator.Instance.NameExpression( symbol.ContainingType ) )!, 
                                this.MetaSyntaxFactory.IdentifierName2( this.MetaSyntaxFactory.LiteralExpression( node.Identifier.Text ) ) );
                    }
                }
            }

            return base.VisitIdentifierName( node );
        }

        public override SyntaxNode VisitQualifiedName( QualifiedNameSyntax node )
        {
            if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Fully qualifies composed names.
                var symbol = this._semanticAnnotationMap.GetSymbol( node );

                switch ( symbol )
                {
                    case INamespaceOrTypeSymbol namespaceOrType:
                        return this.Visit( CSharpSyntaxGenerator.Instance.NameExpression( namespaceOrType ) )!;
                }
            }

            return base.VisitQualifiedName( node );
        }

        public override SyntaxNode VisitGenericName( GenericNameSyntax node )
        {
            if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Fully qualifies composed names.
                var symbol = this._semanticAnnotationMap.GetSymbol( node );

                switch ( symbol )
                {
                    case INamespaceOrTypeSymbol namespaceOrType:
                        return this.Visit( CSharpSyntaxGenerator.Instance.NameExpression( namespaceOrType ) )!;
                }
            }

            return base.VisitGenericName( node );
        }

        public override bool VisitIntoStructuredTrivia => false;

        private StatementListCookie UseStatementList( string variableName, List<StatementSyntax> metaStatementList )
        {
            var cookie = new StatementListCookie(
                this,
                this._currentStatementListVariableName!,
                this._currentMetaStatementList! );

            this._currentStatementListVariableName = variableName;
            this._currentMetaStatementList = metaStatementList;
            return cookie;
        }
    }
}