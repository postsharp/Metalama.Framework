// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    private sealed partial class Rewriter : SafeSyntaxRewriter
    {
        private readonly CompilationModel _compilation;
        private readonly SemanticModelProvider _semanticModelProvider;
        private readonly SyntaxGenerationContextFactory _syntaxGenerationContextFactory;
        private readonly ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> _diagnosticSuppressions;
        private readonly TransformationCollection _transformationCollection;
        private readonly SyntaxTree _syntaxTreeForGlobalAttributes;

        private readonly IUserDiagnosticSink _diagnostics;

        // Maps a diagnostic id to the number of times it has been suppressed.
        private ImmutableHashSet<string> _activeSuppressions = ImmutableHashSet.Create<string>( StringComparer.OrdinalIgnoreCase );

        public Rewriter(
            CompilationContext compilationContext,
            TransformationCollection syntaxTransformationCollection,
            ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> diagnosticSuppressions,
            CompilationModel compilation,
            SyntaxTree syntaxTreeForGlobalAttributes,
            IUserDiagnosticSink diagnostics )
        {
            this._syntaxGenerationContextFactory = compilationContext.SyntaxGenerationContextFactory;
            this._diagnosticSuppressions = diagnosticSuppressions;
            this._compilation = compilation;
            this._transformationCollection = syntaxTransformationCollection;
            this._semanticModelProvider = compilation.RoslynCompilation.GetSemanticModelProvider();
            this._syntaxTreeForGlobalAttributes = syntaxTreeForGlobalAttributes;
            this._diagnostics = diagnostics;
        }

        private bool PreserveTrivia => this._syntaxGenerationContextFactory.Default.PreserveTrivia;

        public override bool VisitIntoStructuredTrivia => true;

        /// <summary>
        /// Gets the list of suppressions for a given syntax node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSuppressions( SyntaxNode node )
        {
            return node switch
            {
                FieldDeclarationSyntax { Declaration.Variables.Count: 1 } field => FindSuppressionsCore( field.Declaration.Variables.First() ),

                // If we have a field declaration that declares many field, we merge all suppressions
                // and suppress all for all fields. This is significantly simpler than splitting the declaration.
                FieldDeclarationSyntax { Declaration.Variables.Count: > 1 } field => field.Declaration.Variables.SelectAsReadOnlyList( FindSuppressionsCore )
                    .SelectMany( l => l ),

                _ => FindSuppressionsCore( node )
            };

            IEnumerable<string> FindSuppressionsCore( SyntaxNode identifierNode )
            {
                var declaredSymbol = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( identifierNode );

                if ( declaredSymbol != null )
                {
                    var declaration = this._compilation.Factory.GetDeclaration( declaredSymbol );

                    return this.GetSuppressions( declaration );
                }
                else
                {
                    return ImmutableArray<string>.Empty;
                }
            }
        }

        private IEnumerable<string> GetSuppressions( IDeclaration declaration )
            => this._diagnosticSuppressions[declaration].Select( s => s.Definition.SuppressedDiagnosticId );

        /// <summary>
        /// Adds suppression to a node. This is done both by adding <c>#pragma warning</c> trivia
        /// around the node and by updating (or even suppressing) the <c>#pragma warning</c>
        /// inside the node.
        /// </summary>
        private T AddSuppression<T>( T node, IReadOnlyList<string> suppressionsOnThisElement )
            where T : SyntaxNode
        {
            var transformedNode = node;

            if ( !this._activeSuppressions.IsEmpty && node is not BaseTypeDeclarationSyntax )
            {
                // TODO: We are probably processing classes incorrectly.

                // Since we're adding suppressions, we need to visit each `#pragma warning` of the added node to update them.
                transformedNode = (T) this.Visit( transformedNode ).AssertNotNull();
            }

            if ( suppressionsOnThisElement.Any() )
            {
                // Add `#pragma warning` trivia around the node.
                var errorCodes = SeparatedList<ExpressionSyntax>( suppressionsOnThisElement.Distinct().OrderBy( e => e ).Select( IdentifierName ) );

                var disable = Trivia( SyntaxFactoryEx.PragmaWarningDirectiveTrivia( SyntaxKind.DisableKeyword, errorCodes ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

                var restore = Trivia( SyntaxFactoryEx.PragmaWarningDirectiveTrivia( SyntaxKind.RestoreKeyword, errorCodes ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                transformedNode = transformedNode
                    .WithLeadingTrivia( transformedNode.GetLeadingTrivia().InsertRange( 0, new[] { ElasticLineFeed, disable, ElasticLineFeed } ) )
                    .WithTrailingTrivia( transformedNode.GetTrailingTrivia().AddRange( new[] { ElasticLineFeed, restore, ElasticLineFeed } ) );
#pragma warning restore LAMA0832
            }

            return transformedNode;
        }

        private (SyntaxList<AttributeListSyntax> Attributes, List<SyntaxTrivia> Trivia)? RewriteDeclarationAttributeLists(
            SyntaxNode originalDeclaringNode,
            SyntaxList<AttributeListSyntax> attributeLists,
            SyntaxNode? originalNodeForTrivia = null )
        {
            if ( !this._transformationCollection.IsNodeWithModifiedAttributes( originalDeclaringNode ) )
            {
                return null;
            }

            // Resolve the symbol.
            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalDeclaringNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalDeclaringNode );

            if ( symbol == null )
            {
                return null;
            }

            // Find trivia that's directly on the declaration (and not on its attributes).
            originalNodeForTrivia ??= originalDeclaringNode;

            if ( attributeLists.Any() )
            {
                originalNodeForTrivia = originalNodeForTrivia.RemoveNodes( attributeLists, SyntaxRemoveOptions.KeepNoTrivia )!;
            }

            var originalDeclarationTrivia = originalNodeForTrivia.GetLeadingTrivia();

            var outputLists = new List<AttributeListSyntax>();
            var outputTrivias = new List<SyntaxTrivia>( originalDeclarationTrivia );
            SyntaxGenerationContext? syntaxGenerationContext = null;

            this.RewriteAttributeLists(
                Ref.FromSymbol( symbol, this._compilation.CompilationContext ),
                SyntaxKind.None,
                originalDeclaringNode,
                attributeLists,
                ( a, n ) => a.ContainingDeclaration.GetPrimaryDeclarationSyntax() == n,
                outputLists,
                outputTrivias,
                ref syntaxGenerationContext );

            switch ( symbol )
            {
                case IMethodSymbol method:
                    this.RewriteAttributeLists(
                        Ref.ReturnParameter( method, this._compilation.CompilationContext ),
                        SyntaxKind.ReturnKeyword,
                        originalDeclaringNode,
                        attributeLists,
                        ( a, n ) => a.ContainingDeclaration.ContainingDeclaration!.GetPrimaryDeclarationSyntax() == n,
                        outputLists,
                        outputTrivias,
                        ref syntaxGenerationContext );

                    break;
            }

            if ( outputLists.Count == 0 )
            {
                return (default, outputTrivias);
            }
            else
            {
                return (List( outputLists ), outputTrivias);
            }
        }

        private void RewriteAttributeLists(
            Ref<IDeclaration> target,
            SyntaxKind targetKind,
            SyntaxNode originalDeclaringNode,
            SyntaxList<AttributeListSyntax> inputAttributeLists,
            Func<AttributeBuilder, SyntaxNode, bool> isPrimaryNode,
            List<AttributeListSyntax> outputAttributeLists,
            List<SyntaxTrivia> outputTrivia,
            ref SyntaxGenerationContext? syntaxGenerationContext )
        {
            // Get the final list of attributes.
            var finalModelAttributes = this._compilation.GetAttributeCollection( target );

            // Remove attributes from the list.
            foreach ( var list in inputAttributeLists )
            {
                // Skip the different target kinds.
                if ( list.Target == null )
                {
                    if ( targetKind != SyntaxKind.None )
                    {
                        continue;
                    }
                }
                else
                {
                    if ( !list.Target.Identifier.IsKind( targetKind ) )
                    {
                        continue;
                    }
                }

                var modifiedList = list;

                foreach ( var attribute in list.Attributes )
                {
                    if ( !finalModelAttributes.Any( a => a.IsSyntax( attribute ) ) )
                    {
                        modifiedList = modifiedList.RemoveNode( attribute, SyntaxRemoveOptions.KeepDirectives )!;
                    }
                }

                if ( modifiedList.Attributes.Count > 0 )
                {
                    outputAttributeLists.Add( modifiedList );
                }
                else
                {
                    // If we are removing a custom attribute, keep its trivia.
                    foreach ( var trivia in list.GetLeadingTrivia() )
                    {
                        switch ( trivia.Kind() )
                        {
                            case SyntaxKind.MultiLineCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia:
                                outputTrivia.Add( trivia );

                                break;

                            case SyntaxKind.SingleLineCommentTrivia or SyntaxKind.SingleLineDocumentationCommentTrivia:
                                outputTrivia.Add( trivia );
                                outputTrivia.Add( ElasticLineFeed );

                                break;
                        }
                    }
                }
            }

            // Add new attributes.
            foreach ( var attribute in finalModelAttributes )
            {
                if ( attribute.Target is AttributeBuilder attributeBuilder && isPrimaryNode( attributeBuilder, originalDeclaringNode ) )
                {
                    syntaxGenerationContext ??= this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( originalDeclaringNode );

                    var newAttribute = syntaxGenerationContext.SyntaxGenerator.Attribute( attributeBuilder )
                        .AssertNotNull();

                    var newList = AttributeList( SingletonSeparatedList( newAttribute ) )
                        .WithTrailingTriviaIfNecessary( ElasticLineFeed, syntaxGenerationContext.NormalizeWhitespace )
                        .WithAdditionalAnnotations(
                            attributeBuilder.ParentAdvice?.Aspect.AspectClass.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation );

                    if ( targetKind != SyntaxKind.None )
                    {
                        newList = newList.WithTarget( AttributeTargetSpecifier( Token( targetKind ) ) );
                    }

                    if ( outputTrivia.Any() && !outputAttributeLists.Any() )
                    {
                        newList = newList.WithLeadingTriviaIfNecessary(
                            newList.GetLeadingTrivia().InsertRange( 0, outputTrivia ),
                            syntaxGenerationContext.PreserveTrivia );

                        outputTrivia.Clear();
                    }

                    outputAttributeLists.Add( newList );
                }
            }
        }

        private T ReplaceAttributes<T>( T node, (SyntaxList<AttributeListSyntax> Attributes, List<SyntaxTrivia> Trivia)? attributesTuple )
            where T : MemberDeclarationSyntax
        {
            if ( attributesTuple is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    return (T) node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    return (T) node.WithAttributeLists( attributes );
                }
            }
            else
            {
                return node;
            }
        }

        public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode? VisitEnumDeclaration( EnumDeclarationSyntax node )
        {
            var originalNode = node;
            var members = new List<EnumMemberDeclarationSyntax>( node.Members.Count );

            using ( var suppressionContext = this.WithSuppressions( node ) )
            {
                // Process the type members.
                foreach ( var member in node.Members )
                {
                    var visitedMember = this.VisitEnumMemberDeclarationCore( member );

                    using ( var memberSuppressions = this.WithSuppressions( member ) )
                    {
                        var memberWithSuppressions = this.AddSuppression( visitedMember, memberSuppressions.NewSuppressions );
                        members.Add( memberWithSuppressions );
                    }
                }

                node = this.AddSuppression( node, suppressionContext.NewSuppressions );
            }

            node = node.WithMembers( SeparatedList( members ) );

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        public override SyntaxNode? VisitDelegateDeclaration( DelegateDeclarationSyntax node )
        {
            var originalNode = node;

            using ( var suppressionContext = this.WithSuppressions( node ) )
            {
                node = this.AddSuppression( node, suppressionContext.NewSuppressions );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private SyntaxNode VisitTypeDeclaration<T>( T node )
            where T : TypeDeclarationSyntax
        {
            var originalNode = node;
            var members = new List<MemberDeclarationSyntax>( node.Members.Count );
            var additionalBaseList = this._transformationCollection.GetIntroducedInterfacesForTypeDeclaration( node );
            var syntaxGenerationContext = this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( node );

            var baseList = node.BaseList;

            var parameterList = node.GetParameterList();

            if ( this._transformationCollection.TryGetMemberLevelTransformations( node, out var memberLevelTransformations ) )
            {
                this.ApplyMemberLevelTransformationsToPrimaryConstructor(
                    node,
                    memberLevelTransformations,
                    syntaxGenerationContext,
                    out baseList,
                    out parameterList );
            }

            using ( var suppressionContext = this.WithSuppressions( node ) )
            {
                // Process the type members.
                foreach ( var member in node.Members )
                {
                    foreach ( var visitedMember in this.VisitMember( member ) )
                    {
                        using ( var memberSuppressions = this.WithSuppressions( member ) )
                        {
                            var memberWithSuppressions = this.AddSuppression( visitedMember, memberSuppressions.NewSuppressions );
                            members.Add( memberWithSuppressions );
                        }
                    }

                    // We have to call AddIntroductionsOnPosition outside of the previous suppression scope, otherwise we don't get new suppressions.
                    AddInjectionsOnPosition( new InsertPosition( InsertPositionRelation.After, member ) );
                }

                AddInjectionsOnPosition( new InsertPosition( InsertPositionRelation.Within, node ) );

                node = this.AddSuppression( node, suppressionContext.NewSuppressions );

                // If the type has no braces, add them.
                if ( node.OpenBraceToken.IsKind( SyntaxKind.None ) && members.Count > 0 )
                {
                    // TODO: trivias.
                    node = (T) node
                        .WithOpenBraceToken( Token( SyntaxKind.OpenBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                        .WithCloseBraceToken( Token( SyntaxKind.CloseBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                        .WithSemicolonToken( default );
                }

                node = (T) node.WithMembers( List( members ) );

                // Process the type bases.
                if ( additionalBaseList.Any() )
                {
                    if ( baseList == null )
                    {
                        node = (T) node
                            .WithIdentifier(
                                node.Identifier.WithTrailingTriviaIfNecessary(
                                    default,
                                    syntaxGenerationContext.PreserveTrivia || node.Identifier.ContainsDirectives ) )
                            .WithBaseList(
                                BaseList( SeparatedList( additionalBaseList.SelectAsReadOnlyList( i => i.Syntax ) ) )
                                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                            .WithTrailingTriviaIfNecessary( node.Identifier.TrailingTrivia, syntaxGenerationContext.PreserveTrivia );
                    }
                    else
                    {
                        node = (T) node.WithBaseList(
                            BaseList(
                                baseList.Types.AddRange(
                                    additionalBaseList.SelectAsReadOnlyList(
                                        i => i.Syntax.WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )! ) ) );
                    }
                }
                else if ( baseList != null )
                {
                    node = (T) node.WithBaseList( baseList );
                }

                node = (T) node.WithParameterList( parameterList );

                // Rewrite attributes.
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
                node = this.ReplaceAttributes( node, rewrittenAttributes );

                return node;
            }

            // TODO: Try to avoid closure allocation.
            void AddInjectionsOnPosition( InsertPosition position )
            {
                var injectedMembersAtPosition = this._transformationCollection.GetInjectedMembersOnPosition( position );

                foreach ( var injectedMember in injectedMembersAtPosition )
                {
                    // We should inject into a correct syntax tree.
                    Invariant.Assert( injectedMember.Transformation.TransformedSyntaxTree == originalNode.SyntaxTree );

                    // Allow for tracking of the node inserted.
                    // IMPORTANT: This need to be here and cannot be in injectedMember.Syntax, result of TrackNodes is not trackable!
                    var injectedNode = injectedMember.Syntax.TrackNodes( injectedMember.Syntax );

                    switch ( injectedMember.Declaration )
                    {
                        case IMethod or IConstructor:
                            // TODO: AssertNotNull is needed due to some weird bug in Roslyn.
                            var entryStatements = this._transformationCollection.GetInjectedInitialStatements( injectedMember );

                            injectedNode = InjectStatementsIntoMemberDeclaration( injectedMember.Declaration, entryStatements, injectedNode );

                            break;

                        case IProperty property:
                            if ( property.GetMethod != null )
                            {
                                var getEntryStatements = this._transformationCollection.GetInjectedInitialStatements( property.GetMethod, injectedMember );

                                injectedNode = InjectStatementsIntoMemberDeclaration( property.GetMethod, getEntryStatements, injectedNode );
                            }

                            if ( property.SetMethod != null )
                            {
                                var setEntryStatements = this._transformationCollection.GetInjectedInitialStatements( property.SetMethod, injectedMember );

                                injectedNode = InjectStatementsIntoMemberDeclaration( property.SetMethod, setEntryStatements, injectedNode );
                            }

                            break;
                    }

                    injectedNode = injectedNode
                        .WithLeadingTriviaIfNecessary( new SyntaxTriviaList( ElasticLineFeed, ElasticLineFeed ), syntaxGenerationContext.NormalizeWhitespace )
                        .WithGeneratedCodeAnnotation( injectedMember.Transformation.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation )!;

                    // Insert inserted statements into 
                    switch ( injectedNode )
                    {
                        case ConstructorDeclarationSyntax constructorDeclaration:
                            {
                                if ( injectedMember.DeclarationBuilder != null &&
                                     this._transformationCollection.TryGetMemberLevelTransformations(
                                         injectedMember.DeclarationBuilder.AssertNotNull(),
                                         out var memberLevelTransformations ) )
                                {
                                    injectedNode = this.ApplyMemberLevelTransformations(
                                        constructorDeclaration,
                                        memberLevelTransformations,
                                        syntaxGenerationContext );
                                }

                                break;
                            }

                        case FieldDeclarationSyntax fieldDeclaration:
                            {
                                if ( this._transformationCollection.TryGetMemberLevelTransformations(
                                        injectedMember.DeclarationBuilder.AssertNotNull(),
                                        out var memberLevelTransformations ) )
                                {
                                    injectedNode = this.ApplyMemberLevelTransformations( fieldDeclaration, memberLevelTransformations );
                                }

                                break;
                            }

                        case PropertyDeclarationSyntax propertyDeclaration:
                            {
                                if ( injectedMember.DeclarationBuilder != null &&
                                     this._transformationCollection.TryGetMemberLevelTransformations(
                                         injectedMember.DeclarationBuilder,
                                         out var memberLevelTransformations ) )
                                {
                                    injectedNode = this.ApplyMemberLevelTransformations( propertyDeclaration, memberLevelTransformations );
                                }

                                break;
                            }
                    }

                    if ( injectedMember.Declaration != null )
                    {
                        using ( var suppressions = this.WithSuppressions( injectedMember.Declaration ) )
                        {
                            injectedNode = this.AddSuppression( injectedNode, suppressions.NewSuppressions );
                        }
                    }

                    members.Add( injectedNode );
                }
            }
        }

        private static MemberDeclarationSyntax InjectStatementsIntoMemberDeclaration(
            IMemberOrNamedType contextDeclaration,
            IReadOnlyList<StatementSyntax> entryStatements,
            MemberDeclarationSyntax currentNode )
        {
            if ( entryStatements.Count == 0 )
            {
                return currentNode;
            }

            switch ( currentNode )
            {
                case ConstructorDeclarationSyntax { Body: { } body } constructor:
                    return constructor.WithBody( ReplaceBlock( entryStatements, body ) );

                case ConstructorDeclarationSyntax { ExpressionBody: { } expressionBody } constructor:
                    return
                        constructor.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            body: ReplaceExpression( entryStatements, expressionBody.Expression, true ) );

                // Static constructor overrides also go here.
                case MethodDeclarationSyntax { Body: { } body } method:
                    return method.WithBody( ReplaceBlock( entryStatements, body ) );

                case MethodDeclarationSyntax { ExpressionBody: { } expressionBody } method:
                    var returnsVoid =
                        contextDeclaration switch
                        {
                            IConstructor => true,
                            IMethod { IsAsync: false } contextMethod => contextMethod.ReturnType.Is( Code.SpecialType.Void ),
                            IMethod { IsAsync: true } contextMethod => contextMethod.GetAsyncInfo().ResultType.Is( Code.SpecialType.Void ),
                            _ => throw new InvalidOperationException( $"Not supported: {contextDeclaration}" )
                        };

                    return
                        method.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            body: ReplaceExpression( entryStatements, expressionBody.Expression, returnsVoid ) );

                case MethodDeclarationSyntax { Body: null, ExpressionBody: null }:
                    throw new AssertionFailedException( $"Method without body not supported: {contextDeclaration}" );

                case OperatorDeclarationSyntax { Body: { } body } @operator:
                    return @operator.WithBody( ReplaceBlock( entryStatements, body ) );

                case OperatorDeclarationSyntax { ExpressionBody: { } expressionBody } @operator:
                    return
                        @operator.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            body: ReplaceExpression( entryStatements, expressionBody.Expression, false ) );

                case PropertyDeclarationSyntax { ExpressionBody: { } expressionBody } property:
                    Invariant.Assert( contextDeclaration is IMethod { MethodKind: Code.MethodKind.PropertyGet } );

                    return
                        property.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            accessorList: AccessorList(
                                SingletonList(
                                    AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration,
                                        ReplaceExpression( entryStatements, expressionBody.Expression, false ) ) ) ) );

                case PropertyDeclarationSyntax { AccessorList: { } accessorList } property:
                    Invariant.Assert( contextDeclaration is IMethod { MethodKind: Code.MethodKind.PropertyGet or Code.MethodKind.PropertySet } );

                    return
                        property.WithAccessorList(
                            accessorList.WithAccessors(
                                List(
                                    accessorList.Accessors.SelectAsArray(
                                        a =>
                                            IsMatchingAccessor( a, contextDeclaration )
                                                ? a switch
                                                {
                                                    { Body: { } body } => a.WithBody( ReplaceBlock( entryStatements, body ) ),
                                                    { ExpressionBody: { } expressionBody } =>
                                                        a.PartialUpdate(
                                                            expressionBody: null,
                                                            semicolonToken: default(SyntaxToken),
                                                            body: ReplaceExpression(
                                                                entryStatements,
                                                                expressionBody.Expression,
                                                                a.Kind() is not SyntaxKind.GetAccessorDeclaration ) ),
                                                    _ => throw new AssertionFailedException( $"Not supported: {a.Kind()}" ),
                                                }
                                                : a ) ) ) );

                    static bool IsMatchingAccessor( AccessorDeclarationSyntax accessorDeclaration, IDeclaration contextDeclaration )
                        => (accessorDeclaration.Kind(), contextDeclaration) switch
                        {
                            (SyntaxKind.GetAccessorDeclaration, IMethod { MethodKind: Code.MethodKind.PropertyGet }) => true,
                            (SyntaxKind.SetAccessorDeclaration, IMethod { MethodKind: Code.MethodKind.PropertySet }) => true,
                            (SyntaxKind.InitAccessorDeclaration, IMethod { MethodKind: Code.MethodKind.PropertySet }) => true,
                            _ => false,
                        };

                default:
                    throw new AssertionFailedException( $"Not supported: {currentNode.Kind()}" );
            }

            BlockSyntax ReplaceBlock( IReadOnlyList<StatementSyntax> entryStatements, BlockSyntax targetBlock )
            {
                return
                    Block(
                        Block( List( entryStatements ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                        targetBlock
                            .WithSourceCodeAnnotationIfNotGenerated()
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ) );
            }

            BlockSyntax ReplaceExpression( IReadOnlyList<StatementSyntax> entryStatements, ExpressionSyntax targetExpression, bool returnsVoid )
            {
                return
                    Block(
                        Block( List( entryStatements ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                        returnsVoid
                            ? ExpressionStatement( targetExpression.WithSourceCodeAnnotationIfNotGenerated() )
                            : ReturnStatement(
                                Token( default, SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                targetExpression.WithSourceCodeAnnotationIfNotGenerated(),
                                Token( SyntaxKind.SemicolonToken ) ) );
            }
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitMember( MemberDeclarationSyntax member )
        {
            static MemberDeclarationSyntax[] Singleton( MemberDeclarationSyntax m )
            {
                return new[] { m };
            }

            return member switch
            {
                ConstructorDeclarationSyntax constructor => Singleton( this.VisitConstructorDeclarationCore( constructor ) ),
                MethodDeclarationSyntax method => Singleton( this.VisitMethodDeclarationCore( method ) ),
                PropertyDeclarationSyntax property => Singleton( this.VisitPropertyDeclarationCore( property ) ),
                OperatorDeclarationSyntax @operator => Singleton( this.VisitOperatorDeclarationCore( @operator ) ),
                EventDeclarationSyntax @event => Singleton( this.VisitEventDeclarationCore( @event ) ),
                FieldDeclarationSyntax field => this.VisitFieldDeclarationCore( field ),
                EventFieldDeclarationSyntax @eventField => this.VisitEventFieldDeclarationCore( @eventField ),
                _ => Singleton( (MemberDeclarationSyntax) this.Visit( member )! )
            };
        }

        private FieldDeclarationSyntax ApplyMemberLevelTransformations(
            FieldDeclarationSyntax fieldDeclaration,
            MemberLevelTransformations memberLevelTransformations )
        {
            Invariant.Assert( fieldDeclaration.Declaration.Variables.Count == 1 );

            var originalFieldVariableDeclarator = fieldDeclaration.Declaration.Variables[0];
            var newFieldVariableDeclarator = this.ApplyMemberLevelTransformations( originalFieldVariableDeclarator, memberLevelTransformations );

            return fieldDeclaration.ReplaceNode( originalFieldVariableDeclarator, newFieldVariableDeclarator );
        }

        private VariableDeclaratorSyntax ApplyMemberLevelTransformations(
            VariableDeclaratorSyntax fieldVariableDeclarator,
            MemberLevelTransformations memberLevelTransformations )
        {
            Invariant.Assert( memberLevelTransformations.Parameters.Length == 0 );
            Invariant.Assert( memberLevelTransformations.Arguments.Length == 0 );

            var transformation = memberLevelTransformations.Expressions[0];

            if ( memberLevelTransformations.Expressions.Length == 1 )
            {
                // The expressions 'default' and 'default!' in the initializer are considered the same as if there was no initializer.
                if ( fieldVariableDeclarator.Initializer?.Value is not (null or
                    { RawKind: (int) SyntaxKind.DefaultLiteralExpression } or
                    PostfixUnaryExpressionSyntax
                    {
                        RawKind: (int) SyntaxKind.SuppressNullableWarningExpression,
                        Operand.RawKind: (int) SyntaxKind.DefaultLiteralExpression
                    }) )
                {
                    this._diagnostics.Report(
                        AspectLinkerDiagnosticDescriptors.CannotAssignToExpressionFromPrimaryConstructor.CreateRoslynDiagnostic(
                            fieldVariableDeclarator.GetDiagnosticLocation(),
                            (fieldVariableDeclarator.Identifier.ValueText, transformation.TargetMember.DeclaringType,
                             "The field already has an initializer.") ) );
                }
                else
                {
                    fieldVariableDeclarator = fieldVariableDeclarator.WithInitializer( EqualsValueClause( transformation.InitializerExpression ) );
                }
            }
            else if ( memberLevelTransformations.Expressions.Length > 1 )
            {
                var aspects = memberLevelTransformations.Expressions.SelectAsArray( e => e.ParentAdvice.Aspect.ToString() );

                this._diagnostics.Report(
                    AspectLinkerDiagnosticDescriptors.CannotAssignToMemberMoreThanOnceFromPrimaryConstructor.CreateRoslynDiagnostic(
                        fieldVariableDeclarator.GetDiagnosticLocation(),
                        (transformation.TargetMember.DeclarationKind, transformation.TargetMember, transformation.TargetMember.DeclaringType, aspects) ) );
            }

            return fieldVariableDeclarator;
        }

        private PropertyDeclarationSyntax ApplyMemberLevelTransformations(
            PropertyDeclarationSyntax propertyDeclaration,
            MemberLevelTransformations memberLevelTransformations )
        {
            Invariant.Assert( memberLevelTransformations.Parameters.Length == 0 );
            Invariant.Assert( memberLevelTransformations.Arguments.Length == 0 );

            var transformation = memberLevelTransformations.Expressions[0];

            if ( memberLevelTransformations.Expressions.Length == 1 )
            {
                if ( propertyDeclaration.Initializer != null )
                {
                    this._diagnostics.Report(
                        AspectLinkerDiagnosticDescriptors.CannotAssignToExpressionFromPrimaryConstructor.CreateRoslynDiagnostic(
                            propertyDeclaration.GetDiagnosticLocation(),
                            (propertyDeclaration.Identifier.ValueText, transformation.TargetMember.DeclaringType,
                             "The property already has an initializer.") ) );
                }

                if ( propertyDeclaration.ExpressionBody != null
                     || propertyDeclaration.AccessorList?.Accessors.Any( a => a.Body != null || a.ExpressionBody != null ) == true )
                {
                    this._diagnostics.Report(
                        AspectLinkerDiagnosticDescriptors.CannotAssignToExpressionFromPrimaryConstructor.CreateRoslynDiagnostic(
                            propertyDeclaration.GetDiagnosticLocation(),
                            (propertyDeclaration.Identifier.ValueText, transformation.TargetMember.DeclaringType, "Is is not an auto-property.") ) );
                }

                propertyDeclaration = propertyDeclaration
                    .PartialUpdate(
                        initializer: EqualsValueClause( memberLevelTransformations.Expressions[0].InitializerExpression ),
                        semicolonToken: Token( SyntaxKind.SemicolonToken ) );
            }
            else if ( memberLevelTransformations.Expressions.Length > 1 )
            {
                var aspects = memberLevelTransformations.Expressions.SelectAsArray( e => e.ParentAdvice.Aspect.ToString() );

                this._diagnostics.Report(
                    AspectLinkerDiagnosticDescriptors.CannotAssignToMemberMoreThanOnceFromPrimaryConstructor.CreateRoslynDiagnostic(
                        propertyDeclaration.GetDiagnosticLocation(),
                        (transformation.TargetMember.DeclarationKind, transformation.TargetMember, transformation.TargetMember.DeclaringType, aspects) ) );
            }

            return propertyDeclaration;
        }

        private ConstructorDeclarationSyntax ApplyMemberLevelTransformations(
            ConstructorDeclarationSyntax constructorDeclaration,
            MemberLevelTransformations memberLevelTransformations,
            SyntaxGenerationContext syntaxGenerationContext )
        {
            Invariant.Assert( memberLevelTransformations.Expressions.Length == 0 );

            constructorDeclaration = constructorDeclaration.WithParameterList(
                AppendParameters( constructorDeclaration.ParameterList, memberLevelTransformations.Parameters, syntaxGenerationContext ) );

            constructorDeclaration = constructorDeclaration.WithInitializer(
                this.AppendInitializerArguments( constructorDeclaration.Initializer, memberLevelTransformations.Arguments ) );

            return constructorDeclaration;
        }

        private void ApplyMemberLevelTransformationsToPrimaryConstructor(
            TypeDeclarationSyntax typeDeclaration,
            MemberLevelTransformations memberLevelTransformations,
            SyntaxGenerationContext syntaxGenerationContext,
            out BaseListSyntax? newBaseList,
            out ParameterListSyntax? newParameterList )
        {
            Invariant.AssertNot( typeDeclaration.BaseList == null && memberLevelTransformations.Arguments.Length > 0 );
            Invariant.AssertNotNull( typeDeclaration.GetParameterList() );

            newParameterList = AppendParameters( typeDeclaration.GetParameterList()!, memberLevelTransformations.Parameters, syntaxGenerationContext );
            newBaseList = typeDeclaration.BaseList;

            if ( memberLevelTransformations.Arguments.Length > 0 )
            {
                var semanticModel = this._semanticModelProvider.GetSemanticModel( typeDeclaration.SyntaxTree );
                var baseType = semanticModel.GetDeclaredSymbol( typeDeclaration );
                var baseTypeSyntax = typeDeclaration.BaseList.AssertNotNull().Types[0];

                BaseTypeSyntax newBaseTypeSyntax;

                switch ( baseTypeSyntax )
                {
                    case SimpleBaseTypeSyntax simpleBaseType:
                        newBaseTypeSyntax =
                            PrimaryConstructorBaseType(
                                simpleBaseType.Type,
                                ArgumentList( SeparatedList( memberLevelTransformations.Arguments.Select( x => x.ToSyntax() ) ) ) );

                        break;

                    case PrimaryConstructorBaseTypeSyntax primaryCtorBaseType:
                        newBaseTypeSyntax =
                            primaryCtorBaseType
                                .WithArgumentList(
                                    primaryCtorBaseType.ArgumentList.AddArguments( memberLevelTransformations.Arguments.SelectAsArray( x => x.ToSyntax() ) ) );

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected base type: {baseTypeSyntax.Kind()}" );
                }

                // TODO: This may be slower than replacing specific index.
                newBaseList = typeDeclaration.BaseList.ReplaceNode( baseTypeSyntax, newBaseTypeSyntax );
            }
        }

        private static ParameterListSyntax AppendParameters(
            ParameterListSyntax existingParameters,
            ImmutableArray<IntroduceParameterTransformation> newParameters,
            SyntaxGenerationContext syntaxGenerationContext )
        {
            if ( newParameters.IsEmpty )
            {
                return existingParameters;
            }
            else
            {
                if ( existingParameters.Parameters.Any( p => p.Modifiers.Any( m => m.IsKind( SyntaxKind.ParamsKeyword ) ) ) )
                {
                    return existingParameters.WithParameters(
                        existingParameters.Parameters.InsertRange(
                            existingParameters.Parameters.Count - 1,
                            newParameters.Select(
                                x => x.ToSyntax( syntaxGenerationContext )
                                    .WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ) ) ) );
                }
                else
                {
                    return existingParameters.WithParameters(
                        existingParameters.Parameters.AddRange(
                            newParameters.Select(
                                x => x.ToSyntax( syntaxGenerationContext )
                                    .WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ) ) ) );
                }
            }
        }

        private ConstructorInitializerSyntax? AppendInitializerArguments(
            ConstructorInitializerSyntax? initializerSyntax,
            ImmutableArray<IntroduceConstructorInitializerArgumentTransformation> newArguments )
        {
            if ( newArguments.IsEmpty )
            {
                return initializerSyntax;
            }

            var newArgumentsSyntax = newArguments.Select(
                a => a.ToSyntax().WithTrailingTriviaIfNecessary( ElasticSpace, this._syntaxGenerationContextFactory.Default.NormalizeWhitespace ) );

            if ( initializerSyntax == null )
            {
                return ConstructorInitializer(
                    SyntaxKind.BaseConstructorInitializer,
                    ArgumentList( SeparatedList( newArgumentsSyntax ) ) );
            }
            else
            {
                return initializerSyntax.WithArgumentList( initializerSyntax.ArgumentList.AddArguments( newArgumentsSyntax.ToArray() ) );
            }
        }

        private IReadOnlyList<FieldDeclarationSyntax> VisitFieldDeclarationCore( FieldDeclarationSyntax node )
        {
            var originalNode = node;

            if ( node.Declaration.Variables.Any( this._transformationCollection.HasMemberLevelTransformations ) )
            {
                node = node.ReplaceNodes(
                    node.Declaration.Variables,
                    ( variableDeclarator, _ ) =>
                    {
                        if ( this._transformationCollection.TryGetMemberLevelTransformations( variableDeclarator, out var memberLevelTransformations ) )
                        {
                            variableDeclarator = this.ApplyMemberLevelTransformations( variableDeclarator, memberLevelTransformations );
                        }

                        return variableDeclarator;
                    } );
            }

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( this._transformationCollection.IsNodeWithModifiedAttributes ) )
            {
                // TODO: This needs to use rewritten variable declaration or do removal in place.
                var members = new List<FieldDeclarationSyntax>( originalNode.Declaration.Variables.Count );

                // If we have changes in attributes and several members, we have to split them.
                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._transformationCollection.IsRemovedSyntax( variable ) )
                    {
                        continue;
                    }

                    var declaration = VariableDeclaration( node.Declaration.Type, SingletonSeparatedList( variable ) );
                    var attributes = this.RewriteDeclarationAttributeLists( variable, originalNode.AttributeLists, originalNode );

                    var fieldDeclaration = FieldDeclaration(
                        default,
                        node.Modifiers,
                        declaration,
                        Token( default, SyntaxKind.SemicolonToken, new( ElasticLineFeed ) ) );

                    fieldDeclaration = this.ReplaceAttributes( fieldDeclaration, attributes );

                    members.Add( fieldDeclaration );
                }

                return members;
            }
            else
            {
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists(
                    originalNode.Declaration.Variables[0],
                    originalNode.AttributeLists,
                    originalNode );

                node = this.ReplaceAttributes( node, rewrittenAttributes );

                var anyChangeToVariables = false;
                var rewrittenVariables = new List<VariableDeclaratorSyntax>();

                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._transformationCollection.IsRemovedSyntax( variable ) )
                    {
                        anyChangeToVariables = true;

                        continue;
                    }

                    rewrittenVariables.Add( variable );
                }

                if ( anyChangeToVariables )
                {
                    if ( rewrittenVariables.Count > 0 )
                    {
                        return new[] { node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( rewrittenVariables ) ) ) };
                    }
                    else
                    {
                        return Array.Empty<FieldDeclarationSyntax>();
                    }
                }
                else
                {
                    return new[] { node };
                }
            }
        }

        private EnumMemberDeclarationSyntax VisitEnumMemberDeclarationCore( EnumMemberDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private ConstructorDeclarationSyntax VisitConstructorDeclarationCore( ConstructorDeclarationSyntax node )
        {
            var originalNode = node;

            node = (ConstructorDeclarationSyntax) this.VisitConstructorDeclaration( node )!;

            if ( this._transformationCollection.TryGetMemberLevelTransformations( node, out var memberLevelTransformations ) )
            {
                var syntaxGenerationContext = this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( node );
                node = this.ApplyMemberLevelTransformations( node, memberLevelTransformations, syntaxGenerationContext );
            }

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null )
            {
                var declaration = (IMember) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedInitialStatements( declaration );

                node = (ConstructorDeclarationSyntax) InjectStatementsIntoMemberDeclaration( declaration, entryStatements, node );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private MethodDeclarationSyntax VisitMethodDeclarationCore( MethodDeclarationSyntax node )
        {
            var originalNode = node;

            node = (MethodDeclarationSyntax) this.VisitMethodDeclaration( node )!;

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null )
            {
                var declaration = (IMember) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedInitialStatements( declaration );

                node = (MethodDeclarationSyntax) InjectStatementsIntoMemberDeclaration( declaration, entryStatements, node );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private OperatorDeclarationSyntax VisitOperatorDeclarationCore( OperatorDeclarationSyntax node )
        {
            var originalNode = node;

            node = (OperatorDeclarationSyntax) this.VisitOperatorDeclaration( node )!;

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null )
            {
                var declaration = (IMember) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedInitialStatements( declaration );

                node = (OperatorDeclarationSyntax) InjectStatementsIntoMemberDeclaration( declaration, entryStatements, node );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        public override SyntaxNode VisitParameter( ParameterSyntax node )
        {
            var originalNode = node;
            node = (ParameterSyntax) base.VisitParameter( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );

            if ( rewrittenAttributes is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    node = node.WithAttributeLists( attributes );
                }
            }

            return node;
        }

        public override SyntaxNode VisitTypeParameter( TypeParameterSyntax node )
        {
            var originalNode = node;
            node = (TypeParameterSyntax) base.VisitTypeParameter( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );

            if ( rewrittenAttributes is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    node = node.WithAttributeLists( attributes );
                }
            }

            return node;
        }

        private PropertyDeclarationSyntax VisitPropertyDeclarationCore( PropertyDeclarationSyntax node )
        {
            var originalNode = node;

            if ( this._transformationCollection.TryGetMemberLevelTransformations( node, out var memberLevelTransformations ) )
            {
                node = this.ApplyMemberLevelTransformations( node, memberLevelTransformations );
            }

            node = (PropertyDeclarationSyntax) this.VisitPropertyDeclaration( node )!;

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null && symbol.SetMethod != null )
            {
                var declaration = (IProperty) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedInitialStatements( declaration.SetMethod.AssertNotNull() );

                node = (PropertyDeclarationSyntax) InjectStatementsIntoMemberDeclaration( declaration.SetMethod, entryStatements, node );
            }

            if ( this._transformationCollection.IsAutoPropertyWithSynthesizedSetter( originalNode ) )
            {
                node = node.WithSynthesizedSetter( this._syntaxGenerationContextFactory.Default );
            }

            if ( this._transformationCollection.GetAdditionalDeclarationFlags( originalNode ) is not AspectLinkerDeclarationFlags.None and var flags )
            {
                var existingFlags = node.GetLinkerDeclarationFlags();
                node = node.WithLinkerDeclarationFlags( existingFlags | flags );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        public override SyntaxNode VisitAccessorDeclaration( AccessorDeclarationSyntax node )
        {
            var originalNode = node;
            node = (AccessorDeclarationSyntax) base.VisitAccessorDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );

            if ( rewrittenAttributes is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    node = node.WithAttributeLists( attributes );
                }
            }

            return node;
        }

        private EventDeclarationSyntax VisitEventDeclarationCore( EventDeclarationSyntax node )
        {
            var originalNode = node;
            node = (EventDeclarationSyntax) this.VisitEventDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitEventFieldDeclarationCore( EventFieldDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( this._transformationCollection.IsNodeWithModifiedAttributes ) )
            {
                var members = new List<MemberDeclarationSyntax>( originalNode.Declaration.Variables.Count );

                // If we have changes in attributes and several members, we have to split them.
                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    var declaration = VariableDeclaration( node.Declaration.Type, SingletonSeparatedList( variable ) );

                    var attributes = this.RewriteDeclarationAttributeLists( variable, originalNode.AttributeLists, node );

                    var eventDeclaration = EventFieldDeclaration(
                        default,
                        node.Modifiers,
                        Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( Space ) ),
                        declaration,
                        Token( default, SyntaxKind.SemicolonToken, new( ElasticLineFeed ) ) );

                    eventDeclaration = this.ReplaceAttributes( eventDeclaration, attributes );

                    members.Add( eventDeclaration );
                }

                return members;
            }
            else
            {
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode.Declaration.Variables[0], originalNode.AttributeLists, node );
                node = this.ReplaceAttributes( node, rewrittenAttributes );

                return new[] { node };
            }
        }

        public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
        {
            SyntaxGenerationContext? syntaxGenerationContext = null;
            List<AttributeListSyntax> outputLists = new();
            List<SyntaxTrivia> outputTrivias = new();

            this.RewriteAttributeLists(
                this._compilation.ToTypedRef<IDeclaration>(),
                SyntaxKind.AssemblyKeyword,
                node,
                node.AttributeLists,
                ( _, n ) => n.SyntaxTree == this._syntaxTreeForGlobalAttributes,
                outputLists,
                outputTrivias,
                ref syntaxGenerationContext );

            return ((CompilationUnitSyntax) base.VisitCompilationUnit( node )!).WithAttributeLists( List( outputLists ) );
        }

        public override SyntaxNode? VisitPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node )
        {
            // Don't disable or restore warnings that have been suppressed in a parent scope.

            var remainingErrorCodes = node
                .ErrorCodes
                .Where( c => !this._activeSuppressions.Contains( GetErrorCode( c ) ) )
                .ToImmutableArray();

            if ( remainingErrorCodes.IsEmpty )
            {
                return null;
            }
            else
            {
                return node.WithErrorCodes( SeparatedList( remainingErrorCodes ) );
            }

            static string GetErrorCode( ExpressionSyntax expression )
            {
                return expression switch
                {
                    IdentifierNameSyntax identifier => identifier.Identifier.Text,
                    LiteralExpressionSyntax literal => $"CS{literal.Token.Value:0000}",
                    _ => throw new AssertionFailedException( $"Unexpected expression '{expression.Kind()}' at '{expression.GetLocation()}'." )
                };
            }
        }

        private SuppressionContext WithSuppressions( SyntaxNode node ) => new( this, this.GetSuppressions( node ) );

        private SuppressionContext WithSuppressions( IDeclaration declaration ) => new( this, this.GetSuppressions( declaration ) );
    }
}