// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
#if DEBUG
using Metalama.Framework.Engine.Formatting;
#endif

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Aspect linker injection steps. Adds introduced members from all transformation to the Roslyn compilation. This involves calling template expansion.
    /// This results in the transformation registry and intermediate compilation, and also produces diagnostics.
    /// </summary>
    internal sealed partial class LinkerInjectionStep : AspectLinkerPipelineStep<AspectLinkerInput, LinkerInjectionStepOutput>
    {
        private readonly ProjectServiceProvider _serviceProvider;
        private readonly CompilationContext _compilationContext;
        private readonly IConcurrentTaskRunner _concurrentTaskRunner;

        public LinkerInjectionStep( ProjectServiceProvider serviceProvider, CompilationContext compilationContext )
        {
            this._serviceProvider = serviceProvider;
            this._compilationContext = compilationContext;
            this._concurrentTaskRunner = serviceProvider.GetRequiredService<IConcurrentTaskRunner>();
        }

        public override async Task<LinkerInjectionStepOutput> ExecuteAsync( AspectLinkerInput input, CancellationToken cancellationToken )
        {
            // We don't use a code fix filter because the linker is not supposed to suggest code fixes. If that changes, we need to pass a filter.
            var diagnostics = new UserDiagnosticSink( input.CompileTimeProject, null );

            var supportsNullability = input.CompilationModel.RoslynCompilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            var transformationComparer = TransformationLinkerOrderComparer.Instance;
            var injectionHelperProvider = new LinkerInjectionHelperProvider( input.CompilationModel, supportsNullability );
            var nameProvider = new LinkerInjectionNameProvider( input.CompilationModel, injectionHelperProvider, OurSyntaxGenerator.Default );
            var syntaxTransformationCollection = new SyntaxTransformationCollection( transformationComparer );
            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );
            var aspectReferenceSyntaxProvider = new LinkerAspectReferenceSyntaxProvider();

            ConcurrentSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations = new();
            ConcurrentSet<PropertyBuilder> buildersWithSynthesizedSetters = new();
            ConcurrentDictionary<IMemberBuilder, ConcurrentLinkedList<AspectLinkerDeclarationFlags>> buildersWithAdditionalDeclarationFlags = new();
            ConcurrentDictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations = new();
            ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations> introductionMemberLevelTransformations = new();
            ConcurrentDictionary<TypeDeclarationSyntax, TypeLevelTransformations> typeLevelTransformations = new();
            ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> builderToTransformationMap = new();
            ConcurrentSet<SyntaxNode> nodesWithModifiedAttributes = new();

            void IndexTransformationsInSyntaxTree( IGrouping<SyntaxTree, ITransformation> transformationGroup )
            {
                // Transformations need to be sorted here because some transformations require a LexicalScope to get an unique name, and it
                // will give deterministic results only when called in a deterministic order.
                var sortedTransformations = transformationGroup.OrderBy( x => x, transformationComparer ).ToArray();

                // IntroduceDeclarationTransformation instances need to be indexed first.
                foreach ( var transformation in sortedTransformations )
                {
                    IndexIntroduceDeclarationTransformation( transformation, builderToTransformationMap );
                }

                // Replace transformations need to be indexed second.
                // NOTE: This is correct because replaced transformation is always in the same syntax tree as the replacing one.
                foreach ( var transformation in sortedTransformations )
                {
                    IndexReplaceTransformation(
                        input,
                        transformation,
                        syntaxTransformationCollection,
                        builderToTransformationMap,
                        replacedIntroduceDeclarationTransformations );
                }

                foreach ( var transformation in sortedTransformations )
                {
                    IndexOverrideTransformation(
                        transformation,
                        syntaxTransformationCollection,
                        buildersWithSynthesizedSetters );

                    this.IndexIntroduceTransformation(
                        input,
                        transformation,
                        diagnostics,
                        lexicalScopeFactory,
                        nameProvider,
                        aspectReferenceSyntaxProvider,
                        buildersWithSynthesizedSetters,
                        buildersWithAdditionalDeclarationFlags,
                        syntaxTransformationCollection,
                        replacedIntroduceDeclarationTransformations );

                    this.IndexMemberLevelTransformation(
                        input,
                        diagnostics,
                        lexicalScopeFactory,
                        transformation,
                        symbolMemberLevelTransformations,
                        introductionMemberLevelTransformations );

                    IndexNodesWithModifiedAttributes( transformation, nodesWithModifiedAttributes );
                }
            }

            var transformationsBySyntaxTree = input.Transformations.GroupBy( t => t.TransformedSyntaxTree );

            await this._concurrentTaskRunner.RunInParallelAsync( transformationsBySyntaxTree, IndexTransformationsInSyntaxTree, cancellationToken );

            await this._concurrentTaskRunner.RunInParallelAsync(
                introductionMemberLevelTransformations.Values,
                t => t.Sort( transformationComparer ),
                cancellationToken );

            await this._concurrentTaskRunner.RunInParallelAsync(
                symbolMemberLevelTransformations.Values,
                t => t.Sort( transformationComparer ),
                cancellationToken );

            var syntaxTreeForGlobalAttributes = input.CompilationModel.PartialCompilation.SyntaxTreeForCompilationLevelAttributes;

            // Group diagnostic suppressions by target.
            var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
                s => s.Declaration,
                input.CompilationModel.Comparers.Default );

            // Replace wildcard AssemblyVersionAttribute with actual version.
            var attributes = input.CompilationModel.GetAttributeCollection( input.CompilationModel.ToRef() );
            var assemblyVersionAttributeType = (INamedType) input.CompilationModel.Factory.GetTypeByReflectionType( typeof(AssemblyVersionAttribute) );
            var assemblyVersionAttribute = input.CompilationModel.Attributes.OfAttributeType( assemblyVersionAttributeType ).FirstOrDefault();

