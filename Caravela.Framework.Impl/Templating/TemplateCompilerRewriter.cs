// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private readonly TemplateMetaSyntaxFactoryImpl _templateMetaSyntaxFactory;
        private MetaContext? _currentMetaContext;
        private int _nextStatementListId;
        private ISymbol? _rootTemplateSymbol;

        public TemplateCompilerRewriter( Compilation compileTimeCompilation, SemanticAnnotationMap semanticAnnotationMap ) : base( compileTimeCompilation )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
            this._templateMetaSyntaxFactory = new TemplateMetaSyntaxFactoryImpl( this.MetaSyntaxFactory );
        }

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

        protected override ExpressionSyntax Transform( SyntaxToken token )
        {
            if ( token.Kind() == SyntaxKind.IdentifierToken )
            {
                var identifierSymbol = this._semanticAnnotationMap.GetDeclaredSymbol( token.Parent! );
                var isDeclaredWithinTemplate = identifierSymbol != null && SymbolEqualityComparer.Default.Equals( identifierSymbol.ContainingSymbol, this._rootTemplateSymbol );

                if ( isDeclaredWithinTemplate )
                {
                    if ( !this._currentMetaContext!.TryGetGeneratedSymbolLocal( identifierSymbol!, out _ ) )
                    {
                        var declaredSymbolNameLocal = this.ReserveRunTimeSymbolName( token ).Identifier;
                        this._currentMetaContext.AddGeneratedSymbolLocal( identifierSymbol!, declaredSymbolNameLocal );
                        return IdentifierName( declaredSymbolNameLocal.Text );
                    }
                    else
                    {
                        // That should not happen because all identifier references are wrapped in an IdentifierName, which we process separately.
                        // Therefore, we get to this method only for identifier declarations.
                        throw new AssertionFailedException();
                    }

                }
            }

            return base.Transform( token );
        }

        protected override ExpressionSyntax TransformIdentifierName( IdentifierNameSyntax node )
        {
            switch ( node.Identifier.Kind() )
            {
                case SyntaxKind.GlobalKeyword:
                case SyntaxKind.VarKeyword:
                    return base.TransformIdentifierName( node );

                case SyntaxKind.IdentifierToken:
                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected identifier kind: {node.Identifier.Kind()}." );
            }

            if ( node.Identifier.Text == "dynamic" )
            {
                // We change all dynamic into var in the template.
                return base.TransformIdentifierName( IdentifierName( Identifier( "var" ) ) );
            }

            // If the identifier is declared withing the template, the expanded name is given by the TemplateExpansionContext and
            // stored in a template variable named __foo, where foo is the variable name in the template. This variable is defined
            // and initialized in the VisitVariableDeclarator.
            // For identifiers declared outside of the template we just call the regular Roslyn SyntaxFactory.IdentifierName().
            var identifierSymbol = this._semanticAnnotationMap.GetSymbol( node );
            var isDeclaredWithinTemplate = identifierSymbol != null && SymbolEqualityComparer.Default.Equals( identifierSymbol.ContainingSymbol, this._rootTemplateSymbol );

            if ( isDeclaredWithinTemplate )
            {
                if ( !this._currentMetaContext!.TryGetGeneratedSymbolLocal( identifierSymbol!, out var declaredSymbolNameLocal ) )
                {
                    // That should not happen because IdentifierName is used only for an identifier reference, not an identifier defitinition.
                    // Identifier definitions should be processed by Transform(SyntaxToken).
                    throw new AssertionFailedException();
                }

                return this.MetaSyntaxFactory.IdentifierName1( IdentifierName( declaredSymbolNameLocal.Text ) );
            }
            else
            {
                return this.MetaSyntaxFactory.IdentifierName2( this.MetaSyntaxFactory.LiteralExpression( node.Identifier.Text ) );
            }
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
                                .AddArgumentListArguments( Argument( InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.BooleanKeyword ) ) )
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
            if ( this.TryVisitNamespaceOrTypeName( node, out var transformedNode ) )
            {
                return transformedNode;
            }
            
            var transformationKind = this.GetTransformationKind( node.Expression );

            // Cast to dynamic expressions.
            if ( transformationKind != TransformationKind.Transform &&
                 this._semanticAnnotationMap.GetType( node.Expression ) is IDynamicTypeSymbol )
            {
                return InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.CreateDynamicMemberAccessExpression) ),
                    ArgumentList( SeparatedList( new[]
                    {
                        Argument( CastFromDynamic(
                            this.MetaSyntaxFactory.Type( typeof(IDynamicMember) ), (ExpressionSyntax) this.Visit( node.Expression )! ) ),
                        Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( node.Name.Identifier.ValueText ) ) )
                    } ) ) );
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

            var body = (BlockSyntax) this.BuildRunTimeBlock( node.Body, false );

            var result = MethodDeclaration(
                    this.MetaSyntaxFactory.Type( typeof( SyntaxNode ) ),
                    Identifier( node.Identifier.Text + TemplateCompiler.TemplateMethodSuffix ) )
                .WithModifiers( TokenList( Token( SyntaxKind.PublicKeyword ) ) )
                .NormalizeWhitespace()
                .WithBody( body )
                .WithLeadingTrivia( node.GetLeadingTrivia() )
                .WithTrailingTrivia( LineFeed, LineFeed );

            this.Unindent( 3 );

            return result;
        }

        public override SyntaxNode VisitBlock( BlockSyntax node )
        {
            var transformationKind = this.GetTransformationKind( node );
            if ( transformationKind == TransformationKind.Transform )
            {
                return this.BuildRunTimeBlock( node, true );
            }
            else
            {
                using ( this.WithMetaContext( MetaContext.CreateForBuildTimeBlock( this._currentMetaContext! ) ) )
                {
                    var metaStatements = this.ToMetaStatements( node.Statements );

                    // Add the statements to the parent list.
                    this._currentMetaContext!.Statements.AddRange( metaStatements );

                    // Returns an empty block intentionally.
                    return Block();
                }
            }
        }

        /// <summary>
        /// Generates a run-time block.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="generateExpression"><c>true</c> if the returned <see cref="SyntaxNode"/> must be an
        /// expression (in this case, a delegate invocation is returned), or <c>false</c> if it can be a statement
        /// (in this case, a return statement is returned).</param>
        /// <returns></returns>
        private SyntaxNode BuildRunTimeBlock(BlockSyntax node, bool generateExpression)
        {
            using (this.WithMetaContext(
                MetaContext.CreateForRunTimeBlock(this._currentMetaContext, $"__s{++this._nextStatementListId}")))
            {
                // List<StatementSyntax> statements = new List<StatementSyntax>();
                var listType = this.MetaSyntaxFactory.Type(typeof(List<StatementSyntax>));
                this._currentMetaContext!.Statements.Add( LocalDeclarationStatement(
                        VariableDeclaration(listType)
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                            Identifier(this._currentMetaContext.StatementListVariableName))
                                        .WithInitializer(
                                            EqualsValueClause(
                                                ObjectCreationExpression(listType, ArgumentList(), default))))))
                    .NormalizeWhitespace()
                    .WithLeadingTrivia(this.GetIndentation()));

                // It is important to call ToList to ensure proper ordering of nodes.
                var metaStatements = this.ToMetaStatements(node.Statements).ToList();
                this._currentMetaContext.Statements.AddRange(metaStatements);

                if (generateExpression)
                {
                    // return statements.ToArray();
                    this._currentMetaContext.Statements.Add(
                        ReturnStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(this._currentMetaContext.StatementListVariableName),
                                        IdentifierName("ToArray"))))
                            .WithLeadingTrivia(this.GetIndentation()));

                    // Block( Func<StatementSyntax[]>( delegate { ... } )
                    return this.DeepIndent(this.MetaSyntaxFactory.Block(
                        InvocationExpression(
                            ObjectCreationExpression(
                                    this.MetaSyntaxFactory.GenericType(typeof(Func<>), ArrayType(
                                            this.MetaSyntaxFactory.Type(typeof(StatementSyntax)))
                                        .WithRankSpecifiers(
                                            SingletonList(
                                                ArrayRankSpecifier(
                                                    SingletonSeparatedList<ExpressionSyntax>(
                                                        OmittedArraySizeExpression()))))))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                AnonymousMethodExpression()
                                                    .WithBody(Block(this._currentMetaContext.Statements)
                                                        .AddNoDeepIndentAnnotation()))))))));
                }
                else
                {
                    // return __s;
                    this._currentMetaContext.Statements.Add(
                        ReturnStatement(
                            this.MetaSyntaxFactory.Block(
                                IdentifierName(this._currentMetaContext.StatementListVariableName)).WithLeadingTrivia(this.GetIndentation())));

                    return Block(this._currentMetaContext.Statements);
                }
            }
        }

        private IEnumerable<StatementSyntax> ToMetaStatements( in SyntaxList<StatementSyntax> statements )
            => statements.SelectMany( i => this.ToMetaStatements(i) );

        private StatementSyntax ToMetaStatement( StatementSyntax statement )
        {
            var statements = this.ToMetaStatements( statement );

            return statements.Count == 1 ? statements[0] : Block( statements );
        }

        private List<StatementSyntax> ToMetaStatements( StatementSyntax statement )
        {
            MetaContext newContext = MetaContext.CreateHelperContext( this._currentMetaContext! );

            void ProcessNode( SyntaxNode node )
            {
                var visitedNode = this.Visit( node );

                if ( visitedNode is StatementSyntax statementSyntax )
                {
                    // The statement is already build-time code so there is nothing to transform.
                    newContext.Statements.Add( statementSyntax.WithLeadingTrivia( this.GetIndentation() ) );
                }
                else if ( visitedNode is ExpressionSyntax expressionSyntax )
                {
                    // The statement is run-time code and has been transformed into an expression
                    // creating the StatementSyntax.

                    var statementComment = NormalizeSpace( node.ToString() );

                    if ( statementComment.Length > 120 )
                    {
                        // TODO: handle surrogate pairs correctly
                        statementComment = statementComment.Substring( 0, 117 ) + "...";
                    }

                    var leadingTrivia = TriviaList( CarriageReturnLineFeed ).AddRange( this.GetIndentation() )
                        .Add( Comment( "// " + statementComment ) ).Add( CarriageReturnLineFeed ).AddRange( this.GetIndentation() );
                    var trailingTrivia = TriviaList( CarriageReturnLineFeed, CarriageReturnLineFeed );

                    // statements.Add( expression )
                    var add =
                        this.DeepIndent(
                            ExpressionStatement(
                                InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName( this._currentMetaContext!.StatementListVariableName ),
                                            IdentifierName( "Add" ) ) )
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument( expressionSyntax ) ) ) ) ) );

                    newContext.Statements.Add( add.WithLeadingTrivia( leadingTrivia ).WithTrailingTrivia( trailingTrivia ) );
                }
                else
                {
                    throw new AssertionFailedException();
                }
            }

            using ( this.WithMetaContext( newContext ) )
            {

                if ( statement is BlockSyntax block )
                {
                    foreach ( var childStatement in block.Statements )
                    {
                        ProcessNode( childStatement );
                    }
                }
                else
                {
                    ProcessNode( statement );
                }
        }

        return newContext.Statements;
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

            StatementSyntax statement =this.ToMetaStatement( node.Statement );

            this.Unindent();

            return ForEachStatement( node.Type, node.Identifier, node.Expression, statement );
        }

        private IdentifierNameSyntax ReserveRunTimeSymbolName(SyntaxToken buildTimeIdentifier)
        {
            if ( buildTimeIdentifier.IsMissing )
            {
                throw new AssertionFailedException();
            }
            
            var metaVariableName = "__" + buildTimeIdentifier.Text;

            var callGetUniqueIdentifier = this._templateMetaSyntaxFactory.GetUniqueIdentifier(buildTimeIdentifier.Text);

            var localDeclaration =
                LocalDeclarationStatement(
                        VariableDeclaration(
                                this.MetaSyntaxFactory.Type(typeof(SyntaxToken)))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier(metaVariableName))
                                        .WithInitializer(EqualsValueClause(callGetUniqueIdentifier)))))
                    .NormalizeWhitespace();

            this._currentMetaContext!.Statements.Add(localDeclaration);

            return IdentifierName(metaVariableName);
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
                // Special processing of the `var result = proceed()` statement.
                
                // Transform the variable identifier.
                var returnVariableIdentifier = this.Transform( proceedAssignments[0].Identifier )!;
                
                var callCreateSyntaxType = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression( CastFromDynamic(
                            this.MetaSyntaxFactory.Type( typeof( IProceedImpl ) ),
                            proceedAssignments[0].Initializer!.Value ) ),
                        IdentifierName( nameof( IProceedImpl.CreateTypeSyntax ) ) ),
                    ArgumentList() );

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
                        SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { Argument( returnVariableIdentifier ) } ) ) );

                var createBlock = this.MetaSyntaxFactory.Block( localDeclarationStatement, callProceed );

                createBlock = this.DeepIndent( createBlock );

                // Annotate the block for removal.
                return InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.WithFlattenBlockAnnotation ) ),
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
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof( TemplateSyntaxFactory.TemplateReturnStatement ) ) ).AddArgumentListArguments(
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
                else if ( symbol != null && symbol.IsStatic && node.Parent is not MemberAccessExpressionSyntax && node.Parent is not AliasQualifiedNameSyntax )
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

        private bool TryVisitNamespaceOrTypeName( SyntaxNode node, [NotNullWhen(true)] out SyntaxNode? transformedNode )
        {
            // Fully qualifies composed names.
            var symbol = this._semanticAnnotationMap.GetSymbol( node );

            switch ( symbol )
            {
                case INamespaceOrTypeSymbol namespaceOrType:
                    var nameExpression = CSharpSyntaxGenerator.Instance.NameExpression( namespaceOrType );
                    if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
                    {
                        transformedNode = this.Transform( nameExpression );
                    }
                    else
                    {
                        transformedNode = nameExpression;
                    }

                    return true;
                
                default:
                    transformedNode = null;
                    return false;
            }
        }

        public override SyntaxNode VisitQualifiedName( QualifiedNameSyntax node )
        {
            if ( this.TryVisitNamespaceOrTypeName( node, out var transformedNode ) )
            {
                return transformedNode;
            }
            else
            {
                return base.VisitQualifiedName( node );
            }
        }

        public override SyntaxNode VisitAliasQualifiedName( AliasQualifiedNameSyntax node )
        {
            if ( this.TryVisitNamespaceOrTypeName( node, out var transformedNode ) )
            {
                return transformedNode;
            }
            else
            {
                return base.VisitAliasQualifiedName( node );
            }
        }

        public override SyntaxNode VisitGenericName( GenericNameSyntax node )
        {
            if ( this.TryVisitNamespaceOrTypeName( node, out var transformedNode ) )
            {
                return transformedNode;
            }
            else
            {
                return base.VisitGenericName( node );
            }
        }

        public override bool VisitIntoStructuredTrivia => false;

        private MetaContextCookie WithMetaContext( MetaContext newMetaContext )
        {
            var cookie = new MetaContextCookie(
                this,
                this._currentMetaContext );

            this._currentMetaContext = newMetaContext;
            return cookie;
        }
    }
}