// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

// TODO: A lot methods here are called multiple times. Optimize.
// TODO: Split into a subclass for each declaration type?

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Provides methods for rewriting of types and members.
    /// </summary>
    internal partial class LinkerRewritingDriver
    {
        private readonly LinkerIntroductionRegistry _introductionRegistry;
        private readonly LinkerAnalysisRegistry _analysisRegistry;
        private readonly IReadOnlyList<Inliner> _inliners;

        public AspectReferenceResolver ReferenceResolver { get; }

        public Compilation IntermediateCompilation { get; }

        public UserDiagnosticSink DiagnosticSink { get; }

        public LinkerRewritingDriver(
            Compilation intermediateCompilation,
            LinkerIntroductionRegistry introductionRegistry,
            LinkerAnalysisRegistry analysisRegistry,
            AspectReferenceResolver referenceResolver,
            UserDiagnosticSink diagnosticSink,
            IReadOnlyList<Inliner> inliners )
        {
            this._introductionRegistry = introductionRegistry;
            this._analysisRegistry = analysisRegistry;
            this.IntermediateCompilation = intermediateCompilation;
            this._inliners = inliners;
            this.DiagnosticSink = diagnosticSink;
            this.ReferenceResolver = referenceResolver;
        }

        public bool IsDiscarded( ISymbol symbol, ResolvedAspectReferenceSemantic semantic )
            => symbol switch
            {
                IMethodSymbol methodSymbol => this.IsDiscarded( methodSymbol, semantic ),
                IPropertySymbol propertySymbol => this.IsDiscarded( propertySymbol, semantic ),
                IEventSymbol eventSymbol => this.IsDiscarded( eventSymbol, semantic ),
                IFieldSymbol => false,
                _ => throw new AssertionFailedException()
            };

        private bool IsInlineable( ISymbol inlinedSymbol, ResolvedAspectReferenceSemantic semantic )
            => inlinedSymbol switch
            {
                IMethodSymbol methodSymbol => this.IsInlineable( methodSymbol, semantic ),
                IPropertySymbol propertySymbol => this.IsInlineable( propertySymbol, semantic ),
                IEventSymbol eventSymbol => this.IsInlineable( eventSymbol, semantic ),
                IFieldSymbol => false,
                _ => throw new AssertionFailedException()
            };

        private bool IsInlineableReference( ResolvedAspectReference aspectReference, MethodKind methodKind )
            => aspectReference.Specification.Flags.HasFlag( AspectReferenceFlags.Inlineable )
               && IsAsync( GetMethod( aspectReference.ContainingSymbol, methodKind ) ) == IsAsync( GetMethod( aspectReference.ResolvedSymbol, methodKind ) )
               && IsIterator( GetMethod( aspectReference.ContainingSymbol, methodKind ) )
               == IsIterator( GetMethod( aspectReference.ResolvedSymbol, methodKind ) )
               && this.GetInliner( aspectReference, out _ );

        private static IMethodSymbol? GetMethod( ISymbol symbol, MethodKind kind )
            => symbol switch
            {
                IMethodSymbol method => method,
                IPropertySymbol property => kind switch
                {
                    MethodKind.PropertyGet => property.GetMethod,
                    MethodKind.PropertySet => property.SetMethod,
                    _ => throw new AssertionFailedException()
                },
                IEventSymbol @event => kind switch
                {
                    MethodKind.EventAdd => @event.AddMethod,
                    MethodKind.EventRemove => @event.RemoveMethod,
                    _ => throw new AssertionFailedException()
                },
                _ => throw new AssertionFailedException()
            };

        private static bool IsAsync( IMethodSymbol? symbol ) => symbol is { IsAsync: true };

        private static bool IsIterator( IMethodSymbol? symbol ) => symbol != null && IteratorHelper.IsIterator( symbol );

        private bool GetInliner( ResolvedAspectReference aspectReference, [NotNullWhen( true )] out Inliner? matchingInliner )
        {
            if ( !SymbolEqualityComparer.Default.Equals( aspectReference.ContainingSymbol.ContainingType, aspectReference.ResolvedSymbol.ContainingType ) )
            {
                // Never inline method from another type.
                matchingInliner = null;

                return false;
            }

            var semanticModel = this.IntermediateCompilation.GetSemanticModel( aspectReference.Expression.SyntaxTree );

            foreach ( var inliner in this._inliners )
            {
                if ( inliner.CanInline( aspectReference, semanticModel ) )
                {
                    // We have inliner that will be able to inline the reference.
                    matchingInliner = inliner;

                    return true;
                }
            }

            matchingInliner = null;

            return false;
        }

        /// <summary>
        /// Assembles a linked body of the method/accessor, where aspect reference annotations are replaced by target symbols and inlineable references are inlined.
        /// </summary>
        /// <param name="symbol">Method or accessor symbol.</param>
        /// <returns>Block representing the linked body.</returns>
        public BlockSyntax GetLinkedBody( IMethodSymbol symbol, InliningContext inliningContext )
        {
            var replacements = new Dictionary<SyntaxNode, SyntaxNode?>();
            var bodyRootNode = this.GetBodyRootNode( symbol, out var isImplicitlyLinked );

            if ( !isImplicitlyLinked )
            {
                AddAspectReferenceReplacements();
            }

            if ( inliningContext.HasIndirectReturn )
            {
                // If the inlining context has indirect return (i.e. return through variable/label), we need to replace all remaining return statements.
                AddReturnNodeReplacements();
            }

            var rewrittenBody = this.RewriteBody( bodyRootNode, symbol, replacements );

            // TODO: This is not a nice place to have this, but there are problems if we attempt to do this using the replacements.
            //       The replacement block would already have different statements that would not match original instances.
            //       We would need either to annotate child statements or provide special mechanism for block changes (which should be enough for now).
            if ( inliningContext.HasIndirectReturn
                 && symbol.ReturnsVoid
                 && !SymbolEqualityComparer.Default.Equals( symbol, inliningContext.CurrentDeclaration ) )
            {
                // Add the implicit return for void methods.
                inliningContext.UseLabel();

                rewrittenBody =
                    Block(
                            rewrittenBody,
                            CreateGotoStatement() )
                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }

            // Add the SourceCode annotation, if it is source code.
            if ( !(symbol.GetPrimarySyntaxReference() is { } primarySyntax && primarySyntax.GetSyntax().HasAnnotation( FormattingAnnotations.GeneratedCode )) )
            {
                rewrittenBody = rewrittenBody.AddSourceCodeAnnotation();
            }

            return rewrittenBody;

            void AddAspectReferenceReplacements()
            {
                // Get all aspect references that were found in the symbol during analysis.
                var containedAspectReferences = this._analysisRegistry.GetContainedAspectReferences( symbol );

                // Collect syntax node replacements from inliners and redirect the rest to correct targets.
                foreach ( var aspectReference in containedAspectReferences )
                {
                    if ( this.IsInlineable( aspectReference.ResolvedSymbol, aspectReference.Semantic ) )
                    {
                        if ( !this.GetInliner( aspectReference, out var inliner ) )
                        {
                            throw new AssertionFailedException();
                        }

                        inliner.Inline( inliningContext, aspectReference, out var replacedNode, out var newNode );
                        replacements.Add( replacedNode, newNode );
                    }
                    else
                    {
                        var linkedExpression = this.GetLinkedExpression( aspectReference );
                        replacements.Add( aspectReference.Expression, linkedExpression );
                    }
                }
            }

            void AddReturnNodeReplacements()
            {
                foreach ( var returnNode in GetReturnNodes( bodyRootNode ) )
                {
                    if ( returnNode is ReturnStatementSyntax returnStatement )
                    {
                        if ( !replacements.ContainsKey( returnStatement ) )
                        {
                            inliningContext.UseLabel();

                            if ( returnStatement.Expression != null )
                            {
                                replacements[returnStatement] =
                                    Block(
                                            CreateAssignmentStatement( returnStatement.Expression ),
                                            CreateGotoStatement() )
                                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                            }
                            else
                            {
                                replacements[returnStatement] = CreateGotoStatement();
                            }
                        }
                    }
                    else if ( returnNode is ExpressionSyntax returnExpression )
                    {
                        inliningContext.UseLabel();

                        if ( symbol.ReturnsVoid )
                        {
                            replacements[returnNode] =
                                Block(
                                        ExpressionStatement( returnExpression ),
                                        CreateGotoStatement() )
                                    .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else
                        {
                            replacements[returnNode] =
                                Block(
                                        CreateAssignmentStatement( returnExpression ),
                                        CreateGotoStatement() )
                                    .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                    }
                    else
                    {
                        throw new AssertionFailedException();
                    }
                }
            }

            StatementSyntax CreateAssignmentStatement( ExpressionSyntax expression )
            {
                IdentifierNameSyntax identifier;

                if ( inliningContext.ReturnVariableName != null )
                {
                    identifier = IdentifierName( inliningContext.ReturnVariableName );
                }
                else
                {
                    identifier =
                        IdentifierName(
                            Identifier(
                                TriviaList(),
                                SyntaxKind.UnderscoreToken,
                                "_",
                                "_",
                                TriviaList() ) );
                }

                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            identifier,
                            expression ) );
            }

            GotoStatementSyntax CreateGotoStatement()
            {
                return
                    GotoStatement(
                        SyntaxKind.GotoStatement,
                        Token( SyntaxKind.GotoKeyword ).WithLeadingTrivia( ElasticLineFeed ).WithTrailingTrivia( ElasticSpace ),
                        default,
                        IdentifierName( inliningContext.ReturnLabelName.AssertNotNull() ),
                        Token( SyntaxKind.SemicolonToken ) );
            }
        }

        private SyntaxNode GetBodyRootNode( IMethodSymbol symbol, out bool isImplicitlyLinked )
        {
            var declaration = symbol.GetPrimaryDeclaration();

            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                // Override targets are implicitly linked, i.e. no replacement of aspect references is necessary.
                isImplicitlyLinked = true;

                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body ?? methodDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case AccessorDeclarationSyntax accessorDecl:
                        var body = (SyntaxNode?) accessorDecl.Body ?? accessorDecl.ExpressionBody;

                        if ( body != null && !(symbol.AssociatedSymbol != null && IsExplicitInterfaceEventField( symbol.AssociatedSymbol )) )
                        {
                            return body;
                        }
                        else
                        {
                            return GetImplicitAccessorBody( symbol );
                        }

                    case ArrowExpressionClauseSyntax arrowExpressionClause:
                        // Expression-bodied property.
                        return arrowExpressionClause;

                    case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: EventFieldDeclarationSyntax } }:
                        return GetImplicitAccessorBody( symbol );

                    default:
                        throw new AssertionFailedException();
                }
            }

            if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                isImplicitlyLinked = false;

                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body ?? methodDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case AccessorDeclarationSyntax accessorDecl:
                        return (SyntaxNode?) accessorDecl.Body ?? accessorDecl.ExpressionBody ?? throw new AssertionFailedException();

                    default:
                        throw new AssertionFailedException();
                }
            }

            if ( symbol.AssociatedSymbol != null && IsExplicitInterfaceEventField( symbol.AssociatedSymbol ) )
            {
                isImplicitlyLinked = true;

                return GetImplicitAccessorBody( symbol );
            }

            throw new AssertionFailedException();
        }

        private static BlockSyntax GetImplicitAccessorBody( IMethodSymbol symbol )
        {
            switch ( symbol )
            {
                case { MethodKind: MethodKind.PropertyGet }:
                    return GetImplicitGetterBody( symbol );

                case { MethodKind: MethodKind.PropertySet }:
                    return GetImplicitSetterBody( symbol );

                case { MethodKind: MethodKind.EventAdd }:
                    return GetImplicitAdderBody( symbol );

                case { MethodKind: MethodKind.EventRemove }:
                    return GetImplicitRemoverBody( symbol );

                default:
                    throw new InvalidOperationException();
            }
        }

