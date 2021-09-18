// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable RedundantUsingDirective

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Compiles the source code of a template, annotated with <see cref="TemplateAnnotator"/>,
    /// to an executable template.
    /// </summary>
    internal sealed partial class TemplateCompilerRewriter : MetaSyntaxRewriter, IDiagnosticAdder
    {
        private readonly string _templateName;
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
        private readonly IDiagnosticAdder _diagnosticAdder;
        private readonly CancellationToken _cancellationToken;
        private readonly SerializableTypes _serializableTypes;
        private readonly TemplateMetaSyntaxFactoryImpl _templateMetaSyntaxFactory;
        private readonly TemplateMemberClassifier _templateMemberClassifier;
        private readonly BuildTimeOnlyRewriter _buildTimeOnlyRewriter;
        private MetaContext? _currentMetaContext;
        private int _nextStatementListId;
        private ISymbol? _rootTemplateSymbol;

        public TemplateCompilerRewriter(
            string templateName,
            Compilation runTimeCompilation,
            Compilation compileTimeCompilation,
            SyntaxTreeAnnotationMap syntaxTreeAnnotationMap,
            IDiagnosticAdder diagnosticAdder,
            IServiceProvider serviceProvider,
            SerializableTypes serializableTypes,
            CancellationToken cancellationToken ) : base( compileTimeCompilation )
        {
            this._templateName = templateName;
            this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
            this._diagnosticAdder = diagnosticAdder;
            this._cancellationToken = cancellationToken;
            this._serializableTypes = serializableTypes;
            this._templateMetaSyntaxFactory = new TemplateMetaSyntaxFactoryImpl( this.MetaSyntaxFactory );
            this._templateMemberClassifier = new TemplateMemberClassifier( runTimeCompilation, syntaxTreeAnnotationMap, serviceProvider );
            this._buildTimeOnlyRewriter = new BuildTimeOnlyRewriter( this );
        }

        public bool Success { get; private set; } = true;

        public void Report( Diagnostic diagnostic )
        {
            this._diagnosticAdder.Report( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.Success = false;
            }
        }

        private static string NormalizeSpace( string statementComment )
        {
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
        /// <param name="symbol">The symbol in the source template.</param>
        /// <returns>The identifier of the compiled template that contains the run-time symbol name.</returns>
        private IdentifierNameSyntax ReserveRunTimeSymbolName( ISymbol symbol )
        {
            var metaVariableIdentifier = this._currentMetaContext!.GetTemplateVariableName( symbol );

            var callGetUniqueIdentifier = this._templateMetaSyntaxFactory.GetUniqueIdentifier( symbol.Name );

            var localDeclaration =
                LocalDeclarationStatement(
                        VariableDeclaration( this.MetaSyntaxFactory.Type( typeof(SyntaxToken) ) )
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator( metaVariableIdentifier )
                                        .WithInitializer( EqualsValueClause( callGetUniqueIdentifier ) ) ) ) )
                    .NormalizeWhitespace();

            this._currentMetaContext!.Statements.Add( localDeclaration );

            return IdentifierName( metaVariableIdentifier );
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
            var targetScope = node.GetTargetScopeFromAnnotation();

            switch ( targetScope )
            {
                case TemplatingScope.RunTimeOnly:
                    return TransformationKind.Transform;

                case TemplatingScope.CompileTimeOnly:
                    return TransformationKind.None;
            }

            var scope = node.GetScopeFromAnnotation().GetValueOrDefault();

            // Take a decision from the node if we can.
            if ( scope != TemplatingScope.Both && scope != TemplatingScope.Unknown )
            {
                return scope.MustBeTransformed() ? TransformationKind.Transform : TransformationKind.None;
            }

            // Look for annotation on the parent, but stop at 'if' and 'foreach' statements,
            // which have special interpretation.
            var parent = node.Parent;

            if ( parent == null )
            {
                // This situation seems to happen only when Transform is called from a newly created syntax node,
                // which has not been added to the syntax tree yet. Transform then calls Visit and, which then calls GetTransformationKind
                // so we need to return Transform here. This is not nice and would need to be refactored.

                return TransformationKind.Transform;
            }

            if ( parent is IfStatementSyntax ||
                 parent is ForEachStatementSyntax ||
                 parent is ElseClauseSyntax ||
                 parent is WhileStatementSyntax ||
                 parent is SwitchSectionSyntax )
            {
                throw new AssertionFailedException( $"The node '{node}' must be annotated." );
            }

            return this.GetTransformationKind( parent );
        }

        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            if ( node == null )
            {
                return null;
            }

            this._cancellationToken.ThrowIfCancellationRequested();

            // Captures the root symbol.
            if ( this._rootTemplateSymbol == null )
            {
                if ( node == null )
                {
                    throw new ArgumentNullException( nameof(node) );
                }

                this._rootTemplateSymbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

                if ( this._rootTemplateSymbol == null )
                {
                    throw new AssertionFailedException( "Didn't find a symbol for a template method node." );
                }
            }

            if ( node.GetTargetScopeFromAnnotation() == TemplatingScope.RunTimeOnly &&
                 node.GetScopeFromAnnotation().GetValueOrDefault().GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
            {
                // The node itself does not need to be transformed because it is compile time, but it needs to be converted
                // into a run-time value. However, calls to variants of Proceed must be transformed into calls to the standard Proceed.
                return this.CreateRunTimeExpression( (ExpressionSyntax) this._buildTimeOnlyRewriter.Visit( node ), node );
            }
            else
            {
                return base.Visit( node );
            }
        }

        protected override ExpressionSyntax TransformTupleExpression( TupleExpressionSyntax node )
        {
            // tuple can be initialize from variables and then items take names from variable name
            // but variable name is not safe and could be renamed because of target variables 
            // in this case we initialize tuple with explicit names
            var symbol = (INamedTypeSymbol) this._syntaxTreeAnnotationMap.GetExpressionType( node )!;
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

                var identifierSymbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( token.Parent! );

                if ( this.IsDeclaredWithinTemplate( identifierSymbol! ) )
                {
                    if ( !this._currentMetaContext!.TryGetRunTimeSymbolLocal( identifierSymbol!, out _ ) )
                    {
                        var declaredSymbolNameLocal = this.ReserveRunTimeSymbolName( identifierSymbol! ).Identifier;
                        this._currentMetaContext.AddRunTimeSymbolLocal( identifierSymbol!, declaredSymbolNameLocal );

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

            var transformedToken = base.Transform( token );

            var tokenKind = transformedToken.Kind().ToString();

            if ( tokenKind.EndsWith( "Keyword", StringComparison.Ordinal ) )
            {
                transformedToken = transformedToken.WithTrailingTrivia( Space );
            }

            return transformedToken;
        }

        protected override ExpressionSyntax TransformVariableDeclaration( VariableDeclarationSyntax node )
        {
            switch ( node )
            {
                case { Type: NullableTypeSyntax { ElementType: IdentifierNameSyntax { Identifier: { Text: "dynamic" } } } }:
                    // Variable of dynamic? type needs to become var type (without the ?).
                    return base.TransformVariableDeclaration(
                        VariableDeclaration(
                            IdentifierName( Identifier( "var" ) ),
                            node.Variables ) );

                default:
                    return base.TransformVariableDeclaration( node );
            }
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

        private bool IsDeclaredWithinTemplate( ISymbol? symbol )
        {
            if ( symbol == null )
            {
                return false;
            }
            else
            {
                // Symbol is Declared in Template if ContainsSymbol is Template method or if ContainsSymbol
                // is child level of Template method f.e. local function etc.
                return SymbolEqualityComparer.Default.Equals( symbol.ContainingSymbol, this._rootTemplateSymbol )
                       || this.IsDeclaredWithinTemplate( symbol.ContainingSymbol );
            }
        }

        protected override ExpressionSyntax TransformNullableType( NullableTypeSyntax node )
        {
            if ( node.ElementType is IdentifierNameSyntax identifier && string.Equals( identifier.Identifier.Text, "dynamic", StringComparison.Ordinal ) )
            {
                // Avoid transforming "dynamic?" into "var?".
                return base.TransformIdentifierName( IdentifierName( Identifier( "var" ) ) );
            }
            else
            {
                return base.TransformNullableType( node );
            }
        }

        private ExpressionSyntax TransformIdentifierToken( IdentifierNameSyntax node )
        {
            if ( string.Equals( node.Identifier.Text, "dynamic", StringComparison.Ordinal ) )
            {
                // We change all dynamic into var in the template.
                return base.TransformIdentifierName( IdentifierName( Identifier( "var" ) ) );
            }

            // If the identifier is declared withing the template, the expanded name is given by the TemplateExpansionContext and
            // stored in a template variable named __foo, where foo is the variable name in the template. This variable is defined
            // and initialized in the VisitVariableDeclarator.
            // For identifiers declared outside of the template we just call the regular Roslyn SyntaxFactory.IdentifierName().
            var identifierSymbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            if ( this.IsDeclaredWithinTemplate( identifierSymbol! ) )
            {
                if ( this._currentMetaContext!.TryGetRunTimeSymbolLocal( identifierSymbol!, out var declaredSymbolNameLocal ) )
                {
                    return this.MetaSyntaxFactory.IdentifierName1( IdentifierName( declaredSymbolNameLocal.Text ) );
                }
                else if ( identifierSymbol is IParameterSymbol parameterSymbol
                          && SymbolEqualityComparer.Default.Equals( parameterSymbol.ContainingSymbol, this._rootTemplateSymbol ) )
                {
                    // We have a reference to a template parameter. Currently, only introductions can have template parameters, and these don't need
                    // to be renamed.

                    return base.TransformIdentifierName( node );
                }
                else
                {
                    // That should not happen in a correct compilation because IdentifierName is used only for an identifier reference, not an identifier definition.
                    // Identifier definitions should be processed by Transform(SyntaxToken).

                    // However, this can happen in an incorrect/incomplete compilation. In this case, returning anything is fine.
                    this.Report(
                        TemplatingDiagnosticDescriptors.UndeclaredRunTimeIdentifier.CreateDiagnostic(
                            this._syntaxTreeAnnotationMap.GetLocation( node ),
                            node.Identifier.Text ) );

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
                var transformedExpression = this.Transform( node.Expression );
                var transformedArgument = this.MetaSyntaxFactory.Argument( transformedExpression );

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

        protected override ExpressionSyntax TransformExpression( ExpressionSyntax expression, ExpressionSyntax originalExpression )
            => this.CreateRunTimeExpression( expression, originalExpression );

        /// <summary>
        /// Transforms an <see cref="ExpressionSyntax"/> that instantiates a <see cref="RuntimeExpression"/>
        /// that represents the input.
        /// </summary>
        private ExpressionSyntax CreateRunTimeExpression( ExpressionSyntax expression, SyntaxNode originalExpression )
        {
            switch ( expression.Kind() )
            {
                // TODO: We need to transform null and default values though. How to do this right then?
                case SyntaxKind.NullLiteralExpression:
                case SyntaxKind.DefaultLiteralExpression:
                    return InvocationExpression(
                            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.RuntimeExpression) ) )
                        .AddArgumentListArguments( Argument( this.MetaSyntaxFactory.LiteralExpression( this.Transform( expression.Kind() ) ) ) );

                case SyntaxKind.DefaultExpression:
                    return InvocationExpression(
                            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.RuntimeExpression) ) )
                        .AddArgumentListArguments( Argument( this.MetaSyntaxFactory.DefaultExpression( this.Transform( ((DefaultExpressionSyntax) expression).Type ) ) ) );
                    return expression;

                case SyntaxKind.IdentifierName:
                    {
                        var identifierName = (IdentifierNameSyntax) expression;

                        if ( identifierName.IsVar )
                        {
                            return this.TransformIdentifierName( (IdentifierNameSyntax) expression );
                        }

                        break;
                    }

                case SyntaxKind.SimpleLambdaExpression:
                    break;

                case SyntaxKind.ThisExpression:
                    // Cannot use 'this' in a context that expects a run-time expression.
                    var location = this._syntaxTreeAnnotationMap.GetLocation( expression );

                    // Find a meaningful parent exception.
                    var parentExpression = expression.Ancestors()
                                               .Where( n => n is InvocationExpressionSyntax or BinaryExpressionSyntax )
                                               .FirstOrDefault()
                                           ?? expression;

                    this.Report( TemplatingDiagnosticDescriptors.CannotUseThisInRunTimeContext.CreateDiagnostic( location, parentExpression.ToString() ) );

                    return expression;
            }

            var type = this._syntaxTreeAnnotationMap.GetExpressionType( expression )!;

            // A local function that wraps the input `expression` into a LiteralExpression.
            ExpressionSyntax CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind syntaxKind )
            {
                return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.RuntimeExpression) ) )
                    .AddArgumentListArguments(
                        Argument(
                            this.MetaSyntaxFactory.LiteralExpression(
                                this.Transform( syntaxKind ),
                                this.MetaSyntaxFactory.Literal( expression ) ) ),
                        Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( DocumentationCommentId.CreateDeclarationId( type ) ) ) ) );
            }

            if ( type is IErrorTypeSymbol )
            {
                // There is a compile-time error. Return default.
                return LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) );
            }

            // ReSharper disable once ConstantConditionalAccessQualifier
            switch ( type?.Name )
            {
                case "dynamic":
                case "Task" when type is INamedTypeSymbol { IsGenericType: true } namedType && namedType.TypeArguments[0] is IDynamicTypeSymbol &&
                                 type.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks":
                case "IEnumerable" or "IEnumerator" or "IAsyncEnumerable" or "IAsyncEnumerator"
                    when type is INamedTypeSymbol { IsGenericType: true } namedType2 && namedType2.TypeArguments[0] is IDynamicTypeSymbol &&
                         type.ContainingNamespace.ToDisplayString() == "System.Collections.Generic":

                    var expressionText = SyntaxFactoryEx.LiteralExpression( originalExpression.ToString() );
                    var location = this._templateMetaSyntaxFactory.Location( this._syntaxTreeAnnotationMap.GetLocation( originalExpression ) );

                    return InvocationExpression( this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.GetDynamicSyntax) ) )
                        .AddArgumentListArguments(
                            Argument( CastExpression( NullableType( PredefinedType( Token( SyntaxKind.ObjectKeyword ) ) ), expression ) ),
                            Argument( expressionText ),
                            Argument( location ) );

                case "String":
                    return CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind.StringLiteralExpression );

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
                    return CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind.NumericLiteralExpression );

                case nameof(Char):
                    return CreateRunTimeExpressionForLiteralCreateExpressionFactory( SyntaxKind.CharacterLiteralExpression );

                case nameof(Boolean):
                    return InvocationExpression(
                            this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.RuntimeExpression) ) )
                        .AddArgumentListArguments(
                            Argument(
                                InvocationExpression( this.MetaSyntaxFactory.SyntaxFactoryMethod( nameof(LiteralExpression) ) )
                                    .AddArgumentListArguments(
                                        Argument(
                                            InvocationExpression(
                                                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.Boolean) ) )
                                                .AddArgumentListArguments( Argument( expression ) ) ) ) ),
                            Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( "T:System.Boolean" ) ) ) );

                case null:
                    throw new AssertionFailedException( $"Cannot convert {expression.Kind()} '{expression}' to a run-time value." );

                default:
                    // Try to find a serializer for this type.
                    if ( this._serializableTypes.IsSerializable( type, this._syntaxTreeAnnotationMap.GetLocation( expression ), this ) )
                    {
                        return InvocationExpression(
                            this._templateMetaSyntaxFactory.GenericTemplateSyntaxFactoryMember(
                                nameof(TemplateSyntaxFactory.Serialize),
                                this.MetaSyntaxFactory.Type( type ) ),
                            ArgumentList( SingletonSeparatedList( Argument( expression ) ) ) );
                    }
                    else
                    {
                        // We don't have a valid tree, but let the compilation continue. The call to IsSerializable wrote a diagnostic.
                        return LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) );
                    }
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
                 this._syntaxTreeAnnotationMap.GetExpressionType( node.Expression ) is IDynamicTypeSymbol )
            {
                return InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.DynamicMemberAccessExpression) ),
                    ArgumentList(
                        SeparatedList(
                            new[]
                            {
                                Argument( this.CastToDynamicExpression( (ExpressionSyntax) this.Visit( node.Expression )! ) ),
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

            if ( this.GetTransformationKind( node ) == TransformationKind.Transform
                 || this._templateMemberClassifier.IsDynamicType( node.Expression ) )
            {
                return this.TransformExpressionStatement( node );
            }
            else
            {
                var transformedExpression = this.Visit( node.Expression );

                if ( transformedExpression == null )
                {
                    return null;
                }
                else
                {
                    return node.Update(
                        this.VisitList( node.AttributeLists ),
                        (ExpressionSyntax) transformedExpression,
                        this.VisitToken( node.SemicolonToken ) );
                }
            }
        }

        protected override ExpressionSyntax TransformExpressionStatement( ExpressionStatementSyntax node )
        {
            if ( node.Expression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax { Identifier: { Text: "_" } } } assignment &&
                 this.IsCompileTimeDynamic( assignment.Right ) )
            {
                // Process the statement "_ = meta.XXX()", where "meta.XXX()" is a call to a compile-time dynamic method. 

                var invocationExpression = InvocationExpression(
                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.DynamicDiscardAssignment) ) )
                    .AddArgumentListArguments(
                        Argument( this.CastToDynamicExpression( (ExpressionSyntax) this._buildTimeOnlyRewriter.Visit( assignment.Right ) ) ) );

                return this.WithCallToAddSimplifierAnnotation( invocationExpression );
            }

            var expression = this.Transform( node.Expression );

            var toArrayStatementExpression = InvocationExpression(
                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.ToStatement) ),
                ArgumentList( SingletonSeparatedList( Argument( expression ) ) ) );

            return toArrayStatementExpression;
        }

        public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
        {
            var transformationKind = this.GetTransformationKind( node );

            if ( node.IsNameOf() )
            {
                // nameof is always transformed into a literal.
                var name = node.GetNameOfValue();

                if ( transformationKind == TransformationKind.Transform )
                {
                    return this.MetaSyntaxFactory.LiteralExpression(
                        this.MetaSyntaxFactory.Kind( SyntaxKind.StringLiteralExpression ),
                        this.MetaSyntaxFactory.Literal( name ) );
                }
                else
                {
                    return SyntaxFactoryEx.LiteralExpression( name );
                }
            }
            else if ( this._buildTimeOnlyRewriter.TryRewriteProceedInvocation( node, out var proceedNode ) )
            {
                return proceedNode;
            }

            if ( transformationKind != TransformationKind.Transform &&
                 node.ArgumentList.Arguments.Any( a => this._templateMemberClassifier.IsDynamicParameter( a ) ) )
            {
                // We are transforming a call to a compile-time method that accepts dynamic arguments.

                SyntaxNode? LocalTransformArgument( ArgumentSyntax a )
                {
                    if ( this._templateMemberClassifier.IsDynamicParameter( a ) )
                    {
                        var expressionScope = a.Expression.GetScopeFromAnnotation().GetValueOrDefault();
                        var transformedExpression = (ExpressionSyntax) this.Visit( a.Expression )!;

                        switch ( expressionScope )
                        {
                            case TemplatingScope.Dynamic:
                            case TemplatingScope.RunTimeOnly:
                                return Argument( transformedExpression );

                            default:
                                return Argument( this.CreateRunTimeExpression( transformedExpression, a.Expression ) );
                        }
                    }
                    else
                    {
                        return this.Visit( a );
                    }
                }

                var transformedArguments = node.ArgumentList.Arguments.Select( syntax => LocalTransformArgument( syntax )! ).ToArray();

                return node.Update(
                    (ExpressionSyntax) this.Visit( node.Expression )!,
                    ArgumentList( SeparatedList( transformedArguments )! ) );
            }
            else if ( this._templateMemberClassifier.IsDynamicType( node.Expression ) )
            {
                // We are in an invocation like: `meta.This.Foo(...)`.
            }
            else if ( this._templateMemberClassifier.IsRunTimeMethod( node.Expression ) )
            {
                // Replace `meta.RunTime(x)` to `x`.
                var expression = node.ArgumentList.Arguments[0].Expression;

                return this.CreateRunTimeExpression( expression, expression );
            }
            else
            {
                // Process special methods.

                switch ( this._templateMemberClassifier.GetMetaMemberKind( node.Expression ) )
                {
                    case MetaMemberKind.Comment:
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
                }
            }

            // Expand extension methods.
            if ( transformationKind == TransformationKind.Transform )
            {
                var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node.Expression );

                if ( symbol is IMethodSymbol { IsExtensionMethod: true } method )
                {
                    var receiver = ((MemberAccessExpressionSyntax) node.Expression).Expression;

                    List<ArgumentSyntax> arguments = new( node.ArgumentList.Arguments.Count + 1 )
                    {
                        Argument( receiver ).WithTemplateAnnotationsFrom( receiver )
                    };

                    arguments.AddRange( node.ArgumentList.Arguments );

                    var replacementNode = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                this.MetaSyntaxFactory.Type( method.ContainingType ),
                                IdentifierName( method.Name ) ),
                            ArgumentList( SeparatedList( arguments ) ) )
                        .WithSymbolAnnotationsFrom( node )
                        .WithTemplateAnnotationsFrom( node );

                    var result = this.VisitInvocationExpression( replacementNode );

                    return result;
                }
            }

            return base.VisitInvocationExpression( node );
        }

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            if ( node.Body == null && node.ExpressionBody == null )
            {
                // Not supported or incomplete syntax.
                return node;
            }

            this.Indent( 3 );

            // TODO: templates may support build-time parameters, which must to the compiled template method.

            BlockSyntax body;

            if ( node.Body != null )
            {
                body = (BlockSyntax) this.BuildRunTimeBlock( node.Body, false );
            }
            else
            {
                var isVoid = node.ReturnType is PredefinedTypeSyntax predefinedType && predefinedType.Keyword.Kind() == SyntaxKind.VoidKeyword;

                body = (BlockSyntax) this.BuildRunTimeBlock(
                    node.ExpressionBody.AssertNotNull().Expression,
                    false,
                    isVoid );
            }

            var result = this.CreateTemplateMethod( node, body );

            this.Unindent( 3 );

            return result;
        }

        public override SyntaxNode VisitAccessorDeclaration( AccessorDeclarationSyntax node )
        {
            if ( node.Body == null && node.ExpressionBody == null )
            {
                // Not supported or incomplete syntax.
                return node;
            }

            this.Indent( 3 );

            // TODO: templates may support build-time parameters, which must to the compiled template method.

            BlockSyntax body;

            if ( node.Body != null )
            {
                body = (BlockSyntax) this.BuildRunTimeBlock( node.Body, false );
            }
            else
            {
                var isVoid = node.Keyword.Kind() != SyntaxKind.GetKeyword;

                body = (BlockSyntax) this.BuildRunTimeBlock(
                    node.ExpressionBody.AssertNotNull().Expression,
                    false,
                    isVoid );
            }

            var result = this.CreateTemplateMethod( node, body );

            this.Unindent( 3 );

            return result;
        }

        public override SyntaxNode VisitPropertyDeclaration( PropertyDeclarationSyntax node )
        {
            this.Indent( 3 );

            var body = (BlockSyntax) this.BuildRunTimeBlock( node.ExpressionBody.AssertNotNull().Expression, false, false );

            var result = this.CreateTemplateMethod( node, body );

            this.Unindent( 3 );

            return result;
        }

        private MethodDeclarationSyntax CreateTemplateMethod( SyntaxNode node, BlockSyntax body )
            => MethodDeclaration(
                    this.MetaSyntaxFactory.Type( typeof(SyntaxNode) ),
                    Identifier( this._templateName ) )
                .WithModifiers( TokenList( Token( SyntaxKind.PublicKeyword ) ) )
                .NormalizeWhitespace()
                .WithBody( body )
                .WithLeadingTrivia( node.GetLeadingTrivia() )
                .WithTrailingTrivia( LineFeed, LineFeed );

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
        /// Generates a run-time block from expression.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="generateExpression"><c>true</c> if the returned <see cref="SyntaxNode"/> must be an
        /// expression (in this case, a delegate invocation is returned), or <c>false</c> if it can be a statement
        /// (in this case, a return statement is returned).</param>
        /// <returns></returns>
        private SyntaxNode BuildRunTimeBlock( ExpressionSyntax node, bool generateExpression, bool isVoid )
        {
            StatementSyntax statement;

            if ( node is ThrowExpressionSyntax throwExpression )
            {
                statement = ThrowStatement( throwExpression.ThrowKeyword, throwExpression.Expression, Token( SyntaxKind.SemicolonToken ) );
            }
            else
            {
                statement = isVoid ? ExpressionStatement( node ) : ReturnStatement( node );
            }

            return this.BuildRunTimeBlock( () => this.ToMetaStatements( statement ), generateExpression );
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
            => this.BuildRunTimeBlock( () => this.ToMetaStatements( node.Statements ).ToList(), generateExpression );

        /// <summary>
        /// Generates a run-time block.
        /// </summary>
        /// <param name="createMetaStatements">Function that returns meta statements.</param>
        /// <param name="generateExpression"><c>true</c> if the returned <see cref="SyntaxNode"/> must be an
        /// expression (in this case, a delegate invocation is returned), or <c>false</c> if it can be a statement
        /// (in this case, a return statement is returned).</param>
        /// <returns></returns>
        private SyntaxNode BuildRunTimeBlock( Func<List<StatementSyntax>> createMetaStatements, bool generateExpression )
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

                this._currentMetaContext.Statements.AddRange( createMetaStatements() );

                // TemplateSyntaxFactory.ToStatementArray( __s1 )
                var toArrayStatementExpression = InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.ToStatementArray) ),
                    ArgumentList( SingletonSeparatedList( Argument( IdentifierName( this._currentMetaContext.StatementListVariableName ) ) ) ) );

                if ( generateExpression )
                {
                    // return TemplateSyntaxFactory.ToStatementArray( __s1 );

                    var returnStatementSyntax = ReturnStatement( toArrayStatementExpression ).WithLeadingTrivia( this.GetIndentation() ).NormalizeWhitespace();
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
                                    .NormalizeWhitespace()
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

        protected override ExpressionSyntax TransformInterpolatedStringExpression( InterpolatedStringExpressionSyntax node )
        {
            List<ExpressionSyntax> transformedContents = new( node.Contents.Count );

            foreach ( var content in node.Contents )
            {
                switch ( content )
                {
                    case InterpolatedStringTextSyntax text:
                        transformedContents.Add( this.TransformInterpolatedStringText( text ) );

                        break;

                    case InterpolationSyntax interpolation:
                        if ( this.GetTransformationKind( interpolation ) == TransformationKind.None )
                        {
                            // We have a compile-time interpolation (e.g. formatting string argument).
                            // We can evaluate it at compile time and add it as a text content.

                            var compileTimeInterpolatedString =
                                InterpolatedStringExpression(
                                    Token( SyntaxKind.InterpolatedStringStartToken ),
                                    SingletonList<InterpolatedStringContentSyntax>( interpolation ),
                                    Token( SyntaxKind.InterpolatedStringEndToken ) );

                            var token = this.MetaSyntaxFactory.Token(
                                LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ),
                                this.Transform( SyntaxKind.InterpolatedStringTextToken ),
                                compileTimeInterpolatedString,
                                compileTimeInterpolatedString,
                                LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ) );

                            transformedContents.Add( this.MetaSyntaxFactory.InterpolatedStringText( token ) );
                        }
                        else
                        {
                            transformedContents.Add( this.TransformInterpolation( interpolation ) );
                        }

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }

            this.Indent();

            var createInterpolatedString = InvocationExpression( this.MetaSyntaxFactory.SyntaxFactoryMethod( nameof(InterpolatedStringExpression) ) )
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument( this.Transform( node.StringStartToken ) ).WithLeadingTrivia( this.GetIndentation() ),
                                Token( SyntaxKind.CommaToken ).WithTrailingTrivia( GetLineBreak() ),
                                Argument( this.MetaSyntaxFactory.List<InterpolatedStringContentSyntax>( transformedContents ) )
                                    .WithLeadingTrivia( this.GetIndentation() ),
                                Token( SyntaxKind.CommaToken ).WithTrailingTrivia( GetLineBreak() ),
                                Argument( this.Transform( node.StringEndToken ) ).WithLeadingTrivia( this.GetIndentation() )
                            } ) ) );

            var callRender = InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.RenderInterpolatedString) ),
                    ArgumentList( SingletonSeparatedList( Argument( createInterpolatedString ) ) ) )
                .NormalizeWhitespace();

            this.Unindent();

            return callRender;
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

            // It seems that trivia can be lost upstream, there can be a missing one between the 'in' keyword and the expression. Add them to be sure.
            return ForEachStatement(
                node.Type.WithTrailingTrivia( Space ),
                node.Identifier.WithTrailingTrivia( Space ),
                node.Expression.WithLeadingTrivia( Space ),
                statement );
        }

        /// <summary>
        /// Determines if the expression will be transformed into syntax that instantiates an <see cref="IDynamicExpression"/>.
        /// </summary>
        private bool IsCompileTimeDynamic( ExpressionSyntax? expression )
            => expression != null
               && expression.GetScopeFromAnnotation() == TemplatingScope.CompileTimeOnlyReturningRuntimeOnly
               && this.GetTransformationKind( expression ) != TransformationKind.Transform
               && this._syntaxTreeAnnotationMap.GetExpressionType( expression ) is IDynamicTypeSymbol;

        public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
        {
            InvocationExpressionSyntax invocationExpression;

            if ( this.IsCompileTimeDynamic( node.Expression ) )
            {
                // We have a dynamic parameter. We need to call the second overload of ReturnStatement, the one that accepts the IDynamicExpression
                // itself and not the syntax.

                var expression = (ExpressionSyntax) this.Visit( node.Expression )!;

                invocationExpression = InvocationExpression(
                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.DynamicReturnStatement) ) )
                    .AddArgumentListArguments( Argument( this.CastToDynamicExpression( expression ) ) );

                // TODO: pass expressionText and Location
            }
            else
            {
                var expression = this.Transform( node.Expression );

                invocationExpression = InvocationExpression(
                        this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.ReturnStatement) ) )
                    .AddArgumentListArguments( Argument( expression ) );
            }

            return this.WithCallToAddSimplifierAnnotation( invocationExpression );
        }

        private CastExpressionSyntax CastToDynamicExpression( ExpressionSyntax expression )
            => CastExpression(
                this.MetaSyntaxFactory.Type( typeof(IDynamicExpression) ),
                expression );

        public override SyntaxNode VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
        {
            var declaration = node.Declaration;

            if ( declaration.Variables.Count == 1 )
            {
                var declarator = declaration.Variables[0];

                if ( declarator.Initializer != null )
                {
                    if ( this.IsCompileTimeDynamic( declarator.Initializer.Value ) )
                    {
                        var invocationExpression = InvocationExpression(
                                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.DynamicLocalDeclaration) ) )
                            .AddArgumentListArguments(
                                Argument( (ExpressionSyntax) this.Visit( declaration.Type )! ),
                                Argument( this.Transform( declarator.Identifier ) ),
                                Argument( this.CastToDynamicExpression( (ExpressionSyntax) this.Visit( declarator.Initializer.Value )! ) ) );

                        return this.WithCallToAddSimplifierAnnotation( invocationExpression );

                        // TODO: pass expressionText and Location
                    }
                }
            }

            return base.VisitLocalDeclarationStatement( node );
        }

        public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
        {
            if ( node.Identifier.Kind() == SyntaxKind.IdentifierToken && !node.IsVar )
            {
                var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

                if ( this.GetTransformationKind( node ) == TransformationKind.Transform )
                {
                    // Fully qualifies simple identifiers.

                    if ( symbol is INamespaceOrTypeSymbol namespaceOrType )
                    {
                        return this.Transform( LanguageServiceFactory.CSharpSyntaxGenerator.NameExpression( namespaceOrType ) );
                    }
                    else if ( symbol is { IsStatic: true } && node.Parent is not MemberAccessExpressionSyntax && node.Parent is not AliasQualifiedNameSyntax )
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
                                    this.Transform( LanguageServiceFactory.CSharpSyntaxGenerator.NameExpression( symbol.ContainingType ) ),
                                    this.MetaSyntaxFactory.IdentifierName2( SyntaxFactoryEx.LiteralExpression( node.Identifier.Text ) ) );
                        }
                    }
                }
            }

            return base.VisitIdentifierName( node );
        }

        private ExpressionSyntax WithCallToAddSimplifierAnnotation( ExpressionSyntax expression )
            => InvocationExpression(
                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.AddSimplifierAnnotations) ),
                ArgumentList( SingletonSeparatedList( Argument( expression ) ) ) );

        /// <summary>
        /// Transforms a type or namespace so that it is fully qualified, but return <c>false</c> if the input <paramref name="node"/>
        /// is not a type or namespace.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="transformedNode"></param>
        /// <returns></returns>
        private bool TryVisitNamespaceOrTypeName( SyntaxNode node, [NotNullWhen( true )] out SyntaxNode? transformedNode )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            switch ( symbol )
            {
                case INamespaceOrTypeSymbol namespaceOrType:
                    var nameExpression = LanguageServiceFactory.CSharpSyntaxGenerator.NameExpression( namespaceOrType );

                    transformedNode = this.GetTransformationKind( node ) == TransformationKind.Transform
                        ? this.WithCallToAddSimplifierAnnotation( this.Transform( nameExpression ) )
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

        protected override ExpressionSyntax TransformQualifiedName( QualifiedNameSyntax node )
        {
            var transformed = base.TransformQualifiedName( node );

            if ( node.HasAnnotation( Simplifier.Annotation ) )
            {
                transformed = this.WithCallToAddSimplifierAnnotation( transformed );
            }

            return transformed;
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

        protected override ExpressionSyntax TransformConditionalExpression( ConditionalExpressionSyntax node )
        {
            if ( node.WhenFalse is ThrowExpressionSyntax || node.WhenTrue is ThrowExpressionSyntax )
            {
                // If any of the expressions if a throw exception, we cannot reduce it at compile time because it would generate incorrect syntax.
                return base.TransformConditionalExpression( node );
            }

            var transformedCondition = this.Transform( node.Condition );
            var transformedWhenTrue = this.Transform( node.WhenTrue );
            var transformedWhenFalse = this.Transform( node.WhenFalse );

            return InvocationExpression(
                this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.ConditionalExpression) ),
                ArgumentList(
                    SeparatedList( new[] { Argument( transformedCondition ), Argument( transformedWhenTrue ), Argument( transformedWhenFalse ) } ) ) );
        }

        protected override ExpressionSyntax TransformYieldStatement( YieldStatementSyntax node )
        {
            if ( node.Kind() == SyntaxKind.YieldReturnStatement && node.Expression is InvocationExpressionSyntax invocation &&
                 this._templateMemberClassifier.GetMetaMemberKind( invocation.Expression ) == MetaMemberKind.Proceed )
            {
                // We have a 'yield return meta.Proceed()' statement.

                return InvocationExpression(
                    this._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.ConditionalExpression) ) );
            }
            else
            {
                return base.TransformYieldStatement( node );
            }
        }
    }
}