#pragma warning disable CA1307 // Specify StringComparison for clarity
            if ( assemblyVersionAttribute?.ConstructorArguments.FirstOrDefault() is { Value: string version }
                 && version.Contains( '*' ) )
            {
                attributes.Remove( assemblyVersionAttributeType );

                // It's hacky to add an AttributeBuilder with null Advice, but it seems to work fine.
                // We avoid to use user APIs that require a user code execution context.
                var assemblyVersionAttributeConstructor =
                    assemblyVersionAttributeType.Constructors.Single( x => x.Parameters is [{ Type.SpecialType: SpecialType.String }] );

                var newAssemblyVersionAttribute =
                    new StandaloneAttributeData( assemblyVersionAttributeConstructor )
                    {
                        ConstructorArguments = ImmutableArray.Create(
                            TypedConstant.Create(
                                input.CompilationModel.RoslynCompilation.Assembly.Identity.Version.ToString(),
                                assemblyVersionAttributeConstructor.Parameters[0].Type ) )
                    };

                attributes.Add(
                    new AttributeBuilder(
                        null!,
                        input.CompilationModel.DeclaringAssembly,
                        newAssemblyVersionAttribute ) );
            }
#pragma warning restore CA1307

            // Rewrite syntax trees.
            var intermediateCompilation = input.CompilationModel.PartialCompilation;
            var transformations = new ConcurrentBag<SyntaxTreeTransformation>();

            async Task RewriteSyntaxTreeAsync( SyntaxTree initialSyntaxTree )
            {
                Rewriter rewriter = new(
                    this._compilationContext,
                    syntaxTransformationCollection,
                    suppressionsByTarget,
                    input.CompilationModel,
                    symbolMemberLevelTransformations,
                    introductionMemberLevelTransformations,
                    nodesWithModifiedAttributes,
                    syntaxTreeForGlobalAttributes,
                    typeLevelTransformations );

                var oldRoot = await initialSyntaxTree.GetRootAsync( cancellationToken );
                var newRoot = rewriter.Visit( oldRoot ).AssertNotNull();

                if ( oldRoot != newRoot )
                {
                    var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                    transformations.Add( SyntaxTreeTransformation.ReplaceTree( initialSyntaxTree, intermediateSyntaxTree ) );
                }
            }

            await this._concurrentTaskRunner.RunInParallelAsync( intermediateCompilation.SyntaxTrees.Values, RewriteSyntaxTreeAsync, cancellationToken );

            var helperSyntaxTree = injectionHelperProvider.GetLinkerHelperSyntaxTree( intermediateCompilation.LanguageOptions );
            transformations.Add( SyntaxTreeTransformation.AddTree( helperSyntaxTree ) );

            intermediateCompilation = intermediateCompilation.Update( transformations );

            var injectionRegistry = new LinkerInjectionRegistry(
                transformationComparer,
                input.CompilationModel,
                intermediateCompilation,
                transformations,
                syntaxTransformationCollection.InjectedMembers,
                builderToTransformationMap );

            var projectOptions = this._serviceProvider.GetService<IProjectOptions>();

            return
                new LinkerInjectionStepOutput(
                    diagnostics,
                    input.CompilationModel,
                    intermediateCompilation,
                    injectionRegistry,
                    input.OrderedAspectLayers,
                    projectOptions );
        }

        private static void IndexNodesWithModifiedAttributes(
            ITransformation transformation,
            ConcurrentSet<SyntaxNode> nodesWithModifiedAttributes )
        {
            // We only need to index transformations on syntax (i.e. on source code) because introductions on generated code
            // are taken from the compilation model.

            // Note: Compilation-level attributes will not be indexed because the containing declaration has no
            // syntax reference.

            if ( transformation is IntroduceAttributeTransformation introduceAttributeTransformation )
            {
                foreach ( var declaringSyntax in introduceAttributeTransformation.TargetDeclaration.GetDeclaringSyntaxReferences() )
                {
                    nodesWithModifiedAttributes.Add( declaringSyntax.GetSyntax() );
                }
            }
            else if ( transformation is RemoveAttributesTransformation removeAttributesTransformation )
            {
                foreach ( var declaringSyntax in removeAttributesTransformation.ContainingDeclaration.GetDeclaringSyntaxReferences() )
                {
                    nodesWithModifiedAttributes.Add( declaringSyntax.GetSyntax() );
                }
            }
        }

        private static void IndexIntroduceDeclarationTransformation(
            ITransformation transformation,
            ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> builderToTransformationMap )
        {
            if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
            {
                builderToTransformationMap.TryAdd( introduceDeclarationTransformation.DeclarationBuilder, introduceDeclarationTransformation );
            }
        }

        private static void IndexReplaceTransformation(
            AspectLinkerInput input,
            ITransformation transformation,
            SyntaxTransformationCollection syntaxTransformationCollection,
            ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> builderToTransformationMap,
            ConcurrentSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
        {
            var compilation = input.CompilationModel;

            if ( transformation is not IReplaceMemberTransformation replaceMemberTransformation )
            {
                return;
            }

            {
                if ( replaceMemberTransformation.ReplacedMember.IsDefault )
                {
                    return;
                }

                // We want to get the replaced member as it is in the compilation of the transformation, i.e. with applied redirections up to that point.
                // TODO: the target may have been removed from the
                var replacedDeclaration = (IDeclaration) replaceMemberTransformation.ReplacedMember.GetTarget(
                    compilation,
                    ReferenceResolutionOptions.DoNotFollowRedirections );

                replacedDeclaration = replacedDeclaration switch
                {
                    BuiltDeclaration declaration => declaration.Builder,
                    _ => replacedDeclaration
                };

                switch ( replacedDeclaration )
                {
                    case Member replacedMember when replacedMember is Field or Property:
                        var memberSyntaxReference =
                            replacedMember.Symbol.GetPrimarySyntaxReference()
                            ?? throw new AssertionFailedException(
                                $"The {replacedMember.DeclarationKind.ToDisplayString()} '{replacedMember.Symbol}' does not have syntax." );

                        var removedMemberSyntax = memberSyntaxReference.GetSyntax();
                        syntaxTransformationCollection.AddRemovedSyntax( removedMemberSyntax );

                        break;

                    case Constructor replacedConstructor:
                        Invariant.Assert( replacedConstructor.Symbol.GetPrimarySyntaxReference() == null );

                        break;

                    // This needs to point to an interface
                    case IDeclarationBuilder replacedBuilder:
                        if ( !builderToTransformationMap.TryGetValue( replacedBuilder, out var introduceDeclarationTransformation ) )
                        {
                            throw new AssertionFailedException( $"Builder {replacedBuilder} is missing registered transformation." );
                        }

                        replacedIntroduceDeclarationTransformations.Add( introduceDeclarationTransformation );

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected replace declaration: '{replacedDeclaration}'." );
                }
            }
        }

        private void IndexIntroduceTransformation(
            AspectLinkerInput input,
            ITransformation transformation,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            LinkerInjectionNameProvider nameProvider,
            LinkerAspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            IReadOnlyCollection<PropertyBuilder> buildersWithSynthesizedSetters,
            ConcurrentDictionary<IMemberBuilder, ConcurrentLinkedList<AspectLinkerDeclarationFlags>> buildersWithAdditionalDeclarationFlags,
            SyntaxTransformationCollection syntaxTransformationCollection,
            ConcurrentSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
        {
            {
                if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation
                     && replacedIntroduceDeclarationTransformations.Contains( introduceDeclarationTransformation ) )
                {
                    return;
                }

                switch ( transformation )
                {
                    case IInjectMemberTransformation injectMemberTransformation:
                        // Transformed syntax tree must match insert position.
                        Invariant.Assert( injectMemberTransformation.TransformedSyntaxTree == injectMemberTransformation.InsertPosition.SyntaxNode.SyntaxTree );

                        // Create the SyntaxGenerationContext for the insertion point.
                        var positionInSyntaxTree = GetSyntaxTreePosition( injectMemberTransformation.InsertPosition );

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                            injectMemberTransformation.TransformedSyntaxTree,
                            positionInSyntaxTree );

                        // TODO: It smells that we pass original compilation here. Should be the compilation for the transformation.
                        //       For introduction, this should be a compilation that INCLUDES the builder.

                        // Call GetInjectedMembers
                        var injectionContext = new MemberInjectionContext(
                            this._serviceProvider,
                            diagnostics,
                            nameProvider,
                            aspectReferenceSyntaxProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            input.CompilationModel );

                        var injectedMembers = injectMemberTransformation.GetInjectedMembers( injectionContext );

                        injectedMembers = PostProcessInjectedMembers( injectedMembers );

                        syntaxTransformationCollection.Add( injectMemberTransformation, injectedMembers );

                        break;

                    case IInjectInterfaceTransformation injectInterfaceTransformation:
                        var introducedInterface = injectInterfaceTransformation.GetSyntax();
                        syntaxTransformationCollection.Add( injectInterfaceTransformation, introducedInterface );

                        break;
                }

                IEnumerable<InjectedMember> PostProcessInjectedMembers( IEnumerable<InjectedMember> injectedMembers )
                {
                    if ( transformation is IntroducePropertyTransformation introducePropertyTransformation
                         && buildersWithSynthesizedSetters.Contains( introducePropertyTransformation.IntroducedDeclaration ) )
                    {
                        // This is a property which should have a synthesized setter added.
                        injectedMembers =
                            injectedMembers
                                .Select(
                                    im =>
                                    {
                                        switch ( im )
                                        {
                                            // ReSharper disable once MissingIndent
                                            case
                                            {
                                                Semantic: InjectedMemberSemantic.Introduction, Kind: DeclarationKind.Property,
                                                Syntax: PropertyDeclarationSyntax propertyDeclaration
                                            }:
                                                return im.WithSyntax( propertyDeclaration.WithSynthesizedSetter() );

                                            case { Semantic: InjectedMemberSemantic.InitializerMethod }:
                                                return im;

                                            default:
                                                throw new AssertionFailedException( $"Unexpected semantic for '{im.Declaration}'." );
                                        }
                                    } );
                    }

                    if ( transformation is IIntroduceDeclarationTransformation introduceMemberTransformation
                         && buildersWithAdditionalDeclarationFlags.TryGetValue(
                             (IMemberBuilder) introduceMemberTransformation.DeclarationBuilder,
                             out var additionalFlagsList ) )
                    {
                        // This is a member builder that should have linker declaration flags added.
                        injectedMembers =
                            injectedMembers
                                .Select(
                                    im =>
                                    {
                                        var flags = im.Syntax.GetLinkerDeclarationFlags();

                                        foreach ( var additionalFlags in additionalFlagsList )
                                        {
                                            flags &= additionalFlags;
                                        }

                                        return
                                            im.WithSyntax( im.Syntax.WithLinkerDeclarationFlags( flags ) );
                                    } );
                    }

                    return injectedMembers;
                }
            }
        }

        private static int GetSyntaxTreePosition( InsertPosition insertPosition )
            => insertPosition.Relation switch
            {
                InsertPositionRelation.After => insertPosition.SyntaxNode.Span.End + 1,
                InsertPositionRelation.Within => ((BaseTypeDeclarationSyntax) insertPosition.SyntaxNode).CloseBraceToken.Span.Start - 1,
                _ => 0
            };

        private static void IndexOverrideTransformation(
            ITransformation transformation,
            SyntaxTransformationCollection syntaxTransformationCollection,
            ConcurrentSet<PropertyBuilder> buildersWithSynthesizedSetters )
        {
            if ( transformation is not IOverrideDeclarationTransformation overriddenDeclaration )
            {
                return;
            }

            // If this is an auto-property that does not override a base property, we can add synthesized init-only setter.
            // If this is overridden property we need to:
            //  1) Block inlining of the first override (force the trampoline).
            //  2) Substitute all sets of the property (can be only in constructors) to use the first override instead.
            if ( overriddenDeclaration.OverriddenDeclaration is IProperty
                {
                    IsAutoPropertyOrField: true, Writeability: Writeability.ConstructorOnly, SetMethod.IsImplicitlyDeclared: true,
                    OverriddenProperty: null or { SetMethod: not null }
                } overriddenAutoProperty )
            {
                switch ( overriddenAutoProperty )
                {
                    case Property codeProperty:
                        syntaxTransformationCollection.AddAutoPropertyWithSynthesizedSetter(
                            (PropertyDeclarationSyntax) codeProperty.GetPrimaryDeclarationSyntax().AssertNotNull() );

                        break;

                    case BuiltProperty { PropertyBuilder: var builder }:
                        buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );

                        break;

                    case PropertyBuilder builder:
                        buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected declaration: '{overriddenAutoProperty}'." );
                }
            }
        }

        private void IndexMemberLevelTransformation(
            AspectLinkerInput input,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            ITransformation transformation,
            ConcurrentDictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations,
            ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations> introductionMemberLevelTransformations )
        {
            if ( transformation is not IMemberLevelTransformation memberLevelTransformation )
            {
                return;
            }

            // TODO: Supports only constructors without overrides.
            //       Needs to be generalized for anything else (take into account overrides).

            MemberLevelTransformations? memberLevelTransformations;
            var declarationSyntax = memberLevelTransformation.TargetMember.GetPrimaryDeclarationSyntax();

            if ( declarationSyntax != null )
            {
                memberLevelTransformations = symbolMemberLevelTransformations.GetOrAddNew( declarationSyntax );
            }
            else
            {
                var parentDeclarationBuilder = (memberLevelTransformation.TargetMember as DeclarationBuilder
                                                ?? (memberLevelTransformation.TargetMember as BuiltDeclaration)?.Builder)
                    .AssertNotNull();

                memberLevelTransformations = introductionMemberLevelTransformations.GetOrAddNew( parentDeclarationBuilder );
            }

            switch (transformation, memberLevelTransformation.TargetMember)
            {
                case (IInsertStatementTransformation insertStatementTransformation, Constructor constructor):
                    {
                        var primaryDeclaration = constructor.GetPrimaryDeclarationSyntax().AssertNotNull();

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext( primaryDeclaration );

                        foreach ( var insertedStatement in GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext ) )
                        {
                            memberLevelTransformations.Add(
                                new LinkerInsertedStatement(
                                    transformation,
                                    insertedStatement.Statement,
                                    insertedStatement.ContextDeclaration ) );
                        }

                        break;
                    }

                case (IInsertStatementTransformation insertStatementTransformation, BuiltConstructor or ConstructorBuilder):
                    {
                        var constructorBuilder = memberLevelTransformation.TargetMember as ConstructorBuilder
                                                 ?? ((BuiltConstructor) memberLevelTransformation.TargetMember).ConstructorBuilder;

                        var positionInSyntaxTree = GetSyntaxTreePosition( constructorBuilder.ToInsertPosition() );

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                            constructorBuilder.PrimarySyntaxTree.AssertNotNull(),
                            positionInSyntaxTree );

                        foreach ( var insertedStatement in GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext ) )
                        {
                            memberLevelTransformations.Add(
                                new LinkerInsertedStatement(
                                    transformation,
                                    insertedStatement.Statement,
                                    insertedStatement.ContextDeclaration ) );
                        }

                        break;
                    }

                case (IntroduceParameterTransformation appendParameterTransformation, _):
                    memberLevelTransformations.Add( appendParameterTransformation );

                    break;

                case (IntroduceConstructorInitializerArgumentTransformation appendArgumentTransformation, _):
                    memberLevelTransformations.Add( appendArgumentTransformation );

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected combination: ('{transformation}', '{memberLevelTransformation.TargetMember}')." );
            }

            IEnumerable<InsertedStatement> GetInsertedStatements(
                IInsertStatementTransformation insertStatementTransformation,
                SyntaxGenerationContext syntaxGenerationContext )
            {
                var context = new InsertStatementTransformationContext(
                    this._serviceProvider,
                    diagnostics,
                    lexicalScopeFactory,
                    syntaxGenerationContext,
                    input.CompilationModel );

                var statements = insertStatementTransformation.GetInsertedStatements( context );
#if DEBUG
                statements = statements.ToMutableList();

                foreach ( var statement in statements )
                {
                    if ( statement.Statement is BlockSyntax block )
                    {
                        if ( !block.Statements.All( s => s.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) ) )
                        {
                            throw new AssertionFailedException( "GeneratedCodeAnnotationKind annotation missing." );
                        }
                    }
                    else
                    {
                        if ( !statement.Statement.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) )
                        {
                            throw new AssertionFailedException( "GeneratedCodeAnnotationKind annotation missing." );
                        }
                    }
                }
#endif

                return statements;
            }
        }
    }
}