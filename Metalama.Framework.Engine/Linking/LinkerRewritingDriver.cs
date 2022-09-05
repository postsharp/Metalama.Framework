// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// TODO: A lot methods here are called multiple times. Optimize.
// TODO: Split into a subclass for each declaration type?

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Provides methods for rewriting of types and members.
    /// </summary>
    internal partial class LinkerRewritingDriver
    {
        public LinkerIntroductionRegistry IntroductionRegistry { get; }

        public UserDiagnosticSink DiagnosticSink { get; }

        public Compilation IntermediateCompilation { get; }

        internal LinkerAnalysisRegistry AnalysisRegistry { get; }

        public IServiceProvider ServiceProvider { get; }

        public LinkerRewritingDriver(
            Compilation intermediateCompilation,
            LinkerIntroductionRegistry introductionRegistry,
            LinkerAnalysisRegistry analysisRegistry,
            UserDiagnosticSink diagnosticSink,
            IServiceProvider serviceProvider )
        {
            this.IntroductionRegistry = introductionRegistry;
            this.AnalysisRegistry = analysisRegistry;
            this.IntermediateCompilation = intermediateCompilation;
            this.DiagnosticSink = diagnosticSink;
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Assembles a linked body of the method/accessor, where aspect reference annotations are replaced by target symbols and inlineable references are inlined.
        /// </summary>
        /// <param name="semantic">Method or accessor symbol.</param>
        /// <param name="inliningContext"></param>
        /// <returns>Block representing the linked body.</returns>
        public BlockSyntax GetSubstitutedBody( IntermediateSymbolSemantic<IMethodSymbol> semantic, SubstitutionContext substitutionContext )
        {
            var triviaSource = this.ResolveBodyBlockTriviaSource( semantic, out var shouldRemoveExistingTrivia );
            var bodyRootNode = this.GetBodyRootNode( semantic.Symbol, substitutionContext.SyntaxGenerationContext );
            var rewrittenBody = this.RewriteBody( bodyRootNode, semantic.Symbol, substitutionContext );

            // Add the SourceCode annotation, if it is source code.
            if ( !(semantic.Symbol.GetPrimarySyntaxReference() is { } primarySyntax
                   && primarySyntax.GetSyntax().HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind )) )
            {
                rewrittenBody = rewrittenBody.WithSourceCodeAnnotation();
            }

            if ( triviaSource == null )
            {
                // Strip the trivia from the block.
                return rewrittenBody
                    .WithOpenBraceToken(
                        Token( SyntaxKind.OpenBraceToken )
                            .WithLeadingTrivia( ElasticMarker )
                            .WithTrailingTrivia( ElasticMarker ) )
                    .WithCloseBraceToken(
                        Token( SyntaxKind.CloseBraceToken )
                            .WithLeadingTrivia( ElasticMarker )
                            .WithTrailingTrivia( ElasticMarker ) )
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
                        _ => throw new AssertionFailedException( Justifications.CoverageMissing )
                    };

                if ( shouldRemoveExistingTrivia )
                {
                    rewrittenBody =
                        rewrittenBody
                            .WithOpenBraceToken( rewrittenBody.OpenBraceToken.WithoutTrivia() )
                            .WithCloseBraceToken( rewrittenBody.CloseBraceToken.WithoutTrivia() )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                }
                else
                {
                    rewrittenBody =
                        rewrittenBody
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                }

                // Keep all trivia from the source block and add trivias from the root block.
                return Block( rewrittenBody )
                    .WithOpenBraceToken(
                        Token( SyntaxKind.OpenBraceToken )
                            .WithLeadingTrivia( openBraceLeadingTrivia.Add( ElasticMarker ) )
                            .WithTrailingTrivia( openBraceTrailingTrivia.Insert( 0, ElasticMarker ) ) )
                    .WithCloseBraceToken(
                        Token( SyntaxKind.CloseBraceToken )
                            .WithLeadingTrivia( closeBraceLeadingTrivia.Add( ElasticMarker ) )
                            .WithTrailingTrivia( closeBraceTrailingTrivia.Insert( 0, ElasticMarker ) ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
        }

        /// <summary>
        /// Gets a node that is going to be starting point of substitutions.
        /// </summary>
        private SyntaxNode GetBodyRootNode( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
        {
            var declaration = symbol.GetPrimaryDeclaration();

            if ( this.IntroductionRegistry.IsOverrideTarget( symbol ) )
            {
                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        // Partial methods without declared body have empty implicit body.
                        return methodDecl.Body ?? (SyntaxNode?) methodDecl.ExpressionBody ?? Block();

                    case DestructorDeclarationSyntax destructorDecl:
                        return (SyntaxNode?) destructorDecl.Body ?? destructorDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case OperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case ConversionOperatorDeclarationSyntax operatorDecl:
                        return (SyntaxNode?) operatorDecl.Body ?? operatorDecl.ExpressionBody ?? throw new AssertionFailedException();

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
                    case ParameterSyntax: 
                        // Record positional property.
                        return GetImplicitAccessorBody( symbol, generationContext );

                    default:
                        throw new AssertionFailedException();
                }
            }

            if ( this.IntroductionRegistry.IsOverride( symbol ) )
            {
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

            if ( symbol.AssociatedSymbol != null && symbol.AssociatedSymbol.IsExplicitInterfaceEventField() )
            {
                return GetImplicitAccessorBody( symbol, generationContext );
            }

            throw new AssertionFailedException();
        }

        private static BlockSyntax GetImplicitAccessorBody( IMethodSymbol symbol, SyntaxGenerationContext generationContext )
        {
            switch ( symbol )
            {
                case { MethodKind: MethodKind.PropertyGet }:
                    return GetImplicitGetterBody( symbol, generationContext );

                case { MethodKind: MethodKind.PropertySet }:
                    return GetImplicitSetterBody( symbol, generationContext );

                case { MethodKind: MethodKind.EventAdd }:
                    return GetImplicitAdderBody( symbol, generationContext );

                case { MethodKind: MethodKind.EventRemove }:
                    return GetImplicitRemoverBody( symbol, generationContext );

                default:
                    throw new InvalidOperationException();
            }
        }

#pragma warning disable CA1822 // Mark members as static
        private BlockSyntax RewriteBody( SyntaxNode bodyRootNode, IMethodSymbol symbol, SubstitutionContext context )
#pragma warning restore CA1822 // Mark members as static
        {
            var rewriter = new SubstitutingRewriter( context );

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
                                throw new AssertionFailedException( Justifications.CoverageMissing );

                            // return
                            //     Block()
                            //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case ExpressionSyntax rewrittenExpression:
                                return
                                    Block( ExpressionStatement( rewrittenExpression ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

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
                                throw new AssertionFailedException( Justifications.CoverageMissing );

                            // return
                            //     Block(
                            //             ReturnStatement(
                            //                 Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                            //                 LiteralExpression( SyntaxKind.DefaultLiteralExpression ),
                            //                 Token( SyntaxKind.SemicolonToken ) ) )
                            //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

                            case ExpressionSyntax rewrittenExpression:
                                return
                                    Block(
                                            ReturnStatement(
                                                Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                                                rewrittenExpression,
                                                Token( SyntaxKind.SemicolonToken ) ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

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
        
        public IReadOnlyDictionary<SyntaxNode, SyntaxNodeSubstitution>? GetSubstitutions(InliningContextIdentifier inliningContextId)
        {
            return this.AnalysisRegistry.GetSubstitutions( inliningContextId );
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
            if ( this.IntroductionRegistry.IsOverride( symbol )
                 || this.IntroductionRegistry.IsOverrideTarget( symbol ) )
            {
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
            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation } methodSymbol:
                    return this.RewriteMethod( (MethodDeclarationSyntax) syntax, methodSymbol, generationContext );

                case IMethodSymbol { MethodKind: MethodKind.Destructor } destructorSymbol:
                    return this.RewriteDestructor( (DestructorDeclarationSyntax) syntax, destructorSymbol, generationContext );

                case IMethodSymbol { MethodKind: MethodKind.Conversion } operatorSymbol:
                    return this.RewriteConversionOperator( (ConversionOperatorDeclarationSyntax) syntax, operatorSymbol, generationContext );

                case IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator } operatorSymbol:
                    return this.RewriteOperator( (OperatorDeclarationSyntax) syntax, operatorSymbol, generationContext );

                case IPropertySymbol propertySymbol:
                    return this.RewriteProperty( (PropertyDeclarationSyntax) syntax, propertySymbol, generationContext );

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
        /// <param name="semantic"></param>
        /// <returns></returns>
        private IMethodSymbol ResolveBodySource( IntermediateSymbolSemantic<IMethodSymbol> semantic )
        {
            if ( this.IntroductionRegistry.IsOverride( semantic.Symbol ) )
            {
                Invariant.Assert( semantic.Kind == IntermediateSymbolSemanticKind.Default );

                return semantic.Symbol;
            }

            if ( this.IntroductionRegistry.IsOverrideTarget( semantic.Symbol ) )
            {
                switch ( semantic.Kind )
                {
                    case IntermediateSymbolSemanticKind.Base:
                    case IntermediateSymbolSemanticKind.Default:
                        return semantic.Symbol;

                    case IntermediateSymbolSemanticKind.Final:
                        return (IMethodSymbol) this.IntroductionRegistry.GetLastOverride( semantic.Symbol );

                    default:
                        throw new AssertionFailedException();
                }
            }

            if ( semantic.Symbol.AssociatedSymbol != null && semantic.Symbol.AssociatedSymbol.IsExplicitInterfaceEventField() )
            {
                return semantic.Symbol;
            }

            throw new AssertionFailedException();
        }

        /// <summary>
        /// Gets a syntax node that will the the source of trivia of the specified declaration root block.
        /// </summary>
        private SyntaxNode? ResolveBodyBlockTriviaSource( IntermediateSymbolSemantic<IMethodSymbol> semantic, out bool shouldRemoveExistingTrivia )
        {
            ISymbol? symbol;

            if ( this.IntroductionRegistry.IsOverride( semantic.Symbol ) )
            {
                Invariant.Assert( semantic.Kind == IntermediateSymbolSemanticKind.Default );

                symbol = semantic.Symbol;
                shouldRemoveExistingTrivia = true;
            }
            else if ( this.IntroductionRegistry.IsOverrideTarget( semantic.Symbol ) )
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
                        throw new AssertionFailedException();
                }
            }
            else if ( semantic.Symbol.AssociatedSymbol != null && semantic.Symbol.AssociatedSymbol.IsExplicitInterfaceEventField() )
            {
                symbol = semantic.Symbol;
                shouldRemoveExistingTrivia = true;
            }
            else
            {
                throw new AssertionFailedException();
            }

            return symbol?.GetPrimaryDeclaration() switch
            {
                null => null,
                MethodDeclarationSyntax methodDeclaration => methodDeclaration.Body,
                AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.Body,
                ArrowExpressionClauseSyntax => null,
                _ => throw new AssertionFailedException()
            };
        }

        public static string GetOriginalImplMemberName( ISymbol symbol ) => GetSpecialMemberName( symbol, "Source" );

        public static string GetEmptyImplMemberName( ISymbol symbol ) => GetSpecialMemberName( symbol, "Empty" );

        public static string GetSpecialMemberName( ISymbol symbol, string suffix )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    if ( methodSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( symbol, methodSymbol.ExplicitInterfaceImplementations[0].Name, suffix );
                    }
                    else
                    {
                        return CreateName( symbol, methodSymbol.Name, suffix );
                    }

                case IPropertySymbol propertySymbol:
                    if ( propertySymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( symbol, propertySymbol.ExplicitInterfaceImplementations[0].Name, suffix );
                    }
                    else
                    {
                        return CreateName( symbol, propertySymbol.Name, suffix );
                    }

                case IEventSymbol eventSymbol:
                    if ( eventSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( symbol, eventSymbol.ExplicitInterfaceImplementations[0].Name, suffix );
                    }
                    else
                    {
                        return CreateName( symbol, eventSymbol.Name, suffix );
                    }

                default:
                    throw new AssertionFailedException();
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
    }
}