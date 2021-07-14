﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// TODO: A lot methods here are called multiple times. Optimize.
// TODO: Split into a subclass for each declaration type?

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Provides methods for rewriting of types and members.
    /// </summary>
    internal partial class LinkerRewritingDriver
    {
        private readonly LinkerAnalysisRegistry _analysisRegistry;
        private readonly IReadOnlyList<Inliner> _inliners;

        public AspectReferenceResolver ReferenceResolver { get; }

        public Compilation IntermediateCompilation { get; }

        public LinkerRewritingDriver(
            Compilation intermediateCompilation,
            LinkerAnalysisRegistry analysisRegistry,
            AspectReferenceResolver referenceResolver,
            IReadOnlyList<Inliner> inliners )
        {
            this._analysisRegistry = analysisRegistry;
            this.IntermediateCompilation = intermediateCompilation;
            this._inliners = inliners;
            this.ReferenceResolver = referenceResolver;
        }

        public bool IsDiscarded( ISymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            return symbol switch
            {
                IMethodSymbol methodSymbol => this.IsDiscarded( methodSymbol, semantic ),
                IPropertySymbol propertySymbol => this.IsDiscarded( propertySymbol, semantic ),
                IEventSymbol eventSymbol => this.IsDiscarded( eventSymbol, semantic ),
                IFieldSymbol => false,
                _ => throw new AssertionFailedException()
            };
        }

        private bool IsInlineable( ISymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            return symbol switch
            {
                IMethodSymbol methodSymbol => this.IsInlineable( methodSymbol, semantic ),
                IPropertySymbol propertySymbol => this.IsInlineable( propertySymbol, semantic ),
                IEventSymbol eventSymbol => this.IsInlineable( eventSymbol, semantic ),
                IFieldSymbol => false,
                _ => throw new AssertionFailedException()
            };
        }

        private bool IsInlineableReference( ResolvedAspectReference aspectReference )
        {
            return
                aspectReference.Specification.Flags.HasFlag( AspectReferenceFlags.Inlineable )
                && this.GetInliner( aspectReference, out _ );
        }

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
                            GotoStatement(
                                SyntaxKind.GotoStatement,
                                Token( SyntaxKind.GotoKeyword ).WithTrailingTrivia( Space ),
                                default,
                                IdentifierName( inliningContext.ReturnLabelName.AssertNotNull() ),
                                Token( SyntaxKind.SemicolonToken ) ) )
                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
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
                                            ExpressionStatement(
                                                AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    IdentifierName( inliningContext.ReturnVariableName ?? "_" ),
                                                    returnStatement.Expression ) ),
                                            GotoStatement(
                                                SyntaxKind.GotoStatement,
                                                Token( SyntaxKind.GotoKeyword ).WithTrailingTrivia( Space ),
                                                default,
                                                IdentifierName( inliningContext.ReturnLabelName.AssertNotNull() ),
                                                Token( SyntaxKind.SemicolonToken ) ) )
                                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                            }
                            else
                            {
                                replacements[returnStatement] =
                                    GotoStatement(
                                        SyntaxKind.GotoStatement,
                                        Token( SyntaxKind.GotoKeyword ).WithTrailingTrivia( Space ),
                                        default,
                                        IdentifierName( inliningContext.ReturnLabelName.AssertNotNull() ),
                                        Token( SyntaxKind.SemicolonToken ) );
                            }
                        }
                    }
                    else if ( returnNode is ExpressionSyntax returnExpression )
                    {
                        if ( symbol.ReturnsVoid )
                        {
                            replacements[returnNode] =
                                Block(
                                        ExpressionStatement( returnExpression ),
                                        GotoStatement(
                                            SyntaxKind.GotoStatement,
                                            Token( SyntaxKind.GotoKeyword ).WithTrailingTrivia( Space ),
                                            default,
                                            IdentifierName( inliningContext.ReturnLabelName.AssertNotNull() ),
                                            Token( SyntaxKind.SemicolonToken ) ) )
                                    .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                        else
                        {
                            replacements[returnNode] =
                                Block(
                                        ExpressionStatement(
                                            AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                IdentifierName( inliningContext.ReturnVariableName ?? "_" ),
                                                returnExpression ) ),
                                        GotoStatement(
                                            SyntaxKind.GotoStatement,
                                            Token( SyntaxKind.GotoKeyword ).WithTrailingTrivia( Space ),
                                            default,
                                            IdentifierName( inliningContext.ReturnLabelName.AssertNotNull() ),
                                            Token( SyntaxKind.SemicolonToken ) ) )
                                    .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                        }
                    }
                    else
                    {
                        throw new AssertionFailedException();
                    }
                }
            }
        }

        private SyntaxNode GetBodyRootNode( IMethodSymbol symbol, out bool isImplicitlyLinked )
        {
            var declaration = symbol.GetPrimaryDeclaration();

            if ( this._analysisRegistry.IsOverrideTarget( symbol )
                 || (symbol.AssociatedSymbol != null && this._analysisRegistry.IsOverrideTarget( symbol.AssociatedSymbol )) )
            {
                // Override targets are implicitly linked, i.e. no replacement of aspect references is necessary.
                isImplicitlyLinked = true;

                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body ?? methodDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case AccessorDeclarationSyntax accessorDecl:
                        var body = (SyntaxNode?) accessorDecl.Body ?? accessorDecl.ExpressionBody;

                        if ( body != null )
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
            else if ( this._analysisRegistry.IsOverride( symbol )
                      || (symbol.AssociatedSymbol != null && this._analysisRegistry.IsOverride( symbol.AssociatedSymbol )) )
            {
                isImplicitlyLinked = false;

                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body ?? throw new AssertionFailedException();

                    case AccessorDeclarationSyntax accessorDecl:
                        return (SyntaxNode?) accessorDecl.Body ?? throw new AssertionFailedException();

                    default:
                        throw new AssertionFailedException();
                }
            }
            else
            {
                throw new AssertionFailedException();
            }
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
                    if ( symbol.ReturnsVoid )
                    {
                        return
                            Block( ExpressionStatement( (ExpressionSyntax) rewriter.Visit( arrowExpressionClause.Expression ).AssertNotNull() ) );
                    }
                    else
                    {
                        return
                            Block(
                                ReturnStatement(
                                    Token( SyntaxKind.ReturnKeyword ).WithLeadingTrivia( Whitespace( " " ) ),
                                    (ExpressionSyntax) rewriter.Visit( arrowExpressionClause.Expression ).AssertNotNull(),
                                    Token( SyntaxKind.SemicolonToken ) ) );
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
            return this._analysisRegistry.IsOverride( symbol ) || this._analysisRegistry.IsOverrideTarget( symbol );
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
        /// for the first override is the original delcaration.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private IMethodSymbol GetBodySource( IMethodSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                return symbol;
            }
            else if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                return (IMethodSymbol) this._analysisRegistry.GetLastOverride( symbol ).AssertNotNull();
            }
            else
            {
                throw new AssertionFailedException();
            }
        }

        private ExpressionSyntax GetLinkedExpression( ResolvedAspectReference aspectReference )
        {
            if ( !SymbolEqualityComparer.Default.Equals( aspectReference.ResolvedSymbol.ContainingType, aspectReference.ResolvedSymbol.ContainingType ) )
            {
                throw new AssertionFailedException();
            }

            var targetMemberName =
                (this._analysisRegistry.IsOverrideTarget( aspectReference.ResolvedSymbol ), aspectReference.Specification.Order) switch
                {
                    (true, AspectReferenceOrder.Base) => GetOriginalImplMemberName( aspectReference.ResolvedSymbol.Name ),
                    (true, AspectReferenceOrder.Original) => GetOriginalImplMemberName( aspectReference.ResolvedSymbol.Name ),
                    _ => aspectReference.ResolvedSymbol.Name
                };

            if ( aspectReference.ResolvedSymbol.IsStatic )
            {
                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( aspectReference.ResolvedSymbol.ContainingType ),
                    IdentifierName( targetMemberName ) );
            }
            else
            {
                if ( SymbolEqualityComparer.Default.Equals( aspectReference.ContainingSymbol.ContainingType, aspectReference.ResolvedSymbol.ContainingType ) )
                {
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( targetMemberName ) );
                }
                else
                {
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        BaseExpression(),
                        IdentifierName( targetMemberName ) );
                }
            }
        }

        internal static string GetOriginalImplMemberName( string memberName ) => $"__{memberName}__OriginalImpl";

        private static LinkerDeclarationFlags GetDeclarationFlags( ISymbol symbol )
        {
            // TODO: Partials?
            return symbol.DeclaringSyntaxReferences
                .Select( dsr => (dsr.GetSyntax() as MemberDeclarationSyntax)?.GetLinkerDeclarationFlags() ?? LinkerDeclarationFlags.None )
                .SingleOrDefault();
        }
    }
}