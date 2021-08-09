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
                    if ( aspectReference.Specification.Flags.HasFlag( AspectReferenceFlags.Inlineable ) 
                        && SymbolEqualityComparer.Default.Equals( aspectReference.ContainingSymbol, aspectReference.ResolvedSemantic.Symbol) )
                    {
                        // Inlineable self-reference would cause a stack overflow.
                        throw new AssertionFailedException();
                    }

                    if ( this._analysisRegistry.IsInlineable( aspectReference.ResolvedSemantic, out var inliningSpecification ) )
                    {
                        inliningSpecification.SelectedInliners[aspectReference].Inline( inliningContext, aspectReference, out var replacedNode, out var newNode );
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
            if ( this._introductionRegistry.IsOverride( symbol ) )
            {
                return symbol;
            }

            if ( this._introductionRegistry.IsOverrideTarget( symbol ) )
            {
                return (IMethodSymbol) this._introductionRegistry.GetLastOverride( symbol ).AssertNotNull();
            }

            if ( symbol.AssociatedSymbol != null && symbol.AssociatedSymbol.IsExplicitInterfaceEventField() )
            {
                return symbol;
            }

            throw new AssertionFailedException();
        }

        private ExpressionSyntax GetLinkedExpression( ResolvedAspectReference aspectReference )
        {
            if ( !SymbolEqualityComparer.Default.Equals( aspectReference.ResolvedSemantic.Symbol.ContainingType, aspectReference.ResolvedSemantic.Symbol.ContainingType ) )
            {
                throw new AssertionFailedException();
            }

            var targetSymbol = aspectReference.ResolvedSemantic.Symbol;
            var targetSemanticKind = aspectReference.ResolvedSemantic.Kind;
            if ( this._introductionRegistry.IsLastOverride( targetSymbol ) )
            {
                // If something is resolved to the last override, we will point to the target declaration instead.
                targetSymbol = aspectReference.OriginalSymbol;
                targetSemanticKind = IntermediateSymbolSemanticKind.Default;
            }

            // Determine the target name. Specifically, handle case when the resolved symbol points to the original implementation.
            var targetMemberName =
                targetSemanticKind switch
                {
                    IntermediateSymbolSemanticKind.Default when this._introductionRegistry.IsOverrideTarget( targetSymbol ) => GetOriginalImplMemberName( targetSymbol ),
                    IntermediateSymbolSemanticKind.Base => GetEmptyImplMemberName( targetSymbol ),
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
                        if ( targetSymbol.ContainingType.TypeKind == TypeKind.Interface )
                        {
                            return memberAccessExpression
                                .WithExpression( ThisExpression() )
                                .WithName( IdentifierName( targetMemberName ) );
                        }

                        if ( targetSymbol.IsStatic )
                        {
                            // Static member access where the target is a different type.
                            return MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( targetSymbol.ContainingType ),
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
            => GetSpecialMemberName( symbol, "OriginalImpl" );

        internal static string GetEmptyImplMemberName( ISymbol symbol )
            => GetSpecialMemberName( symbol, "EmptyImpl" );

        internal static string GetSpecialMemberName( ISymbol symbol, string suffix )
        {
            switch ( symbol )
            {
                case IMethodSymbol methodSymbol:
                    if ( methodSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( methodSymbol.ExplicitInterfaceImplementations[0].Name, suffix );
                    }
                    else
                    {
                        return CreateName( methodSymbol.Name, suffix );
                    }

                case IPropertySymbol propertySymbol:
                    if ( propertySymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( propertySymbol.ExplicitInterfaceImplementations[0].Name, suffix );
                    }
                    else
                    {
                        return CreateName( propertySymbol.Name, suffix );
                    }

                case IEventSymbol eventSymbol:
                    if ( eventSymbol.ExplicitInterfaceImplementations.Any() )
                    {
                        return CreateName( eventSymbol.ExplicitInterfaceImplementations[0].Name, suffix );
                    }
                    else
                    {
                        return CreateName( eventSymbol.Name, suffix );
                    }

                default:
                    throw new AssertionFailedException();
            }

            static string CreateName( string name, string suffix ) => $"__{name}__{suffix}";
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