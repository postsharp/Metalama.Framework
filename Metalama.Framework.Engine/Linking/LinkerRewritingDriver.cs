// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Utilities;
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
        private readonly LinkerIntroductionRegistry _introductionRegistry;
        private readonly LinkerAnalysisRegistry _analysisRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly Compilation _intermediateCompilation;
        private readonly UserDiagnosticSink _diagnosticSink;

        public LinkerRewritingDriver(
            Compilation intermediateCompilation,
            LinkerIntroductionRegistry introductionRegistry,
            LinkerAnalysisRegistry analysisRegistry,
            UserDiagnosticSink diagnosticSink,
            IServiceProvider serviceProvider )
        {
            this._introductionRegistry = introductionRegistry;
            this._analysisRegistry = analysisRegistry;
            this._intermediateCompilation = intermediateCompilation;
            this._diagnosticSink = diagnosticSink;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Assembles a linked body of the method/accessor, where aspect reference annotations are replaced by target symbols and inlineable references are inlined.
        /// </summary>
        /// <param name="semantic">Method or accessor symbol.</param>
        /// <returns>Block representing the linked body.</returns>
        public BlockSyntax GetLinkedBody( IntermediateSymbolSemantic<IMethodSymbol> semantic, InliningContext inliningContext )
        {
            var replacements = new Dictionary<SyntaxNode, SyntaxNode?>();
            var symbol = this.ResolveBodySource( semantic );
            var bodyRootNode = this.GetBodyRootNode( symbol, inliningContext.SyntaxGenerationContext, out var isImplicitlyLinked );

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
                // TODO: This will not be hit until we are using results of control flow analysis. 
                throw new AssertionFailedException( Justifications.CoverageMissing );

                // // Add the implicit return for void methods.
                // inliningContext.UseLabel();
                //
                // rewrittenBody =
                //     Block(
                //             rewrittenBody,
                //             CreateGotoStatement() )
                //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }

            // Add the SourceCode annotation, if it is source code.
            if ( !(symbol.GetPrimarySyntaxReference() is { } primarySyntax && primarySyntax.GetSyntax().HasAnnotation( FormattingAnnotations.GeneratedCode )) )
            {
                rewrittenBody = rewrittenBody.AddSourceCodeAnnotation();
            }

            // Strip the leading and trailing trivia from the block (this is before and after the opening/closing brace).
            return rewrittenBody.WithoutTrivia();

            void AddAspectReferenceReplacements()
            {
                // Get all aspect references that were found in the symbol during analysis.
                var containedAspectReferences = this._analysisRegistry.GetContainedAspectReferences( symbol );

                // Collect syntax node replacements from inliners and redirect the rest to correct targets.
                foreach ( var aspectReference in containedAspectReferences )
                {
                    if ( aspectReference.Specification.Flags.HasFlag( AspectReferenceFlags.Inlineable )
                         && SymbolEqualityComparer.Default.Equals( aspectReference.ContainingSymbol, aspectReference.ResolvedSemantic.Symbol ) )
                    {
                        // Inlineable self-reference would cause a stack overflow.
                        throw new AssertionFailedException();
                    }

                    if ( this._analysisRegistry.IsInlineable( aspectReference.ResolvedSemantic, out var inliningSpecification ) )
                    {
                        inliningSpecification.SelectedInliners[aspectReference]
                            .Inline( inliningContext, aspectReference, out var replacedNode, out var newNode );

                        replacements.Add( replacedNode, newNode );
                    }
                    else
                    {
                        var linkedExpression = this.GetLinkedExpression( aspectReference, inliningContext.SyntaxGenerationContext );
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
                            Token( SyntaxKind.SemicolonToken ) )
                        .AddGeneratedCodeAnnotation();
            }
        }

        /// <summary>
        /// Gets a node that becomes root node of the target symbol in the final compilation. This is used to get a block/expression for implicit
        /// accessors, i.e. auto-properties and event fields.
        /// </summary>
        private SyntaxNode GetBodyRootNode( IMethodSymbol symbol, SyntaxGenerationContext generationContext, out bool isImplicitlyLinked )
        {
            var declaration = symbol.GetPrimaryDeclaration();

            if ( this._introductionRegistry.IsOverrideTarget( symbol ) )
            {
                // Override targets are implicitly linked, i.e. no replacement of aspect references is necessary.
                isImplicitlyLinked = true;

                switch ( declaration )
                {
                    case MethodDeclarationSyntax methodDecl:
                        return (SyntaxNode?) methodDecl.Body ?? methodDecl.ExpressionBody ?? throw new AssertionFailedException();

                    case AccessorDeclarationSyntax accessorDecl:
                        var body = (SyntaxNode?) accessorDecl.Body ?? accessorDecl.ExpressionBody;

                        if ( body != null && !(symbol.AssociatedSymbol != null && symbol.AssociatedSymbol.IsExplicitInterfaceEventField()) )
                        {
                            return body;
                        }
                        else
                        {
                            return GetImplicitAccessorBody( symbol, generationContext );
                        }

                    case ArrowExpressionClauseSyntax arrowExpressionClause:
                        // Expression-bodied property.
                        return arrowExpressionClause;

                    case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: EventFieldDeclarationSyntax } }:
                        return GetImplicitAccessorBody( symbol, generationContext );

                    default:
                        throw new AssertionFailedException();
                }
            }

            if ( this._introductionRegistry.IsOverride( symbol ) )
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

            if ( symbol.AssociatedSymbol != null && symbol.AssociatedSymbol.IsExplicitInterfaceEventField() )
            {
                isImplicitlyLinked = true;

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
                                throw new AssertionFailedException( Justifications.CoverageMissing );

                            // return
                            //     Block()
                            //         .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

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
            if ( this._introductionRegistry.IsOverride( symbol ) || this._introductionRegistry.IsOverrideTarget( symbol ) )
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
        /// <param name="syntax"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public IReadOnlyList<MemberDeclarationSyntax> RewriteMember( MemberDeclarationSyntax syntax, ISymbol symbol, SyntaxGenerationContext generationContext )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    return this.RewriteMethod( (MethodDeclarationSyntax) syntax, methodSymbol, generationContext );

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
        /// <param name="semantic"></param>
        /// <returns></returns>
        private IMethodSymbol ResolveBodySource( IntermediateSymbolSemantic<IMethodSymbol> semantic )
        {
            if ( this._introductionRegistry.IsOverride( semantic.Symbol ) )
            {
                Invariant.Assert( semantic.Kind == IntermediateSymbolSemanticKind.Default );

                return semantic.Symbol;
            }

            if ( this._introductionRegistry.IsOverrideTarget( semantic.Symbol ) )
            {
                switch ( semantic.Kind )
                {
                    case IntermediateSymbolSemanticKind.Base:
                    case IntermediateSymbolSemanticKind.Default:
                        return semantic.Symbol;

                    case IntermediateSymbolSemanticKind.Final:
                        return (IMethodSymbol) this._introductionRegistry.GetLastOverride( semantic.Symbol );

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
        /// Gets an expression that replaces the expression represented by the aspect reference. This for cases where the reference is not inlined.
        /// </summary>
        /// <param name="aspectReference"></param>
        /// <returns></returns>
        private ExpressionSyntax GetLinkedExpression( ResolvedAspectReference aspectReference, SyntaxGenerationContext syntaxGenerationContext )
        {
            // IMPORTANT: This method needs to always strip trivia if rewriting the existing expression.
            //            Trivia existing around the expression are preserved during substitution.
            if ( !SymbolEqualityComparer.Default.Equals(
                    aspectReference.ResolvedSemantic.Symbol.ContainingType,
                    aspectReference.ResolvedSemantic.Symbol.ContainingType ) )
            {
                throw new AssertionFailedException();
            }

            var targetSymbol = aspectReference.ResolvedSemantic.Symbol;
            var targetSemanticKind = aspectReference.ResolvedSemantic.Kind;

            if ( this._introductionRegistry.IsLastOverride( targetSymbol ) )
            {
                throw new AssertionFailedException( Justifications.CoverageMissing );

                // // If something is resolved to the last override, we will point to the target declaration instead.
                // targetSymbol = aspectReference.OriginalSymbol;
                // targetSemanticKind = IntermediateSymbolSemanticKind.Final;
            }

            // Determine the target name. Specifically, handle case when the resolved symbol points to the original implementation.
            var targetMemberName =
                targetSemanticKind switch
                {
                    IntermediateSymbolSemanticKind.Default
                        when SymbolEqualityComparer.Default.Equals(
                                 aspectReference.ResolvedSemantic.Symbol.ContainingType,
                                 aspectReference.ContainingSymbol.ContainingType )
                             && this._introductionRegistry.IsOverrideTarget( targetSymbol )
                        => GetOriginalImplMemberName( targetSymbol ),
                    IntermediateSymbolSemanticKind.Base
                        when SymbolEqualityComparer.Default.Equals(
                            aspectReference.ResolvedSemantic.Symbol.ContainingType,
                            aspectReference.ContainingSymbol.ContainingType )
                        => GetEmptyImplMemberName( targetSymbol ),
                    _ => targetSymbol.Name
                };

            // Presume that all (annotated) aspect references are member access expressions.
            switch ( aspectReference.Expression )
            {
                case MemberAccessExpressionSyntax memberAccessExpression:
                    // The reference expression is member access.

                    if ( SymbolEqualityComparer.Default.Equals(
                            aspectReference.ContainingSymbol.ContainingType,
                            targetSymbol.ContainingType ) )
                    {
                        if ( aspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                        {
                            return memberAccessExpression
                                .WithExpression( ThisExpression() )
                                .WithName( IdentifierName( targetMemberName ) )
                                .WithoutTrivia();
                        }
                        else
                        {
                            // This is the same type, we can just change the identifier in the expression.
                            // TODO: Is the target always accessible?
                            return memberAccessExpression
                                .WithName( IdentifierName( targetMemberName ) )
                                .WithoutTrivia();
                        }
                    }
                    else
                    {
                        if ( targetSymbol.ContainingType.TypeKind == TypeKind.Interface )
                        {
                            // Overrides are always targeting member defined in the current type.
                            throw new AssertionFailedException( Justifications.CoverageMissing );

                            // return memberAccessExpression
                            //     .WithExpression( ThisExpression() )
                            //     .WithName( IdentifierName( targetMemberName ) );
                        }

                        if ( targetSymbol.IsStatic )
                        {
                            // Static member access where the target is a different type.
                            return
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    syntaxGenerationContext.SyntaxGenerator.Type( targetSymbol.ContainingType ),
                                    IdentifierName( targetMemberName ) );
                        }
                        else
                        {
                            if ( targetSymbol.ContainingType.Is( aspectReference.ContainingSymbol.ContainingType ) )
                            {
                                throw new AssertionFailedException( "Resolved symbol is declared in a derived class." );
                            }
                            else if ( aspectReference.ContainingSymbol.ContainingType.Is( targetSymbol.ContainingType ) )
                            {
                                // Resolved symbol is declared in a base class.
                                switch ( memberAccessExpression.Expression )
                                {
                                    case IdentifierNameSyntax:
                                    case BaseExpressionSyntax:
                                    case ThisExpressionSyntax:
                                        return
                                            MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    BaseExpression(),
                                                    IdentifierName( targetMemberName ) );

                                    default:
                                        var aspectInstance = this.ResolveAspectInstance( aspectReference );

                                        var targetDeclaration = aspectInstance.TargetDeclaration.GetSymbol( this._intermediateCompilation );

                                        this._diagnosticSink.Report(
                                            AspectLinkerDiagnosticDescriptors.CannotUseBaseInvokerWithNonInstanceExpression.CreateRoslynDiagnostic(
                                                targetDeclaration.GetDiagnosticLocation(),
                                                (aspectInstance.AspectClass.ShortName, TargetDeclaration: targetDeclaration) ) );

                                        return aspectReference.Expression;
                                }
                            }
                            else
                            {
                                // Resolved symbol is unrelated to the containing symbol.
                                return memberAccessExpression
                                    .WithName( IdentifierName( targetMemberName ) )
                                    .WithoutTrivia();
                            }
                        }
                    }

                case ConditionalAccessExpressionSyntax conditionalAccessExpression:
                    if ( SymbolEqualityComparer.Default.Equals(
                            aspectReference.ContainingSymbol.ContainingType,
                            targetSymbol.ContainingType ) )
                    {
                        if ( aspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                        {
                            throw new AssertionFailedException( Justifications.CoverageMissing );
                        }
                        else
                        {
                            var rewriter = new ConditionalAccessRewriter( targetMemberName );

                            return 
                                (ExpressionSyntax) rewriter.Visit( 
                                    conditionalAccessExpression
                                    .WithoutTrivia() );
                        }
                    }
                    else
                    {
                        throw new AssertionFailedException( Justifications.CoverageMissing );
                    }

                default:
                    throw new AssertionFailedException();
            }
        }

        private IAspectInstanceInternal ResolveAspectInstance( ResolvedAspectReference aspectReference )
        {
            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( aspectReference.ContainingSymbol );

            return introducedMember.AssertNotNull().Introduction.Advice.Aspect;
        }

        private static string GetOriginalImplMemberName( ISymbol symbol ) => GetSpecialMemberName( symbol, "Source" );

        private static string GetEmptyImplMemberName( ISymbol symbol ) => GetSpecialMemberName( symbol, "Empty" );

        private static string GetSpecialMemberName( ISymbol symbol, string suffix )
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
    }
}