#pragma warning disable CA1822 // Mark members as static
        private BlockSyntax RewriteBody( SyntaxNode bodyRootNode, IMethodSymbol symbol, Dictionary<SyntaxNode, SyntaxNode?> replacements )
#pragma warning restore CA1822 // Mark members as static
        {
            var rewriter = new BodyRewriter( replacements );

            switch ( bodyRootNode )
            {
                case BlockSyntax block:
                    return (BlockSyntax) rewriter.Visit( block ).AssertNotNull();

                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    var rewrittenNode = rewriter.Visit( arrowExpressionClause.Expression );

                    if ( symbol.ReturnsVoid )
                    {
                        switch ( rewrittenNode )
                        {
                            case null:
                                return
                                    Block()
                                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case ExpressionSyntax rewrittenExpression:
                                return
                                    Block( ExpressionStatement( rewrittenExpression ) )
                                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case BlockSyntax rewrittenBlock:
                                return rewrittenBlock;

                            default:
                                throw new AssertionFailedException();
                        }
                    }
                    else
                    {
                        switch ( rewrittenNode )
                        {
                            case null:
                                return
                                    Block(
                                            ReturnStatement(
                                                Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                                                LiteralExpression( SyntaxKind.DefaultLiteralExpression ),
                                                Token( SyntaxKind.SemicolonToken ) ) )
                                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case ExpressionSyntax rewrittenExpression:
                                return
                                    Block(
                                            ReturnStatement(
                                                Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                                                rewrittenExpression,
                                                Token( SyntaxKind.SemicolonToken ) ) )
                                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case BlockSyntax rewrittenBlock:
                                return rewrittenBlock;

                            default:
                                throw new AssertionFailedException();
                        }
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        private static IEnumerable<SyntaxNode> GetReturnNodes( SyntaxNode? rootNode )
        {
            switch ( rootNode )
            {
                case BlockSyntax block:
                    var walker = new ReturnStatementWalker();
                    walker.Visit( block );

                    return walker.ReturnStatements;

                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    return new[] { arrowExpressionClause.Expression };

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether the symbol should be rewritten.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool IsRewriteTarget( ISymbol symbol )
        {
            if ( this._analysisRegistry.IsOverride( symbol ) || this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                return true;
            }

            if ( IsExplicitInterfaceEventField( symbol ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets rewritten member and any additional induced members (e.g. backing field of auto property).
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public IReadOnlyList<MemberDeclarationSyntax> RewriteMember( MemberDeclarationSyntax syntax, ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    return this.RewriteMethod( (MethodDeclarationSyntax) syntax, methodSymbol );

                case IPropertySymbol propertySymbol:
                    return this.RewriteProperty( (PropertyDeclarationSyntax) syntax, propertySymbol );

                case IEventSymbol eventSymbol:
                    return syntax switch
                    {
                        EventDeclarationSyntax eventSyntax => this.RewriteEvent( eventSyntax, eventSymbol ),
                        EventFieldDeclarationSyntax eventFieldSyntax => this.RewriteEventField( eventFieldSyntax, eventSymbol ),
                        _ => throw new InvalidOperationException()
                    };

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets a method symbol that will be the source for the body of the specified declaration. For example, source for the overridden declaration is the last override and source
        /// for the first override is the original declaration.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private IMethodSymbol GetBodySource( IMethodSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                return symbol;
            }

            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                return (IMethodSymbol) this._analysisRegistry.GetLastOverride( symbol ).AssertNotNull();
            }

            if ( symbol.AssociatedSymbol != null && IsExplicitInterfaceEventField( symbol.AssociatedSymbol ) )
            {
                return symbol;
            }

            throw new AssertionFailedException();
        }

        private ExpressionSyntax GetLinkedExpression( ResolvedAspectReference aspectReference )
        {
            if ( !SymbolEqualityComparer.Default.Equals( aspectReference.ResolvedSymbol.ContainingType, aspectReference.ResolvedSymbol.ContainingType ) )
            {
                throw new AssertionFailedException();
            }

            // Determine the target name. Specifically, handle case when the resolved symbol points to the original implementation.
            var targetMemberName =
                aspectReference.Semantic switch
                {
                    ResolvedAspectReferenceSemantic.Original => GetOriginalImplMemberName( aspectReference.ResolvedSymbol ),
                    _ => aspectReference.ResolvedSymbol.Name
                };

            // Presume that all (annotated) aspect references are member access expressions.
            switch ( aspectReference.Expression )
            {
                case MemberAccessExpressionSyntax memberAccessExpression:
                    // The reference expression is member access.

                    if ( SymbolEqualityComparer.Default.Equals(
                        aspectReference.ContainingSymbol.ContainingType,
                        aspectReference.ResolvedSymbol.ContainingType ) )
                    {
                        if ( aspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                        {
                            return memberAccessExpression
                                .WithExpression( ThisExpression() )
                                .WithName( IdentifierName( targetMemberName ) );
                        }
                        else
                        {
                            // This is the same type, we can just change the identifier in the expression.
                            // TODO: Is the target always accessible?
                            return memberAccessExpression.WithName( IdentifierName( targetMemberName ) );
                        }
                    }
                    else
                    {
                        if ( aspectReference.ResolvedSymbol.ContainingType.TypeKind == TypeKind.Interface )
                        {
                            return memberAccessExpression
                                .WithExpression( ThisExpression() )
                                .WithName( IdentifierName( targetMemberName ) );
                        }

                        if ( aspectReference.ResolvedSymbol.IsStatic )
                        {
                            // Static member access where the target is a different type.
                            return MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( aspectReference.ResolvedSymbol.ContainingType ),
                                IdentifierName( targetMemberName ) );
                        }
                        else
                        {
                            if ( aspectReference.ResolvedSymbol.ContainingType.Is( aspectReference.ContainingSymbol.ContainingType ) )
                            {
                                throw new AssertionFailedException( "Resolved symbol is declared in a derived class." );
                            }
                            else if ( aspectReference.ContainingSymbol.ContainingType.Is( aspectReference.ResolvedSymbol.ContainingType ) )
                            {
                                // Resolved symbol is declared in a base class.
                                switch ( memberAccessExpression.Expression )
                                {
                                    case IdentifierNameSyntax:
                                    case BaseExpressionSyntax:
                                    case ThisExpressionSyntax:
                                        return MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            BaseExpression(),
                                            IdentifierName( targetMemberName ) );

                                    default:
                                        var aspectInstance = this.ResolveAspectInstance( aspectReference );

                                        this.DiagnosticSink.Report(
                                            AspectLinkerDiagnosticDescriptors.CannotUseBaseInvokerWithInstanceExpression.CreateDiagnostic(
                                                aspectInstance.TargetDeclaration.GetDiagnosticLocation(),
                                                (aspectInstance.AspectClass.DisplayName, aspectInstance.TargetDeclaration) ) );

                                        return aspectReference.Expression;
                                }
                            }
                            else
                            {
                                // Resolved symbol is unrelated to the containing symbol.
                                return memberAccessExpression.WithName( IdentifierName( targetMemberName ) );
                            }
                        }
                    }

                default:
                    throw new AssertionFailedException();
            }
        }

        private AspectInstance ResolveAspectInstance( ResolvedAspectReference aspectReference )
        {
            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( aspectReference.ContainingSymbol );

            return introducedMember.AssertNotNull().Introduction.Advice.Aspect;
        }

        internal static string GetOriginalImplMemberName( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    if ( methodSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( methodSymbol.ExplicitInterfaceImplementations[0].Name );
                    }
                    else
                    {
                        return CreateName( methodSymbol.Name );
                    }

                case IPropertySymbol propertySymbol:
                    if ( propertySymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( propertySymbol.ExplicitInterfaceImplementations[0].Name );
                    }
                    else
                    {
                        return CreateName( propertySymbol.Name );
                    }

                case IEventSymbol eventSymbol:
                    if ( eventSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( eventSymbol.ExplicitInterfaceImplementations[0].Name );
                    }
                    else
                    {
                        return CreateName( eventSymbol.Name );
                    }

                default:
                    throw new AssertionFailedException();
            }

            string CreateName( string name )
            {
                var hint = $"{name}_Source";
                
                for ( var i = 2; symbol.ContainingType.GetMembers( hint ).Any(); i++ )
                {
                    hint = $"{name}_Source{i}";
                }

                return hint;
            }
        }

        private static string GetBackingFieldName( ISymbol symbol )
        {
            string name;

            switch ( symbol )
            {
                case IPropertySymbol propertySymbol:
                    name =
                        propertySymbol.ExplicitInterfaceImplementations.Any()
                            ? propertySymbol.ExplicitInterfaceImplementations[0].Name
                            : propertySymbol.Name;

                    break;

                case IEventSymbol eventSymbol:
                    name =
                        eventSymbol.ExplicitInterfaceImplementations.Any()
                            ? eventSymbol.ExplicitInterfaceImplementations[0].Name
                            : eventSymbol.Name;

                    break;

                default:
                    throw new AssertionFailedException();
            }

            var firstPropertyLetter = name.Substring( 0, 1 );
            var camelCasePropertyName = firstPropertyLetter.ToLowerInvariant() + (name.Length > 1 ? name.Substring( 1 ) : "");

            if ( symbol.ContainingType.GetMembers( camelCasePropertyName ).Any() && firstPropertyLetter == firstPropertyLetter.ToLowerInvariant() )
            {
                // If there there is another property whose name differs only by the case of the first character, then the lower case variant will be suffixed.
                // This is unlikely given naming standards.

                camelCasePropertyName = FindUniqueName( camelCasePropertyName );
            }

            // TODO: Write tests of the collision resolution algorithm.
            if ( camelCasePropertyName.StartsWith( "_", StringComparison.Ordinal ) )
            {
                return camelCasePropertyName;
            }
            else
            {
                var fieldName = FindUniqueName( "_" + camelCasePropertyName );

                return fieldName;
            }

            string FindUniqueName( string hint )
            {
                if ( !symbol.ContainingType.GetMembers( hint ).Any() )
                {
                    return hint;
                }
                else
                {
                    for ( var i = 1; /* Nothing */; i++ )
                    {
                        var candidate = hint + i;

                        if ( !symbol.ContainingType.GetMembers( candidate ).Any() )
                        {
                            return candidate;
                        }
                    }
                }
            }
        }

        private static LinkerDeclarationFlags GetDeclarationFlags( ISymbol symbol )
        {
            // TODO: Partials?
            var declaration = symbol.GetPrimaryDeclaration();

            switch ( declaration )
            {
                case MemberDeclarationSyntax memberDeclaration:
                    return memberDeclaration.GetLinkerDeclarationFlags();

                case VariableDeclaratorSyntax variableDeclarator:
                    return ((MemberDeclarationSyntax?) variableDeclarator.Parent?.Parent).AssertNotNull().GetLinkerDeclarationFlags();

                case null:
                    return default;

                default:
                    throw new AssertionFailedException();
            }
        }

        private static bool IsExplicitInterfaceEventField( ISymbol symbol )
        {
            if ( symbol is IEventSymbol eventSymbol )
            {
                var declaration = eventSymbol.GetPrimaryDeclaration();

                if ( declaration != null && declaration.GetLinkerDeclarationFlags().HasFlag( LinkerDeclarationFlags.EventField ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}