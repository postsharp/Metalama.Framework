// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable RedundantUsingDirective

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        private readonly IDiagnosticAdder _diagnosticAdder;
        private readonly TemplateMetaSyntaxFactoryImpl _templateMetaSyntaxFactory;
        private MetaContext? _currentMetaContext;
        private int _nextStatementListId;
        private ISymbol? _rootTemplateSymbol;

        public TemplateCompilerRewriter(
            Compilation compileTimeCompilation,
            SemanticAnnotationMap semanticAnnotationMap,
            IDiagnosticAdder diagnosticAdder ) : base( compileTimeCompilation )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
            this._diagnosticAdder = diagnosticAdder;
            this._templateMetaSyntaxFactory = new TemplateMetaSyntaxFactoryImpl( this.MetaSyntaxFactory );
        }

        public bool Success { get; private set; } = true;

        private void ReportDiagnostic( Diagnostic diagnostic )
        {
            this._diagnosticAdder.ReportDiagnostic( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.Success = false;
            }
        }

        private static ExpressionSyntax CastFromDynamic( TypeSyntax targetType, ExpressionSyntax expression )
            => CastExpression( targetType, CastExpression( PredefinedType( Token( SyntaxKind.ObjectKeyword ) ), expression ) );

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

        public override bool VisitIntoStructuredTrivia => false;

        /// <summary>
        /// Sets the current <see cref="MetaContext"/> for the current execution context. To be used in a <c>using</c> statement.
        /// </summary>
        /// <param name="newMetaContext"></param>
        /// <returns></returns>
        private MetaContextCookie WithMetaContext( MetaContext newMetaContext )
        {
            var cookie = new MetaContextCookie( this, this._currentMetaContext );

            this._currentMetaContext = newMetaContext;

            return cookie;
        }

        /// <summary>
        /// Generates the code to generate a run-time symbol name (i.e. a call to <see cref="TemplateSyntaxFactory.GetUniqueIdentifier"/>),
        /// adds this code to the list of statements of the current <see cref="MetaContext"/>, and returns the identifier of
        /// the compiled template that contains the run-time symbol name.
        /// </summary>
        /// <param name="buildTimeIdentifier">The name of the identifier in the source template, used as a hint to generate a run-time identifier.</param>
        /// <returns>The identifier of the compiled template that contains the run-time symbol name.</returns>
        private IdentifierNameSyntax ReserveRunTimeSymbolName( SyntaxToken buildTimeIdentifier )
        {
            if ( buildTimeIdentifier.IsMissing )
            {
                throw new AssertionFailedException();
            }

            var metaVariableName = "__" + buildTimeIdentifier.Text;

            var callGetUniqueIdentifier = this._templateMetaSyntaxFactory.GetUniqueIdentifier( buildTimeIdentifier.Text );

            var localDeclaration =
                LocalDeclarationStatement(
                        VariableDeclaration( this.MetaSyntaxFactory.Type( typeof(SyntaxToken) ) )
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator( Identifier( metaVariableName ) )
                                        .WithInitializer( EqualsValueClause( callGetUniqueIdentifier ) ) ) ) )
                    .NormalizeWhitespace();

            this._currentMetaContext!.Statements.Add( localDeclaration );

            return IdentifierName( metaVariableName );
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
                           || parent is SwitchSectionSyntax 
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

            return symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof(ProceedAttribute) );
        }

        /// <summary>
        /// Determines if the node is a pragma and returns the kind of pragma, if any.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        private bool TryGetPragma( SyntaxNode node, out PragmaKind kind )
        {
            var symbol = this._semanticAnnotationMap.GetSymbol( node );

            if ( symbol == null || !symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof(PragmaAttribute) ) )
            {
                kind = PragmaKind.None;

                return false;
            }
            else
            {
                switch ( symbol.Name )
                {
                    case nameof(ITemplateContextPragma.Comment):
                        kind = PragmaKind.Comment;

                        return true;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            // Captures the root symbol.
            if ( this._rootTemplateSymbol == null )
            {
                if ( node == null )
                {
                    throw new ArgumentNullException( nameof(node) );
                }

                this._rootTemplateSymbol = this._semanticAnnotationMap.GetDeclaredSymbol( node );

                if ( this._rootTemplateSymbol == null )
                {
                    throw new AssertionFailedException( "Didn't find a symbol for a template method node." );
                }
            }

            return base.Visit( node );
        }

        protected override ExpressionSyntax TransformTupleExpression( TupleExpressionSyntax node )
        {
            // tuple can be initialize from variables and then items take names from variable name
            // but variable name is not safe and could be renamed because of target variables 
            // in this case we initialize tuple with explicit names
            var symbol = (INamedTypeSymbol) this._semanticAnnotationMap.GetExpressionType( node )!;
            var transformedArguments = new ArgumentSyntax[node.Arguments.Count];

            for ( var i = 0; i < symbol.TupleElements.Length; i++ )
            {
                var tupleElement = symbol.TupleElements[i];
                ArgumentSyntax arg;

                if ( !tupleElement.Name.Equals( tupleElement.CorrespondingTupleField!.Name, StringComparison.Ordinal ) )
                {
                    var name = symbol.TupleElements[i].Name;
                    arg = node.Arguments[i].WithNameColon( NameColon( name ) );
                }
                else
                {
                    arg = node.Arguments[i];
                }

                transformedArguments[i] = arg;
            }

            var transformedNode = TupleExpression(
                node.OpenParenToken,
                default(SeparatedSyntaxList<ArgumentSyntax>).AddRange( transformedArguments ),
                node.CloseParenToken );

            return base.TransformTupleExpression( transformedNode );
        }

        protected override ExpressionSyntax Transform( SyntaxToken token )
        {
            // Following renaming of local variables cannot be apply for TupleElement  
            if ( token.Kind() == SyntaxKind.IdentifierToken && token.Parent != null && token.Parent is not TupleElementSyntax )
            {
                // Transforms identifier declarations (local variables and local functions). Local identifiers must have
                // a unique name in the target code, which is unknown when the template is compiled, therefore local identifiers
                // get their name dynamically at expansion time. The ReserveRunTimeSymbolName method generates code that
                // reserves the name at expansion time. The result is stored in a local variable of the expanded template.
                // Then, each template reference uses this local variable.

                var identifierSymbol = this._semanticAnnotationMap.GetDeclaredSymbol( token.Parent! );

                if ( this.IsDeclaredWithinTemplate(identifierSymbol!) )
                {
                    if ( !this._currentMetaContext!.TryGetGeneratedSymbolLocal( identifierSymbol!, out _ ) )
                    {
                        var declaredSymbolNameLocal = this.ReserveRunTimeSymbolName( token ).Identifier;
                        this._currentMetaContext.AddGeneratedSymbolLocal( identifierSymbol!, declaredSymbolNameLocal );

                        return IdentifierName( declaredSymbolNameLocal.Text );
                    }
                    else
                    {
                        throw new AssertionFailedException();
                    }
                }
                else
                {
                    // This is not a symbol declaration but a symbol reference.
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
                    return this.TransformIdentifierToken( node );

                default:
                    throw new AssertionFailedException( $"Unexpected identifier kind: {node.Identifier.Kind()}." );
            }
        }

        private bool IsDeclaredWithinTemplate(ISymbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }
            else
            { 
                // Symbol is Declared in Template if ContainsSymbol is Template method or if ContainsSymbol
                // is child level of Template method f.e. local function etc.
                return SymbolEqualityComparer.Default.Equals( symbol.ContainingSymbol, this._rootTemplateSymbol ) || this.IsDeclaredWithinTemplate(symbol.ContainingSymbol);
            }
        }

        private ExpressionSyntax TransformIdentifierToken( IdentifierNameSyntax node )
        {
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

            var isDeclaredWithinTemplate = this.IsDeclaredWithinTemplate( identifierSymbol! );

            // TODO: Perhaps the following condition is bad because the members or fields of the Aspect class
            // that are called in the Template method should also be renamed.
            if ( isDeclaredWithinTemplate )
            {
                if ( this._currentMetaContext!.TryGetGeneratedSymbolLocal( identifierSymbol!, out var declaredSymbolNameLocal ) )
                {
                    return this.MetaSyntaxFactory.IdentifierName1( IdentifierName( declaredSymbolNameLocal.Text ) );
                }
                else
                {
                    // That should not happen in a correct compilation because IdentifierName is used only for an identifier reference, not an identifier definition.
                    // Identifier definitions should be processed by Transform(SyntaxToken).

                    // However, this can happen in an incorrect/incomplete compilation. In this case, returning anything is fine.
                    this.Success = false;
                }
            }

            return this.MetaSyntaxFactory.IdentifierName2( SyntaxFactoryEx.LiteralExpression( node.Identifier.Text ) );
        }

        protected override ExpressionSyntax TransformArgument( ArgumentSyntax node )
        {
            // The base implementation is very verbose, so we use this one:
            if ( node.RefKindKeyword.Kind() == SyntaxKind.None )
            {
                var transformedArgument = this.MetaSyntaxFactory.Argument( this.Transform( node.Expression ) );

                if ( node.NameColon != null )
                {
                    var transformedNameColon = this.TransformNameColon( node.NameColon );

                    transformedArgument =
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    transformedArgument,
                                    IdentifierName( "WithNameColon" ) ) )
                            .WithArgumentList( ArgumentList( SingletonSeparatedList( Argument( transformedNameColon ) ) ) );
                }

                return transformedArgument.WithTemplateAnnotationsFrom( node );
            }
            else
            {
                return base.TransformArgument( node );
            }
        }

        protected override ExpressionSyntax TransformSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        {
            if (node.ExpressionBody != null)
            {
                foreach(var n in node.ExpressionBody.ChildNodes())
                {
                    continue;
                }
            }

            return base.TransformSimpleLambdaExpression( node );
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
                    return ObjectCreationExpression( this.MetaSyntaxFactory.Type( typeof(RuntimeExpression) ) )
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

            var type = this._semanticAnnotationMap.GetExpressionType( expression )!;

            // A local function that wraps the input `expression` into a LiteralExpression.
            ExpressionSyntax CreateLiteralExpressionFactory( SyntaxKind syntaxKind )
            {
                // new RuntimeExpression(LiteralExpression(syntaxKind, Literal(expression)), type)
                return ObjectCreationExpression( this.MetaSyntaxFactory.Type( typeof(RuntimeExpression) ) )
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
                        this.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.UnsupportedContextForProceed.CreateDiagnostic(
                                this._semanticAnnotationMap.GetLocation( expression ),
                                "" ) );

                        return LiteralExpression( SyntaxKind.NullLiteralExpression );
                    }

                    return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ParenthesizedExpression( CastFromDynamic( this.MetaSyntaxFactory.Type( typeof(IDynamicMember) ), expression ) ),
                            IdentifierName( nameof(IDynamicMember.CreateExpression) ) ) );

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
                case nameof(Single):
                case nameof(Double):
                    return CreateLiteralExpressionFactory( SyntaxKind.NumericLiteralExpression );

                case nameof(Char):
                    return CreateLiteralExpressionFactory( SyntaxKind.CharacterLiteralExpression );

                case nameof(Boolean):
                    // new RuntimeExpression(LiteralExpression(BooleanKeyword(expression)), "System.Boolean")
                    return ObjectCreationExpression( this.MetaSyntaxFactory.Type( typeof(RuntimeExpression) ) )
                        .AddArgumentListArguments(
                            Argument(
                                InvocationExpression( this.MetaSyntaxFactory.SyntaxFactoryMethod( nameof(LiteralExpression) ) )
                                    .AddArgumentListArguments(
                                        Argument(
                                            InvocationExpression(
                                                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember(
                                                        nameof(TemplateSyntaxFactory.BooleanKeyword) ) )
                                                .AddArgumentListArguments( Argument( expression ) ) ) ) ),
                            Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( DocumentationCommentId.CreateReferenceId( type ) ) ) ) );

                default:
                    // TODO: don't throw an exception, but add a diagnostic and continue (return the expression). This requires Radka's PR to be merged.
                    // TODO: pluggable syntax serializers must be called here.
                    throw new InvalidUserCodeException(
                        TemplatingDiagnosticDescriptors.CannotConvertBuildTime.CreateDiagnostic(
                            this._semanticAnnotationMap.GetLocation( expression ),
                            (expression.ToString(), type) ) );
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
                 this._semanticAnnotationMap.GetExpressionType( node.Expression ) is IDynamicTypeSymbol )
            {
                return InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.CreateDynamicMemberAccessExpression) ),
                    ArgumentList(
                        SeparatedList(
                            new[]
                            {
                                Argument(
                                    CastFromDynamic(
                                        this.MetaSyntaxFactory.Type( typeof(IDynamicMember) ),
                                        (ExpressionSyntax) this.Visit( node.Expression )! ) ),
                                Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( node.Name.Identifier.ValueText ) ) )
                            } ) ) );
            }

            return base.VisitMemberAccessExpression( node );
        }

        public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
        {
            // The default implementation has to be overridden because VisitInvocationExpression can
            // return null in case of pragma. In this case, the ExpressionStatement must return null too.
            // In the default implementation, such case would result in an exception.

            switch ( this.GetTransformationKind( node ) )
            {
                case TransformationKind.Transform:
                    return this.TransformExpressionStatement( node );

                default:
                    var transformedExpression = this.Visit( node.Expression );

                    if ( transformedExpression == null )
                    {
                        return null;
                    }

                    return node.Update(
                        this.VisitList( node.AttributeLists ),
                        (ExpressionSyntax) transformedExpression!,
                        this.VisitToken( node.SemicolonToken ) );
            }
        }

        public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
        {
            bool ArgumentIsDynamic( ArgumentSyntax argument )
                => this._semanticAnnotationMap.GetParameterSymbol( argument )?.Type is IDynamicTypeSymbol or IArrayTypeSymbol
                    { ElementType: IDynamicTypeSymbol };

            var transformationKind = this.GetTransformationKind( node );

            if ( transformationKind != TransformationKind.Transform && node.ArgumentList.Arguments.Any( ArgumentIsDynamic ) )
            {
                return node.Update(
                    (ExpressionSyntax) this.Visit( node.Expression )!,
                    ArgumentList(
                        SeparatedList(
                            node.ArgumentList.Arguments.Select(
                                a => ArgumentIsDynamic( a ) ? Argument( this.TransformExpression( a.Expression ) ) : this.Visit( a )! ) )! ) );
            }

            if ( this.IsProceed( node.Expression ) )
            {
                this.ReportDiagnostic( TemplatingDiagnosticDescriptors.UnsupportedContextForProceed.CreateDiagnostic( node.Expression.GetLocation(), "" ) );

                return LiteralExpression( SyntaxKind.NullLiteralExpression );
            }
            else if ( this.TryGetPragma( node.Expression, out var pragmaKind ) )
            {
                switch ( pragmaKind )
                {
                    case PragmaKind.Comment:
                        var arguments = node.ArgumentList.Arguments.Insert(
                            0,
                            Argument( IdentifierName( this._currentMetaContext!.StatementListVariableName ) ) );

                        // TemplateSyntaxFactory.AddComments( __s, comments );
                        var add =
                            this.DeepIndent(
                                ExpressionStatement(
                                    InvocationExpression(
                                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.AddComments) ),
                                        ArgumentList( arguments ) ) ) );

                        this._currentMetaContext.Statements.Add( add );

                        return null;

                    default:
                        throw new AssertionFailedException();
                }
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

                    var result = this.Transform(
                        InvocationExpression(
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
            if ( node.Body == null )
            {
                // Not supported or incomplete syntax.
                return node;
            }

            this.Indent( 3 );

            // Generates a template method.

            // TODO: templates may support build-time parameters, which must to the compiled template method.

            // TODO: also compile templates for properties and so on.

            var body = (BlockSyntax) this.BuildRunTimeBlock( node.Body, false );

            var result = MethodDeclaration(
                    this.MetaSyntaxFactory.Type( typeof(SyntaxNode) ),
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
        private SyntaxNode BuildRunTimeBlock( BlockSyntax node, bool generateExpression )
        {
            using ( this.WithMetaContext( MetaContext.CreateForRunTimeBlock( this._currentMetaContext, $"__s{++this._nextStatementListId}" ) ) )
            {
                // List<StatementOrTrivia> statements = new List<StatementOrTrivia>();
                var listType = this.MetaSyntaxFactory.Type( typeof(List<StatementOrTrivia>) );

                this._currentMetaContext!.Statements.Add(
                    LocalDeclarationStatement(
                            VariableDeclaration( listType )
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator( Identifier( this._currentMetaContext.StatementListVariableName ) )
                                            .WithInitializer( EqualsValueClause( ObjectCreationExpression( listType, ArgumentList(), default ) ) ) ) ) )
                        .NormalizeWhitespace()
                        .WithLeadingTrivia( this.GetIndentation() ) );

                // It is important to call ToList to ensure proper ordering of nodes.
                var metaStatements = this.ToMetaStatements( node.Statements ).ToList();
                this._currentMetaContext.Statements.AddRange( metaStatements );

                // TemplateSyntaxFactory.ToStatementArray( __s1 )
                var toArrayStatementExpression = InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.ToStatementArray) ),
                    ArgumentList( SingletonSeparatedList( Argument( IdentifierName( this._currentMetaContext.StatementListVariableName ) ) ) ) );

                if ( generateExpression )
                {
                    // return TemplateSyntaxFactory.ToStatementArray( __s1 );

                    var returnStatementSyntax = ReturnStatement( toArrayStatementExpression ).WithLeadingTrivia( this.GetIndentation() );
                    this._currentMetaContext.Statements.Add( returnStatementSyntax );

                    // Block( Func<StatementSyntax[]>( delegate { ... } )
                    return this.DeepIndent(
                        this.MetaSyntaxFactory.Block(
                            InvocationExpression(
                                ObjectCreationExpression(
                                        this.MetaSyntaxFactory.GenericType(
                                            typeof(Func<>),
                                            ArrayType( this.MetaSyntaxFactory.Type( typeof(StatementSyntax) ) )
                                                .WithRankSpecifiers(
                                                    SingletonList(
                                                        ArrayRankSpecifier( SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) ) ) )
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    AnonymousMethodExpression()
                                                        .WithBody(
                                                            Block( this._currentMetaContext.Statements )
                                                                .AddNoDeepIndentAnnotation() ) ) ) ) ) ) ) );
                }
                else
                {
                    // return __s;
                    this._currentMetaContext.Statements.Add(
                        ReturnStatement( this.MetaSyntaxFactory.Block( toArrayStatementExpression ).WithLeadingTrivia( this.GetIndentation() ) ) );

                    return Block( this._currentMetaContext.Statements );
                }
            }
        }

        /// <summary>
        /// Transforms a list of <see cref="StatementSyntax"/> of the source template into a list of <see cref="StatementSyntax"/> for the compiled
        /// template.
        /// </summary>
        /// <param name="statements"></param>
        /// <returns></returns>
        private IEnumerable<StatementSyntax> ToMetaStatements( in SyntaxList<StatementSyntax> statements ) => statements.SelectMany( this.ToMetaStatements );

        /// <summary>
        /// Transforms a <see cref="StatementSyntax"/> of the source template into a single <see cref="StatementSyntax"/> for the compiled template.
        /// This method is guaranteed to return a single <see cref="StatementSyntax"/>. If the source statement results in several compiled statements,
        /// they will be wrapped into a block.
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        private StatementSyntax ToMetaStatement( StatementSyntax statement )
        {
            var statements = this.ToMetaStatements( statement );

            return statements.Count == 1 ? statements[0] : Block( statements );
        }

        /// <summary>
        /// Transforms a <see cref="StatementSyntax"/> of the source template into a list of <see cref="StatementSyntax"/> for the compiled template.
        /// </summary>
        /// <param name="statement">A statement of the source template.</param>
        /// <returns>A list of statements for the compiled template.</returns>
        private List<StatementSyntax> ToMetaStatements( StatementSyntax statement )
        {
            MetaContext newContext;

            if ( statement is BlockSyntax block )
            {
                // Push the build-time template block.
                newContext = MetaContext.CreateForBuildTimeBlock( this._currentMetaContext! );

                using ( this.WithMetaContext( newContext ) )
                {
                    // Process all statements in this block.
                    foreach ( var childStatement in block.Statements )
                    {
                        ProcessStatement( childStatement );
                    }
                }
            }
            else
            {
                // Push a new MetaContext so statements got added to a new list of statements, but
                // this MetaContext is neither a run-time nor a compile-time lexical scope. 
                newContext = MetaContext.CreateHelperContext( this._currentMetaContext! );

                using ( this.WithMetaContext( newContext ) )
                {
                    ProcessStatement( statement );
                }
            }

            // Returns the statements collected during this call.
            return newContext.Statements;

            void ProcessStatement( StatementSyntax singleStatement )
            {
                var transformedNode = this.Visit( singleStatement );

                switch ( transformedNode )
                {
                    case null:
                        break;

                    case StatementSyntax statementSyntax:
                        // The statement is already build-time code so there is nothing to transform.

                        newContext.Statements.Add( statementSyntax.WithLeadingTrivia( this.GetIndentation() ) );

                        break;

                    case ExpressionSyntax expressionSyntax:
                        {
                            // The statement is run-time code and has been transformed into an expression creating the StatementSyntax.
                            // We need to generate the code adding this code to the list of statements, i.e. `statements.Add( expression )`.

                            // Generate a comment with the template source code.
                            var statementComment = NormalizeSpace( singleStatement.ToString() );

                            if ( statementComment.Length > 120 )
                            {
                                // TODO: handle surrogate pairs correctly
                                statementComment = statementComment.Substring( 0, 117 ) + "...";
                            }

                            var leadingTrivia = TriviaList( CarriageReturnLineFeed )
                                .AddRange( this.GetIndentation() )
                                .Add( Comment( "// " + statementComment ) )
                                .Add( CarriageReturnLineFeed )
                                .AddRange( this.GetIndentation() );

                            var trailingTrivia = TriviaList( CarriageReturnLineFeed, CarriageReturnLineFeed );

                            // TemplateSyntaxFactory.Add( __s, expression )
                            var add =
                                this.DeepIndent(
                                    ExpressionStatement(
                                        InvocationExpression(
                                            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.AddStatement) ),
                                            ArgumentList(
                                                SeparatedList(
                                                    new[]
                                                    {
                                                        Argument( IdentifierName( this._currentMetaContext!.StatementListVariableName ) ),
                                                        Argument( expressionSyntax )
                                                    } ) ) ) ) );

                            newContext.Statements.Add( add.WithLeadingTrivia( leadingTrivia ).WithTrailingTrivia( trailingTrivia ) );

                            break;
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        public override SyntaxNode VisitInterpolation( InterpolationSyntax node )
        {
            if ( node.Expression.GetScopeFromAnnotation() != SymbolDeclarationScope.CompileTimeOnly &&
                 this._semanticAnnotationMap.GetExpressionType( node.Expression )!.Kind != SymbolKind.DynamicType )
            {
                var token = this.MetaSyntaxFactory.Token(
                    LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ),
                    this.Transform( SyntaxKind.InterpolatedStringTextToken ),
                    node.Expression,
                    node.Expression,
                    LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ) );

                return this.DeepIndent( this.MetaSyntaxFactory.InterpolatedStringText( token ) );
            }
            else
            {
                var transformedInterpolation = base.VisitInterpolation( node );

                return transformedInterpolation;
            }
        }

        public override SyntaxNode VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
        {
            return base.VisitSimpleLambdaExpression( node );
        }

        public override SyntaxNode VisitSwitchStatement( SwitchStatementSyntax node )
        {
            if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
            {
                // Run-time. Just serialize to syntax.
                return this.TransformSwitchStatement( node );
            }
            else
            {
                var transformedSections = new SwitchSectionSyntax[node.Sections.Count];

                for ( var i = 0; i < node.Sections.Count; i++ )
                {
                    var section = node.Sections[i];
                    var transformedStatements = this.ToMetaStatements( section.Statements );
                    transformedSections[i] = SwitchSection( section.Labels, List( transformedStatements ) );
                }

                return SwitchStatement(
                    node.SwitchKeyword,
                    node.OpenParenToken,
                    node.Expression,
                    node.CloseParenToken,
                    node.OpenBraceToken,
                    List( transformedSections ),
                    node.CloseBraceToken );
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

            var statement = this.ToMetaStatement( node.Statement );

            this.Unindent();

            return ForEachStatement( node.Type, node.Identifier, node.Expression, statement );
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
                        ParenthesizedExpression(
                            CastFromDynamic(
                                this.MetaSyntaxFactory.Type( typeof(IProceedImpl) ),
                                proceedAssignments[0].Initializer!.Value ) ),
                        IdentifierName( nameof(IProceedImpl.CreateTypeSyntax) ) ),
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
                                this.MetaSyntaxFactory.Type( typeof(IProceedImpl) ),
                                proceedAssignments[0].Initializer!.Value ) ),
                        IdentifierName( nameof(IProceedImpl.CreateAssignStatement) ) ),
                    ArgumentList( SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { Argument( returnVariableIdentifier ) } ) ) );

                var createBlock = this.MetaSyntaxFactory.Block( localDeclarationStatement, callProceed );

                createBlock = this.DeepIndent( createBlock );

                // Annotate the block for removal.
                return InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.WithFlattenBlockAnnotation) ),
                    ArgumentList( SingletonSeparatedList( Argument( createBlock ) ) ) );
            }
        }

        public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
        {
            if ( node.Expression != null && this.IsProceed( node.Expression ) )
            {
                var expressionType = this._semanticAnnotationMap.GetExpressionType( node.Expression );

                if ( expressionType == null )
                {
                    // We need the expression type.
                    throw new AssertionFailedException( "The type of the return expression was not found." );
                }

                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression( CastFromDynamic( this.MetaSyntaxFactory.Type( typeof(IProceedImpl) ), node.Expression ) ),
                        IdentifierName( nameof(IProceedImpl.CreateReturnStatement) ) ),
                    ArgumentList() );
            }
            else
            {
                return InvocationExpression(
                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.TemplateReturnStatement) ) )
                    .AddArgumentListArguments( Argument( this.Transform( node.Expression ) ) );
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
                                this.MetaSyntaxFactory.IdentifierName2( SyntaxFactoryEx.LiteralExpression( node.Identifier.Text ) ) );
                    }
                }
            }

            return base.VisitIdentifierName( node );
        }

        /// <summary>
        /// Transforms a type or namespace so that it is fully qualified, but return <c>false</c> if the input <paramref name="node"/>
        /// is not a type or namespace.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="transformedNode"></param>
        /// <returns></returns>
        private bool TryVisitNamespaceOrTypeName( SyntaxNode node, [NotNullWhen( true )] out SyntaxNode? transformedNode )
        {
            var symbol = this._semanticAnnotationMap.GetSymbol( node );

            switch ( symbol )
            {
                case INamespaceOrTypeSymbol namespaceOrType:
                    var nameExpression = CSharpSyntaxGenerator.Instance.NameExpression( namespaceOrType );

                    transformedNode = this.GetTransformationKind( node ) == TransformationKind.Transform
                        ? this.Transform( nameExpression )
                        : nameExpression;

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
    }
}