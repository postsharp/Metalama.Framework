// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
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
using System.Collections.Concurrent;
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
        private readonly SyntaxTransformationCollection _syntaxTransformationCollection;
        private readonly IReadOnlyDictionary<SyntaxNode, MemberLevelTransformations> _symbolMemberLevelTransformations;
        private readonly ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations> _introductionMemberLevelTransformations;
        private readonly IReadOnlyCollectionWithContains<SyntaxNode> _nodesWithModifiedAttributes;
        private readonly SyntaxTree _syntaxTreeForGlobalAttributes;

        // ReSharper disable once NotAccessedField.Local
#pragma warning disable IDE0052
        private readonly IReadOnlyDictionary<TypeDeclarationSyntax, TypeLevelTransformations> _typeLevelTransformations;
#pragma warning restore IDE0052

        // Maps a diagnostic id to the number of times it has been suppressed.
        private ImmutableHashSet<string> _activeSuppressions = ImmutableHashSet.Create<string>( StringComparer.OrdinalIgnoreCase );

        public Rewriter(
            CompilationContext compilationContext,
            SyntaxTransformationCollection syntaxTransformationCollection,
            ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> diagnosticSuppressions,
            CompilationModel compilation,
            IReadOnlyDictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations,
            ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations> introductionMemberLevelTransformations,
            IReadOnlyCollectionWithContains<SyntaxNode> nodesWithModifiedAttributes,
            SyntaxTree syntaxTreeForGlobalAttributes,
            IReadOnlyDictionary<TypeDeclarationSyntax, TypeLevelTransformations> typeLevelTransformations )
        {
            this._syntaxGenerationContextFactory = compilationContext.SyntaxGenerationContextFactory;
            this._diagnosticSuppressions = diagnosticSuppressions;
            this._compilation = compilation;
            this._syntaxTransformationCollection = syntaxTransformationCollection;
            this._semanticModelProvider = compilation.RoslynCompilation.GetSemanticModelProvider();
            this._symbolMemberLevelTransformations = symbolMemberLevelTransformations;
            this._introductionMemberLevelTransformations = introductionMemberLevelTransformations;
            this._nodesWithModifiedAttributes = nodesWithModifiedAttributes;
            this._syntaxTreeForGlobalAttributes = syntaxTreeForGlobalAttributes;
            this._typeLevelTransformations = typeLevelTransformations;
        }

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
        /// <param name="node"></param>
        /// <param name="suppressionsOnThisElement"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

                var disable = Trivia(
                        PragmaWarningDirectiveTrivia( Token( SyntaxKind.DisableKeyword ), true )
                            .WithErrorCodes( errorCodes )
                            .NormalizeWhitespace()
                            .WithLeadingTrivia( ElasticLineFeed )
                            .WithTrailingTrivia( ElasticLineFeed ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

                var restore =
                    Trivia(
                            PragmaWarningDirectiveTrivia( Token( SyntaxKind.RestoreKeyword ), true )
                                .WithErrorCodes( errorCodes )
                                .NormalizeWhitespace()
                                .WithLeadingTrivia( ElasticLineFeed )
                                .WithTrailingTrivia( ElasticLineFeed ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

                transformedNode =
                    transformedNode
                        .WithLeadingTrivia( node.GetLeadingTrivia().InsertRange( 0, new[] { ElasticLineFeed, disable, ElasticLineFeed } ) )
                        .WithTrailingTrivia( transformedNode.GetTrailingTrivia().AddRange( new[] { ElasticLineFeed, restore, ElasticLineFeed } ) );
            }

            return transformedNode;
        }

        private (SyntaxList<AttributeListSyntax> Attributes, SyntaxTriviaList Trivia)? RewriteDeclarationAttributeLists(
            SyntaxNode originalDeclaringNode,
            SyntaxList<AttributeListSyntax> attributeLists,
            SyntaxNode? originalNodeForTrivia = null )
        {
            if ( !this._nodesWithModifiedAttributes.Contains( originalDeclaringNode ) )
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
                return (default, TriviaList( outputTrivias ));
            }
            else
            {
                return (List( outputLists ), TriviaList( outputTrivias ));
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
                        .WithTrailingTrivia( ElasticLineFeed )
                        .WithAdditionalAnnotations(
                            attributeBuilder.ParentAdvice?.Aspect.AspectClass.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation );

                    if ( targetKind != SyntaxKind.None )
                    {
                        newList = newList.WithTarget( AttributeTargetSpecifier( Token( targetKind ) ) );
                    }

                    if ( outputTrivia.Any() && !outputAttributeLists.Any() )
                    {
                        newList = newList.WithLeadingTrivia( newList.GetLeadingTrivia().InsertRange( 0, outputTrivia ) );

                        outputTrivia.Clear();
                    }

                    outputAttributeLists.Add( newList );
                }
            }
        }

        private static T ReplaceAttributes<T>( T node, (SyntaxList<AttributeListSyntax> Attributes, SyntaxTriviaList Trivia)? attributesTuple )
            where T : MemberDeclarationSyntax
            => attributesTuple is var (attributes, trivia)
                ? (T) node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes )
                : node;

        public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node )
            => this.VisitTypeDeclaration(
                node,
                ( syntax, members ) =>
                {
                    // If the record has no braces, add them.
                    if ( syntax.OpenBraceToken.IsKind( SyntaxKind.None ) && members.Count > 0 )
                    {
                        // TODO: trivias.
                        syntax = syntax
                            .WithOpenBraceToken( Token( SyntaxKind.OpenBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                            .WithCloseBraceToken( Token( SyntaxKind.CloseBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                            .WithSemicolonToken( default );
                    }

                    return syntax.WithMembers( List( members ) );
                } );

        private SyntaxNode VisitTypeDeclaration<T>( T node, Func<T, List<MemberDeclarationSyntax>, T>? withMembers = null )
            where T : TypeDeclarationSyntax
        {
            var originalNode = node;
            var members = new List<MemberDeclarationSyntax>( node.Members.Count );
            var additionalBaseList = this._syntaxTransformationCollection.GetIntroducedInterfacesForTypeDeclaration( node );
            var syntaxGenerationContext = this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( node );

            var baseList = node.BaseList;

#if ROSLYN_4_8_0_OR_GREATER
            var parameterList = node.ParameterList;

            if ( this._symbolMemberLevelTransformations.TryGetValue( node, out var memberLevelTransformations ) )
            {
                this.ApplyMemberLevelTransformationsToPrimaryConstructor( node, memberLevelTransformations, syntaxGenerationContext, out baseList, out parameterList );
            }
#endif

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

                if ( withMembers != null )
                {
                    node = withMembers( node, members );
                }
                else
                {
                    node = (T) node.WithMembers( List( members ) );
                }

                // Process the type bases.
                if ( additionalBaseList.Any() )
                {
                    if ( baseList == null )
                    {
                        node = (T) node
                            .WithIdentifier( node.Identifier.WithTrailingTrivia() )
                            .WithBaseList(
                                BaseList( SeparatedList( additionalBaseList.SelectAsReadOnlyList( i => i.Syntax ) ) )
                                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                            .WithTrailingTrivia( node.Identifier.TrailingTrivia );
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

#if ROSLYN_4_8_0_OR_GREATER
                node = (T) node.WithParameterList( parameterList );
#endif

                // Rewrite attributes.
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
                node = ReplaceAttributes( node, rewrittenAttributes );

                return node;
            }

            // TODO: Try to avoid closure allocation.
            void AddInjectionsOnPosition( InsertPosition position )
            {
                var injectedMembersAtPosition = this._syntaxTransformationCollection.GetInjectedMembersOnPosition( position );

                foreach ( var injectedMember in injectedMembersAtPosition )
                {
                    // We should inject into a correct syntax tree.
                    Invariant.Assert( injectedMember.Transformation.TransformedSyntaxTree == originalNode.SyntaxTree );

                    // Allow for tracking of the node inserted.
                    // IMPORTANT: This need to be here and cannot be in injectedMember.Syntax, result of TrackNodes is not trackable!
                    var injectedNode = injectedMember.Syntax.TrackNodes( injectedMember.Syntax );

                    injectedNode = injectedNode
                        .WithLeadingTrivia( ElasticLineFeed, ElasticLineFeed )
                        .WithGeneratedCodeAnnotation( injectedMember.Transformation.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation )!;

                    // Insert inserted statements into 
                    switch ( injectedNode )
                    {
                        case ConstructorDeclarationSyntax constructorDeclaration:
                            if ( this._introductionMemberLevelTransformations.TryGetValue(
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

        private ConstructorDeclarationSyntax ApplyMemberLevelTransformations(
            ConstructorDeclarationSyntax constructorDeclaration,
            MemberLevelTransformations memberLevelTransformations,
            SyntaxGenerationContext syntaxGenerationContext )
        {
            constructorDeclaration = this.InsertStatements( constructorDeclaration, memberLevelTransformations.Statements );

            constructorDeclaration = constructorDeclaration.WithParameterList(
                AppendParameters( constructorDeclaration.ParameterList, memberLevelTransformations.Parameters, syntaxGenerationContext ) );

            constructorDeclaration = constructorDeclaration.WithInitializer(
                AppendInitializerArguments( constructorDeclaration.Initializer, memberLevelTransformations.Arguments ) );

            return constructorDeclaration;
        }

#if ROSLYN_4_8_0_OR_GREATER
        private void ApplyMemberLevelTransformationsToPrimaryConstructor(
            TypeDeclarationSyntax typeDeclaration,
            MemberLevelTransformations memberLevelTransformations,
            SyntaxGenerationContext syntaxGenerationContext,
            out BaseListSyntax? newBaseList,
            out ParameterListSyntax? newParameterList )
        {
            if (typeDeclaration is RecordDeclarationSyntax)
            {
                // Record declarations are currently handled differently.
                newBaseList = typeDeclaration.BaseList;
                newParameterList = typeDeclaration.ParameterList;
                return;
            }

            Invariant.AssertNot( memberLevelTransformations.Statements.Length > 0 );
            Invariant.AssertNot( typeDeclaration.BaseList == null && memberLevelTransformations.Arguments.Length > 0 );
            Invariant.AssertNotNull( typeDeclaration.ParameterList );

            newParameterList = AppendParameters( typeDeclaration.ParameterList, memberLevelTransformations.Parameters, syntaxGenerationContext );
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
                                ArgumentList( SeparatedList(
                                    memberLevelTransformations.Arguments.Select( x => x.ToSyntax() ) ) ) );

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
#endif

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
                return existingParameters.WithParameters(
                    existingParameters.Parameters.AddRange(
                        newParameters.Select( x => x.ToSyntax( syntaxGenerationContext ).WithTrailingTrivia( ElasticSpace ) ) ) );
            }
        }

        private static ConstructorInitializerSyntax? AppendInitializerArguments(
            ConstructorInitializerSyntax? initializerSyntax,
            ImmutableArray<IntroduceConstructorInitializerArgumentTransformation> newArguments )
        {
            if ( newArguments.IsEmpty )
            {
                return initializerSyntax;
            }

            var newArgumentsSyntax = newArguments.Select( a => a.ToSyntax().WithTrailingTrivia( ElasticSpace ) );

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

        private ConstructorDeclarationSyntax InsertStatements(
            ConstructorDeclarationSyntax constructorDeclaration,
            ImmutableArray<LinkerInsertedStatement> insertedStatements )
        {
            if ( insertedStatements.IsEmpty )
            {
                return constructorDeclaration;
            }

            // TODO: The order here is correct for initialization, i.e. first aspects (transformation order) are initialized first.
            //       This would not be, however, correct for other uses, but we don't have those.

            var beginningStatements = Order( insertedStatements )
                .Select( s => s.Statement );

            switch ( constructorDeclaration )
            {
                case { ExpressionBody: { } expressionBody }:
                    return
                        constructorDeclaration
                            .WithExpressionBody( null )
                            .WithSemicolonToken( default )
                            .WithBody(
                                SyntaxFactoryEx.FormattedBlock(
                                    beginningStatements
                                        .Append( ExpressionStatement( expressionBody.Expression.WithSourceCodeAnnotationIfNotGenerated() ) ) ) );

                case { Body: { } body }:
                    return
                        constructorDeclaration
                            .WithBody(
                                SyntaxFactoryEx.FormattedBlock(
                                    beginningStatements
                                        .Append(
                                            body.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                                                .WithSourceCodeAnnotationIfNotGenerated() ) ) );
            }

            return constructorDeclaration;

            IEnumerable<LinkerInsertedStatement> Order( IEnumerable<LinkerInsertedStatement> statements )
            {
                // TODO: This sort is intended only for beginning statements.
                var memberStatements = new Dictionary<IMember, List<LinkerInsertedStatement>>( this._compilation.Comparers.Default );
                var typeStatements = new List<LinkerInsertedStatement>();

                foreach ( var mark in statements )
                {
                    switch ( mark.ContextDeclaration )
                    {
                        case INamedType:
                            typeStatements.Add( mark );

                            break;

                        case IMember member:
                            if ( !memberStatements.TryGetValue( member, out var list ) )
                            {
                                memberStatements[member] = list = [];
                            }

                            list.Add( mark );

                            break;

                        default:
                            throw new AssertionFailedException( $"Unexpected declaration: '{mark.ContextDeclaration}'." );
                    }
                }

                // TODO: This sorting is suboptimal, but needed for stable order since we are using a dictionary.
                foreach ( var pair in memberStatements.OrderBy( p => p.Key.ToDisplayString() ) )
                {
                    foreach ( var mark in pair.Value )
                    {
                        yield return mark;
                    }
                }

                foreach ( var mark in typeStatements )
                {
                    yield return mark;
                }
            }
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitFieldDeclarationCore( FieldDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( this._nodesWithModifiedAttributes.Contains ) )
            {
                // TODO: This needs to use rewritten variable declaration or do removal in place.
                var members = new List<MemberDeclarationSyntax>( originalNode.Declaration.Variables.Count );

                // If we have changes in attributes and several members, we have to split them.
                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._syntaxTransformationCollection.IsRemovedSyntax( variable ) )
                    {
                        continue;
                    }

                    var declaration = VariableDeclaration( node.Declaration.Type, SingletonSeparatedList( variable ) );
                    var attributes = this.RewriteDeclarationAttributeLists( variable, originalNode.AttributeLists, originalNode );

                    var fieldDeclaration = FieldDeclaration( default, node.Modifiers, declaration, Token( SyntaxKind.SemicolonToken ) )
                        .WithTrailingTrivia( ElasticLineFeed );

                    fieldDeclaration = ReplaceAttributes( fieldDeclaration, attributes );

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

                node = ReplaceAttributes( node, rewrittenAttributes );

                var anyChangeToVariables = false;
                var rewrittenVariables = new List<VariableDeclaratorSyntax>();

                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._syntaxTransformationCollection.IsRemovedSyntax( variable ) )
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
                        return Array.Empty<MemberDeclarationSyntax>();
                    }
                }
                else
                {
                    return new[] { node };
                }
            }
        }

        private ConstructorDeclarationSyntax VisitConstructorDeclarationCore( ConstructorDeclarationSyntax node )
        {
            var originalNode = node;

            if ( this._symbolMemberLevelTransformations.TryGetValue( node, out var memberLevelTransformations ) )
            {
                var syntaxGenerationContext = this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( node );
                node = this.ApplyMemberLevelTransformations( node, memberLevelTransformations, syntaxGenerationContext );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

            return (ConstructorDeclarationSyntax) this.VisitConstructorDeclaration( node )!;
        }

        private MethodDeclarationSyntax VisitMethodDeclarationCore( MethodDeclarationSyntax node )
        {
            var originalNode = node;
            node = (MethodDeclarationSyntax) this.VisitMethodDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private OperatorDeclarationSyntax VisitOperatorDeclarationCore( OperatorDeclarationSyntax node )
        {
            var originalNode = node;
            node = (OperatorDeclarationSyntax) this.VisitOperatorDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

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
                node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
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
                node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
            }

            return node;
        }

        private PropertyDeclarationSyntax VisitPropertyDeclarationCore( PropertyDeclarationSyntax node )
        {
            var originalNode = node;

            node = (PropertyDeclarationSyntax) this.VisitPropertyDeclaration( node )!;

            if ( this._syntaxTransformationCollection.IsAutoPropertyWithSynthesizedSetter( originalNode ) )
            {
                node = node.WithSynthesizedSetter();
            }

            if ( this._syntaxTransformationCollection.GetAdditionalDeclarationFlags( originalNode ) is not AspectLinkerDeclarationFlags.None and var flags )
            {
                var existingFlags = node.GetLinkerDeclarationFlags();
                node = node.WithLinkerDeclarationFlags( existingFlags | flags );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

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
                node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
            }

            return node;
        }

        private EventDeclarationSyntax VisitEventDeclarationCore( EventDeclarationSyntax node )
        {
            var originalNode = node;
            node = (EventDeclarationSyntax) this.VisitEventDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitEventFieldDeclarationCore( EventFieldDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( this._nodesWithModifiedAttributes.Contains ) )
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
                            Token( SyntaxKind.SemicolonToken ) )
                        .WithTrailingTrivia( ElasticLineFeed );

                    eventDeclaration = ReplaceAttributes( eventDeclaration, attributes );

                    members.Add( eventDeclaration );
                }

                return members;
            }
            else
            {
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode.Declaration.Variables[0], originalNode.AttributeLists, node );
                node = ReplaceAttributes( node, rewrittenAttributes );

                return new[] { node };
            }
        }

        public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
        {
            SyntaxGenerationContext? syntaxGenerationContext = null;
            List<AttributeListSyntax> outputLists = [];
            List<SyntaxTrivia> outputTrivias = [];

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