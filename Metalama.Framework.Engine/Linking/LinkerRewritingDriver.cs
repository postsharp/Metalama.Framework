// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// TODO: A lot methods here are called multiple times. Optimize.
// TODO: Split into a subclass for each declaration type?

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Provides methods for rewriting of types and members.
    /// </summary>
    internal sealed partial class LinkerRewritingDriver
    {
        private ProjectServiceProvider ServiceProvider { get; }

        private LinkerInjectionRegistry InjectionRegistry { get; }

        private CompilationContext IntermediateCompilationContext { get; }

        private LinkerAnalysisRegistry AnalysisRegistry { get; }

        public LinkerRewritingDriver(
            ProjectServiceProvider serviceProvider,
            CompilationContext intermediateCompilationContext,
            LinkerInjectionRegistry injectionRegistry,
            LinkerAnalysisRegistry analysisRegistry )
        {
            this.ServiceProvider = serviceProvider;
            this.InjectionRegistry = injectionRegistry;
            this.AnalysisRegistry = analysisRegistry;
            this.IntermediateCompilationContext = intermediateCompilationContext;
        }

        /// <summary>
        /// Assembles a linked body of the method/accessor, where aspect reference annotations are replaced by target symbols and inlineable references are inlined.
        /// </summary>
        /// <param name="semantic">Method or accessor symbol.</param>
        /// <param name="substitutionContext">Substitution context.</param>
        /// <returns>Block representing the linked body.</returns>
        public BlockSyntax GetSubstitutedBody( IntermediateSymbolSemantic<IMethodSymbol> semantic, SubstitutionContext substitutionContext )
        {
            var triviaSource = this.ResolveBodyBlockTriviaSource( semantic, out var shouldRemoveExistingTrivia );
            var bodyRootNode = this.GetBodyRootNode( semantic.Symbol, substitutionContext.SyntaxGenerationContext );
            var rewrittenBody = RewriteBody( bodyRootNode, semantic.Symbol, substitutionContext );
            var rewrittenBlock = TransformToBlock( rewrittenBody, semantic.Symbol );

            // Add the SourceCode annotation, if it is the source code.
            if ( semantic.Kind == IntermediateSymbolSemanticKind.Default && this.InjectionRegistry.IsOverrideTarget( semantic.Symbol ) )
            {
                rewrittenBlock = rewrittenBlock.WithSourceCodeAnnotation();
            }

            if ( triviaSource == null )
            {
                // Strip the trivia from the block and add a flattenable annotation.
                return rewrittenBlock.PartialUpdate(
                        openBraceToken: Token( new( ElasticMarker ), SyntaxKind.OpenBraceToken, new( ElasticMarker ) ),
                        closeBraceToken: Token( new( ElasticMarker ), SyntaxKind.CloseBraceToken, new( ElasticMarker ) ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
            else
            {
                var (openBraceLeadingTrivia, openBraceTrailingTrivia, closeBraceLeadingTrivia, closeBraceTrailingTrivia) =
                    triviaSource switch
                    {
                        BlockSyntax blockSyntax => (
                            blockSyntax.OpenBraceToken.LeadingTrivia,
                            blockSyntax.OpenBraceToken.TrailingTrivia,
                            blockSyntax.CloseBraceToken.LeadingTrivia,
                            blockSyntax.CloseBraceToken.TrailingTrivia
                        ),
                        ArrowExpressionClauseSyntax arrowExpression => (
                            arrowExpression.ArrowToken.LeadingTrivia,
                            arrowExpression.ArrowToken.TrailingTrivia,
                            TriviaList(),
                            TriviaList()
                        ),
                        _ => throw new AssertionFailedException( Justifications.CoverageMissing )
                    };

                if ( shouldRemoveExistingTrivia )
                {
                    rewrittenBlock =
                        rewrittenBlock
                            .PartialUpdate( 
                                openBraceToken: rewrittenBlock.OpenBraceToken.WithoutTrivia(),
                                closeBraceToken: rewrittenBlock.CloseBraceToken.WithoutTrivia() )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                }
                else
                {
                    rewrittenBlock =
                        rewrittenBlock
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                }

                // Keep all trivia from the source block and add trivias from the root block.
                return Block( rewrittenBlock )
                    .PartialUpdate(
                        openBraceToken: Token(
                            openBraceLeadingTrivia.Add( ElasticMarker ),
                            SyntaxKind.OpenBraceToken,
                            openBraceTrailingTrivia.Insert( 0, ElasticMarker ) ),
                        closeBraceToken: Token(
                            closeBraceLeadingTrivia.Add( ElasticMarker ),
                            SyntaxKind.CloseBraceToken,
                            closeBraceTrailingTrivia.Insert( 0, ElasticMarker ) ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }

            static BlockSyntax TransformToBlock( SyntaxNode node, IMethodSymbol symbol )
            {
                // TODO: Convert to block.
                if ( symbol.ReturnsVoid )
                {
                    switch ( node )
                    {
                        case null:
                            throw new AssertionFailedException( Justifications.CoverageMissing );

                        // return
                        //     SyntaxFactoryEx.FormattedBlock()
                        //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                        case BlockSyntax rewrittenBlock:
                            return rewrittenBlock;

                        case ArrowExpressionClauseSyntax rewrittenArrowClause:
                            return
                                Block( ExpressionStatement( rewrittenArrowClause.Expression ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                        default:
                            throw new AssertionFailedException( $"{node.Kind()} is not an expected output of the body substitution." );
                    }
                }
                else
                {
                    switch ( node )
                    {
                        case null:
                            throw new AssertionFailedException( Justifications.CoverageMissing );

                        // return
                        //     Block(
                        //             ReturnStatement(
                        //                 Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        //                 LiteralExpression( SyntaxKind.DefaultLiteralExpression ),
                        //                 Token( SyntaxKind.SemicolonToken ) ) )
                        //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                        case ArrowExpressionClauseSyntax rewrittenArrowClause:
                            return
                                SyntaxFactoryEx.FormattedBlock(
                                        ReturnStatement(
                                            SyntaxFactoryEx.TokenWithSpace( SyntaxKind.ReturnKeyword ),
                                            rewrittenArrowClause.Expression,
                                            Token( SyntaxKind.SemicolonToken ) ) )
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                        case BlockSyntax rewrittenBlock:
                            return rewrittenBlock;

                        default:
                            throw new AssertionFailedException( $"{node.Kind()} is not an expected output of the body substitution." );
                    }
                }
            }
        }

        /// <summary>
        /// Gets a node that is going to be starting point of substitutions.
        /// </summary>
        private SyntaxNode GetBodyRootNode( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
        {
            var declaration = symbol.GetPrimaryDeclaration();

            if ( this.InjectionRegistry.IsOverrideTarget( symbol ) )
            {
                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        // Partial methods without declared body have the whole declaration as body.
                        return methodDecl.Body ?? (SyntaxNode?) methodDecl.ExpressionBody ?? methodDecl;

                    case DestructorDeclarationSyntax destructorDecl:
                        return (SyntaxNode?) destructorDecl.Body
                               ?? destructorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case ConstructorDeclarationSyntax constructorDecl:
                        return (SyntaxNode?) constructorDecl.Body
                               ?? constructorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case OperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body
                               ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case ConversionOperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body
                               ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case AccessorDeclarationSyntax accessorDecl:
                        // Accessors with no body are auto-properties, in which case we have substitution for the whole accessor declaration.
                        Invariant.Assert( !symbol.IsAbstract );

                        return accessorDecl.Body ?? (SyntaxNode?) accessorDecl.ExpressionBody ?? accessorDecl;

                    case ArrowExpressionClauseSyntax arrowExpressionClause:
                        // Expression-bodied property.
                        return arrowExpressionClause;

                    case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: EventFieldDeclarationSyntax } } variableDecl:
                        // Event field accessors start replacement as variableDecls.
                        return variableDecl;

                    case ParameterSyntax { Parent.Parent: RecordDeclarationSyntax } positionalProperty:
                        // Record positional property.
                        return positionalProperty;

                    default:
                        throw new AssertionFailedException( $"Unexpected override target symbol: '{symbol}'." );
                }
            }

            if ( this.InjectionRegistry.IsOverride( symbol ) )
            {
                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body
                               ?? methodDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case AccessorDeclarationSyntax accessorDecl:
                        return (SyntaxNode?) accessorDecl.Body
                               ?? accessorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case DestructorDeclarationSyntax destructorDecl:
                        return (SyntaxNode?) destructorDecl.Body
                               ?? destructorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    case ConstructorDeclarationSyntax constructorDecl:
                        return (SyntaxNode?) constructorDecl.Body
                               ?? constructorDecl.ExpressionBody ?? throw new AssertionFailedException( $"'{symbol}' has no implementation." );

                    default:
                        throw new AssertionFailedException( $"Unexpected override symbol: '{symbol}'." );
                }
            }

            if ( symbol.AssociatedSymbol != null && symbol.AssociatedSymbol.IsExplicitInterfaceEventField() )
            {
                return GetImplicitAccessorBody( symbol, generationContext );
            }

            if ( this.AnalysisRegistry.HasAnySubstitutions( symbol ) )
            {
                switch ( declaration )
                {
                    case ConstructorDeclarationSyntax constructorDecl:
                        return (SyntaxNode?) constructorDecl.Body
                               ?? constructorDecl.ExpressionBody
                               ?? throw new AssertionFailedException( "Constructor is expected to have body or expression body." );

                    case DestructorDeclarationSyntax destructorDecl:
                        return (SyntaxNode?) destructorDecl.Body
                               ?? destructorDecl.ExpressionBody
                               ?? throw new AssertionFailedException( "Destructor is expected to have body or expression body." );

                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body
                               ?? methodDecl.ExpressionBody
                               ?? throw new AssertionFailedException( "Method is expected to have body or expression body." );

                    case ConversionOperatorDeclarationSyntax conversionOperatorDecl:
                        return (SyntaxNode?) conversionOperatorDecl.Body
                               ?? conversionOperatorDecl.ExpressionBody
                               ?? throw new AssertionFailedException( "ConversionOperator is expected to have body or expression body." );

                    case OperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body
                               ?? operatorDecl.ExpressionBody
                               ?? throw new AssertionFailedException( "Operator is expected to have body or expression body." );

                    case AccessorDeclarationSyntax accessorDecl:
                        return (SyntaxNode?) accessorDecl.Body
                               ?? accessorDecl.ExpressionBody
                               ?? throw new AssertionFailedException( "Operator is expected to have body or expression body." );

                    default:
                        throw new AssertionFailedException( $"Unexpected redirection: '{symbol}'." );
                }
            }

            throw new AssertionFailedException( $"Don't know how to process '{symbol}'." );
        }

        private static BlockSyntax GetImplicitAccessorBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
        {
            switch ( symbol )
            {
                case { MethodKind: MethodKind.PropertyGet, Parameters.Length: > 0 }:
                    return GetImplicitIndexerGetterBody( symbol, generationContext );

                case { MethodKind: MethodKind.PropertySet, Parameters.Length: > 0 }:
                    return GetImplicitIndexerSetterBody( symbol, generationContext );

                case { MethodKind: MethodKind.PropertyGet }:
                    return GetImplicitGetterBody( symbol, generationContext );

                case { MethodKind: MethodKind.PropertySet }:
                    return GetImplicitSetterBody( symbol, generationContext );

                case { MethodKind: MethodKind.EventAdd }:
                    return GetImplicitAdderBody( symbol, generationContext );

                case { MethodKind: MethodKind.EventRemove }:
                    return GetImplicitRemoverBody( symbol, generationContext );

                default:
                    throw new AssertionFailedException( $"Don't know how to process '{symbol}'." );
            }
        }

        private static SyntaxNode RewriteBody( SyntaxNode bodyRootNode, IMethodSymbol symbol, SubstitutionContext context )
        {
            var rewriter = new SubstitutingRewriter( context );

            switch ( bodyRootNode )
            {
                case BlockSyntax block:
                    return (BlockSyntax) rewriter.Visit( block ).AssertNotNull();

                case AccessorDeclarationSyntax accessorDecl:
                    return (BlockSyntax) rewriter.Visit( accessorDecl ).AssertNotNull();

                case MethodDeclarationSyntax partialMethodDeclaration:
                    return (BlockSyntax) rewriter.Visit( partialMethodDeclaration ).AssertNotNull();

                case VariableDeclaratorSyntax { Parent.Parent: EventFieldDeclarationSyntax } eventFieldVariable:
                    return (BlockSyntax) rewriter.Visit( eventFieldVariable ).AssertNotNull();

                case ParameterSyntax { Parent.Parent: RecordDeclarationSyntax } positionalProperty:
                    return (BlockSyntax) rewriter.Visit( positionalProperty ).AssertNotNull();

                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    var rewrittenNode = rewriter.Visit( arrowExpressionClause );

                    // TODO: This may be useless.
                    if ( symbol.ReturnsVoid )
                    {
                        switch ( rewrittenNode )
                        {
                            case null:
                                throw new AssertionFailedException( Justifications.CoverageMissing );

                            // return
                            //     SyntaxFactoryEx.FormattedBlock()
                            //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case BlockSyntax rewrittenBlock:
                                return rewrittenBlock;

                            case ArrowExpressionClauseSyntax rewrittenArrowClause:
                                return rewrittenArrowClause;

                            default:
                                throw new AssertionFailedException( $"{rewrittenNode.Kind()} is not an expected output of the body substitution." );
                        }
                    }
                    else
                    {
                        switch ( rewrittenNode )
                        {
                            case null:
                                throw new AssertionFailedException( Justifications.CoverageMissing );

                            // return
                            //     Block(
                            //             ReturnStatement(
                            //                 Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                            //                 LiteralExpression( SyntaxKind.DefaultLiteralExpression ),
                            //                 Token( SyntaxKind.SemicolonToken ) ) )
                            //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case ArrowExpressionClauseSyntax rewrittenArrowClause:
                                return rewrittenArrowClause;

                            case BlockSyntax rewrittenBlock:
                                return rewrittenBlock;

                            default:
                                throw new AssertionFailedException( $"{rewrittenNode.Kind()} is not an expected output of the body substitution." );
                        }
                    }

                default:
                    throw new AssertionFailedException( $"{bodyRootNode.Kind()} is not an expected kind of body root node." );
            }
        }

        public IReadOnlyDictionary<SyntaxNode, SyntaxNodeSubstitution>? GetSubstitutions( InliningContextIdentifier inliningContextId )
        {
            return this.AnalysisRegistry.GetSubstitutions( inliningContextId );
        }

        /// <summary>
        /// Determines whether the symbol should be rewritten.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool IsRewriteTarget( ISymbol symbol )
        {
            if ( this.InjectionRegistry.IsOverrideTarget( symbol ) )
            {
                // Override targets need to be rewritten.
                return true;
            }

            if ( this.InjectionRegistry.IsOverride( symbol ) )
            {
                // Overrides need to be rewritten.
                return true;
            }

            if ( this.AnalysisRegistry.HasAnySubstitutions( symbol ) )
            {
                // Any declarations with substitutions need to be rewritten.
                return true;
            }

            if ( this.InjectionRegistry.IsIntroduced( symbol ) )
            {
                // Introduced declarations need to be rewritten.
                return true;
            }

            if ( this.AnalysisRegistry.HasBaseSemanticReferences( symbol ) )
            {
                // Override member with no aspect override that has it's base semantic referenced. 
                return true;
            }

            if ( symbol.IsExplicitInterfaceEventField() )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets rewritten member and any additional induced members (e.g. backing field of auto property).
        /// </summary>
        public IReadOnlyList<MemberDeclarationSyntax> RewriteMember( MemberDeclarationSyntax syntax, ISymbol symbol, SyntaxGenerationContext generationContext )
        {
            if (this.AnalysisRegistry.HasAnyUnsupportedOverride(symbol))
            {
                // If there is unsupported code in overrides, we will not rewrite the member.
                return new[] { syntax };
            }

            if (this.InjectionRegistry.IsOverride(symbol) && this.AnalysisRegistry.HasAnyUnsupportedOverride( this.InjectionRegistry.GetOverrideTarget(symbol ).AssertNotNull() ) )
            {
                // If there are any overrides with unsupported code, we will skip this member.
                return Array.Empty<MemberDeclarationSyntax>();
            }

            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation } methodSymbol:
                    return this.RewriteMethod( (MethodDeclarationSyntax) syntax, methodSymbol, generationContext );

                case IMethodSymbol { MethodKind: MethodKind.Destructor } destructorSymbol:
                    return this.RewriteDestructor( (DestructorDeclarationSyntax) syntax, destructorSymbol, generationContext );

                case IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } constructorSymbol:
                    return this.RewriteConstructor( (ConstructorDeclarationSyntax) syntax, constructorSymbol, generationContext );

                case IMethodSymbol { MethodKind: MethodKind.Conversion } operatorSymbol:
                    return this.RewriteConversionOperator( (ConversionOperatorDeclarationSyntax) syntax, operatorSymbol, generationContext );

                case IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator } operatorSymbol:
                    return this.RewriteOperator( (OperatorDeclarationSyntax) syntax, operatorSymbol, generationContext );

                case IPropertySymbol { Parameters.Length: 0 } propertySymbol:
                    return this.RewriteProperty( (PropertyDeclarationSyntax) syntax, propertySymbol, generationContext );

                case IPropertySymbol indexerSymbol:
                    return this.RewriteIndexer( (IndexerDeclarationSyntax) syntax, indexerSymbol, generationContext );

                case IFieldSymbol fieldSymbol:
                    return this.RewriteField( (FieldDeclarationSyntax) syntax, fieldSymbol );

                case IEventSymbol eventSymbol:
                    return syntax switch
                    {
                        EventDeclarationSyntax eventSyntax => this.RewriteEvent( eventSyntax, eventSymbol ),
                        EventFieldDeclarationSyntax eventFieldSyntax => this.RewriteEventField( eventFieldSyntax, eventSymbol ),
                        _ => throw new InvalidOperationException( $"Unsupported event syntax: {syntax.Kind()}" )
                    };

                default:
                    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                    throw new AssertionFailedException( $"Unsupported symbol kind: {symbol?.Kind.ToString() ?? "(null)"}" );
            }
        }

        /// <summary>
        /// Gets a syntax node that will the the source of trivia of the specified declaration root block.
        /// </summary>
        private SyntaxNode? ResolveBodyBlockTriviaSource( IntermediateSymbolSemantic<IMethodSymbol> semantic, out bool shouldRemoveExistingTrivia )
        {
            ISymbol? symbol;

            if ( this.InjectionRegistry.IsOverride( semantic.Symbol ) )
            {
                Invariant.Assert( semantic.Kind == IntermediateSymbolSemanticKind.Default );

                symbol = semantic.Symbol;
                shouldRemoveExistingTrivia = true;
            }
            else if ( this.InjectionRegistry.IsOverrideTarget( semantic.Symbol ) )
            {
                symbol = null;

                switch ( semantic.Kind )
                {
                    case IntermediateSymbolSemanticKind.Base:
                    case IntermediateSymbolSemanticKind.Default:
                        shouldRemoveExistingTrivia = false;

                        break;

                    case IntermediateSymbolSemanticKind.Final:
                        shouldRemoveExistingTrivia = true;

                        break;

                    default:
                        throw new AssertionFailedException( $"Unsupported symbol kind: {symbol?.Kind.ToString() ?? "(null)"}" );
                }
            }
            else if ( this.InjectionRegistry.IsIntroduced( semantic.Symbol ) )
            {
                // Introduced, but not override target.

                symbol = semantic.Symbol;
                shouldRemoveExistingTrivia = false;
            }
            else if ( semantic.Symbol.AssociatedSymbol != null && semantic.Symbol.AssociatedSymbol.IsExplicitInterfaceEventField() )
            {
                symbol = semantic.Symbol;
                shouldRemoveExistingTrivia = true;
            }
            else if ( this.AnalysisRegistry.HasAnySubstitutions( semantic.Symbol ) )
            {
                symbol = semantic.Symbol;
                shouldRemoveExistingTrivia = false;
            }
            else
            {
                throw new AssertionFailedException( $"{semantic} is not expected for trivia source resolution." );
            }

            return symbol?.GetPrimaryDeclaration() switch
            {
                null => null,
                MethodDeclarationSyntax methodDeclaration => (SyntaxNode?) methodDeclaration.Body ?? methodDeclaration.ExpressionBody,
                AccessorDeclarationSyntax accessorDeclaration => (SyntaxNode?) accessorDeclaration.Body ?? accessorDeclaration.ExpressionBody,
                ConstructorDeclarationSyntax constructorDeclaration => (SyntaxNode?) constructorDeclaration.Body ?? constructorDeclaration.ExpressionBody,
                DestructorDeclarationSyntax destructorDeclaration => (SyntaxNode?) destructorDeclaration.Body ?? destructorDeclaration.ExpressionBody,
                ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => (SyntaxNode?) conversionOperatorDeclaration.Body
                                                                                     ?? conversionOperatorDeclaration.ExpressionBody,
                OperatorDeclarationSyntax operatorDeclaration => (SyntaxNode?) operatorDeclaration.Body ?? operatorDeclaration.ExpressionBody,
                ArrowExpressionClauseSyntax arrowExpression => arrowExpression,
                var declaration => throw new AssertionFailedException( $"{declaration.Kind()} is not expected primary declaration." )
            };
        }

        private bool ShouldGenerateEmptyMember( ISymbol symbol )
        {
            return this.InjectionRegistry.IsIntroduced( symbol ) && !symbol.IsOverride
                                                                 && !symbol.TryGetHiddenSymbol( this.IntermediateCompilationContext.Compilation, out _ );
        }

        private bool ShouldGenerateSourceMember( ISymbol symbol )
        {
            return this.InjectionRegistry.IsOverrideTarget( symbol );
        }

        public static string GetOriginalImplMemberName( ISymbol symbol ) => GetSpecialMemberName( symbol, "Source" );

        public static string GetEmptyImplMemberName( ISymbol symbol ) => GetSpecialMemberName( symbol, "Empty" );

        private static TypeSyntax GetOriginalImplParameterType()
        {
            return
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            AliasQualifiedName(
                                IdentifierName( Token( SyntaxKind.GlobalKeyword ) ),
                                IdentifierName( "Metalama" ) ),
                            IdentifierName( "Framework" ) ),
                        IdentifierName( "RunTime" ) ),
                    IdentifierName( "Source" ) );
        }

        private static TypeSyntax GetEmptyImplParameterType()
        {
            return
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            AliasQualifiedName(
                                IdentifierName( Token( SyntaxKind.GlobalKeyword ) ),
                                IdentifierName( "Metalama" ) ),
                            IdentifierName( "Framework" ) ),
                        IdentifierName( "RunTime" ) ),
                    IdentifierName( "Empty" ) );
        }

        private static string GetSpecialMemberName( ISymbol symbol, string suffix )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    if ( methodSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( symbol, GetInterfaceMemberName( methodSymbol.ExplicitInterfaceImplementations[0] ), suffix );
                    }
                    else
                    {
                        return CreateName( symbol, methodSymbol.Name, suffix );
                    }

                case IPropertySymbol propertySymbol:
                    if ( propertySymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( symbol, GetInterfaceMemberName( propertySymbol.ExplicitInterfaceImplementations[0] ), suffix );
                    }
                    else
                    {
                        return CreateName( symbol, propertySymbol.Name, suffix );
                    }

                case IEventSymbol eventSymbol:
                    if ( eventSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( symbol, GetInterfaceMemberName( eventSymbol.ExplicitInterfaceImplementations[0] ), suffix );
                    }
                    else
                    {
                        return CreateName( symbol, eventSymbol.Name, suffix );
                    }

                case IFieldSymbol fieldSymbol:
                    return CreateName( symbol, fieldSymbol.Name, suffix );

                default:
                    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                    throw new AssertionFailedException( $"Unsupported symbol kind: {symbol?.Kind.ToString() ?? "(null)"}" );
            }

            static string CreateName( ISymbol symbol, string name, string suffix )
            {
                var hint = $"{name}_{suffix}";

                for ( var i = 2; symbol.ContainingType.GetMembers( hint ).Any(); i++ )
                {
                    hint = $"{name}_{suffix}{i}";
                }

                return hint;
            }

            static string GetInterfaceMemberName( ISymbol interfaceMember )
            {
                var interfaceType = interfaceMember.ContainingType;

                return $"{interfaceType.GetFullName().AssertNotNull().ReplaceOrdinal( ".", "_" ).ReplaceOrdinal( "`", "__" )}_{interfaceMember.Name}";
            }
        }

        public static string GetBackingFieldName( ISymbol symbol )
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
                    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                    throw new AssertionFailedException( $"Unsupported symbol kind: {symbol?.Kind.ToString() ?? "(null)"}" );
            }

            var firstPropertyLetter = name.Substring( 0, 1 );
            var camelCasePropertyName = name.ToCamelCase();

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

        private static SyntaxList<AttributeListSyntax> FilterAttributeListsForTarget(
            SyntaxList<AttributeListSyntax> attributeLists,
            SyntaxKind targetKind,
            bool includeEmptyTarget,
            bool preserveTarget )
        {
            if ( preserveTarget )
            {
                return List( attributeLists.Where( Filter ).ToReadOnlyList() );
            }
            else
            {
                return List( attributeLists.Where( Filter ).Select( al => al.WithTarget( null ) ).ToReadOnlyList() );
            }

            bool Filter( AttributeListSyntax list )
            {
                if ( list.Target == null && includeEmptyTarget )
                {
                    return true;
                }
                else
                {
                    return list.Target?.Identifier.IsKind( targetKind ) == true;
                }
            }
        }

        private T FilterAttributesOnSpecialImpl<T>( ImmutableArray<IParameterSymbol> parameterSymbols, T parameters )
            where T : BaseParameterListSyntax
        {
            var transformed = new List<ParameterSyntax>();

            for ( var i = 0; i < parameters.Parameters.Count; i++ )
            {
                if ( i < parameterSymbols.Length )
                {
                    transformed.Add( parameters.Parameters[i].WithAttributeLists( this.FilterAttributesOnSpecialImpl( parameterSymbols[i] ) ) );
                }
                else
                {
                    // This is only used in indexer linking, before an error is produced.
                    transformed.Add( parameters.Parameters[i] );
                }
            }

            return (T) parameters.WithParameters( SeparatedList( transformed ) );
        }

        private TypeParameterListSyntax FilterAttributesOnSpecialImpl(
            ImmutableArray<ITypeParameterSymbol> typeParameterSymbols,
            TypeParameterListSyntax typeParameters )
        {
            if ( typeParameterSymbols.Length != typeParameters.Parameters.Count )
            {
                // This would mean that linker added a type parameter.
                throw new AssertionFailedException(
                    $"Type parameter count doesn't match ({typeParameterSymbols.Length} != {typeParameters.Parameters.Count})." );
            }

            var transformed = new List<TypeParameterSyntax>();

            for ( var i = 0; i < typeParameterSymbols.Length; i++ )
            {
                transformed.Add( typeParameters.Parameters[i].WithAttributeLists( this.FilterAttributesOnSpecialImpl( typeParameterSymbols[i] ) ) );
            }

            return typeParameters.WithParameters( SeparatedList( transformed ) );
        }

        private AccessorDeclarationSyntax FilterAttributesOnSpecialImpl( IMethodSymbol originalAccessor, AccessorDeclarationSyntax accessorSyntax )
        {
            return accessorSyntax.WithAttributeLists( this.FilterAttributesOnSpecialImpl( originalAccessor ) );
        }

        private SyntaxList<AttributeListSyntax> FilterAttributesOnSpecialImpl( ISymbol symbol )
        {
            var classificationService = this.ServiceProvider.Global.GetRequiredService<AttributeClassificationService>();

            var filteredAttributeLists = new List<AttributeListSyntax>();

            foreach ( var attribute in symbol.GetAttributes() )
            {
                if ( attribute.AttributeClass != null && classificationService.IsCompilerRecognizedAttribute( attribute.AttributeClass ) )
                {
                    filteredAttributeLists.Add(
                        AttributeList( SingletonSeparatedList( (AttributeSyntax) attribute.ApplicationSyntaxReference.AssertNotNull().GetSyntax() ) ) );
                }
            }

            return List( filteredAttributeLists );
        }
    }
}