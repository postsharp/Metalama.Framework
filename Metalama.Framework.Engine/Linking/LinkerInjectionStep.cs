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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MethodBase = Metalama.Framework.Engine.CodeModel.MethodBase;
using RefKind = Metalama.Framework.Code.RefKind;
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
            // TODO: Consider parallelizing based on containing type and not syntax tree. This would remove non-determinism in name selection.

            // We don't use a code fix filter because the linker is not supposed to suggest code fixes. If that changes, we need to pass a filter.
            var diagnostics = new UserDiagnosticSink( input.CompileTimeProject, null );

            var supportsNullability = input.CompilationModel.RoslynCompilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            var transformationComparer = TransformationLinkerOrderComparer.Instance;
            var injectionHelperProvider = new LinkerInjectionHelperProvider( input.CompilationModel, supportsNullability );
            var nameProvider = new LinkerInjectionNameProvider( input.CompilationModel, injectionHelperProvider, OurSyntaxGenerator.Default );
            var transformationCollection = new TransformationCollection( transformationComparer );
            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );
            var aspectReferenceSyntaxProvider = new LinkerAspectReferenceSyntaxProvider();

            HashSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations = new();
            HashSet<PropertyBuilder> buildersWithSynthesizedSetters = new();

            void IndexTransformationsInSyntaxTree( IGrouping<SyntaxTree, ITransformation> transformationGroup )
            {
                // Transformations need to be sorted here because some transformations require a LexicalScope to get an unique name, and it
                // will give deterministic results only when called in a deterministic order.
                var sortedTransformations = transformationGroup.OrderBy( x => x, transformationComparer ).ToArray();

                // IntroduceDeclarationTransformation instances need to be indexed first.
                foreach ( var transformation in sortedTransformations )
                {
                    IndexIntroduceDeclarationTransformation( transformation, transformationCollection );
                }

                // Replace transformations need to be indexed second.
                // NOTE: This is correct because replaced transformation is always in the same syntax tree as the replacing one.
                foreach ( var transformation in sortedTransformations )
                {
                    IndexReplaceTransformation(
                        input,
                        transformation,
                        transformationCollection,
                        replacedIntroduceDeclarationTransformations );
                }

                foreach ( var transformation in sortedTransformations )
                {
                    IndexOverrideTransformation(
                        transformation,
                        transformationCollection,
                        buildersWithSynthesizedSetters );

                    this.IndexInjectTransformation(
                        input,
                        transformation,
                        diagnostics,
                        lexicalScopeFactory,
                        nameProvider,
                        aspectReferenceSyntaxProvider,
                        buildersWithSynthesizedSetters,
                        transformationCollection,
                        replacedIntroduceDeclarationTransformations );

                    IndexMemberLevelTransformation(
                        input,
                        diagnostics,
                        lexicalScopeFactory,
                        transformation,
                        transformationCollection );

                    this.IndexInsertStatementTransformation(
                        input,
                        diagnostics,
                        lexicalScopeFactory,
                        transformation,
                        transformationCollection );

                    IndexNodesWithModifiedAttributes( transformation, transformationCollection );
                }
            }

            // It's imperative that order of transformations is preserved while grouping by syntax tree.
            // The syntax tree we group by must be the main syntax tree of the enclosing type. We should never run transformations
            // of a partial type in parallel.
            var transformationsByCanonicalSyntaxTree = input.Transformations.GroupBy( GetCanonicalSyntaxTree );

            static SyntaxTree GetCanonicalSyntaxTree( ITransformation transformation )
            {
                return GetCanonicalTargetDeclaration( transformation.TargetDeclaration ) switch
                {
                    INamedType namedType => namedType.GetPrimarySyntaxTree().AssertNotNull(),
                    ICompilation compilation => transformation.TransformedSyntaxTree,
                    var t => throw new AssertionFailedException( $"Unsupported: {t.DeclarationKind}" ),
                };

                static IDeclaration GetCanonicalTargetDeclaration(IDeclaration declaration)
                    => declaration switch
                       {
                           IMember member => member.DeclaringType,
                           INamedType type => type,
                           IParameter parameter => GetCanonicalTargetDeclaration(parameter.ContainingDeclaration.AssertNotNull()),
                           ICompilation compilation => compilation,
                           var t => throw new AssertionFailedException( $"Unsupported: {t.DeclarationKind}" ),
                       };
            }

            await this._concurrentTaskRunner.RunInParallelAsync( transformationsByCanonicalSyntaxTree, IndexTransformationsInSyntaxTree, cancellationToken );

            await transformationCollection.FinalizeAsync(
                transformationComparer,
                this._concurrentTaskRunner,
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
                    transformationCollection,
                    suppressionsByTarget,
                    input.CompilationModel,
                    syntaxTreeForGlobalAttributes,
                    diagnostics );

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
                transformationCollection.InjectedMembers,
                transformationCollection.BuilderToTransformationMap,
                transformationCollection.IntroducedParametersByTargetDeclaration,
                this._concurrentTaskRunner,
                cancellationToken );

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
            TransformationCollection transformationCollection )
        {
            // We only need to index transformations on syntax (i.e. on source code) because introductions on generated code
            // are taken from the compilation model.

            // Note: Compilation-level attributes will not be indexed because the containing declaration has no
            // syntax reference.

            if ( transformation is IntroduceAttributeTransformation introduceAttributeTransformation )
            {
                foreach ( var declaringSyntax in introduceAttributeTransformation.TargetDeclaration.GetDeclaringSyntaxReferences() )
                {
                    transformationCollection.AddNodeWithModifiedAttributes( declaringSyntax.GetSyntax() );
                }
            }
            else if ( transformation is RemoveAttributesTransformation removeAttributesTransformation )
            {
                foreach ( var declaringSyntax in removeAttributesTransformation.ContainingDeclaration.GetDeclaringSyntaxReferences() )
                {
                    transformationCollection.AddNodeWithModifiedAttributes( declaringSyntax.GetSyntax() );
                }
            }
        }

        private static void IndexIntroduceDeclarationTransformation( ITransformation transformation, TransformationCollection transformationCollection )
        {
            if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
            {
                transformationCollection.AddIntroduceTransformation(
                    introduceDeclarationTransformation.DeclarationBuilder,
                    introduceDeclarationTransformation );
            }
        }

        private static void IndexReplaceTransformation(
            AspectLinkerInput input,
            ITransformation transformation,
            TransformationCollection transformationCollection,
            HashSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
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
                    case Field replacedField:
                        var fieldSyntaxReference =
                            replacedField.Symbol.GetPrimarySyntaxReference()
                            ?? throw new AssertionFailedException( $"The field '{replacedField.Symbol}' does not have syntax." );

                        var removedFieldSyntax = fieldSyntaxReference.GetSyntax();
                        transformationCollection.AddRemovedSyntax( removedFieldSyntax );

                        break;

                    case Constructor replacedConstructor:
                        Invariant.Assert( replacedConstructor.Symbol.GetPrimarySyntaxReference() == null );

                        break;

                    // This needs to point to an interface
                    case IDeclarationBuilder replacedBuilder:
                        if ( !transformationCollection.TryGetIntroduceDeclarationTransformation( replacedBuilder, out var introduceDeclarationTransformation ) )
                        {
                            throw new AssertionFailedException( $"Builder {replacedBuilder} is missing registered transformation." );
                        }

                        lock ( replacedIntroduceDeclarationTransformations )
                        {
                            replacedIntroduceDeclarationTransformations.Add( introduceDeclarationTransformation );
                        }

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected replace declaration: '{replacedDeclaration}'." );
                }
            }
        }

        private void IndexInjectTransformation(
            AspectLinkerInput input,
            ITransformation transformation,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            LinkerInjectionNameProvider nameProvider,
            LinkerAspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            IReadOnlyCollection<PropertyBuilder> buildersWithSynthesizedSetters,
            TransformationCollection transformationCollection,
            HashSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
        {
            {
                if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
                {
                    lock ( replacedIntroduceDeclarationTransformations )
                    {
                        if ( replacedIntroduceDeclarationTransformations.Contains( introduceDeclarationTransformation ) )
                        {
                            return;
                        }
                    }
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
                        //       But, if we pass the mutable compilation, it will get changed before the template is expanded.
                        //       The expanded template should not see declarations added after it runs.

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

                        transformationCollection.AddInjectedMembers( injectMemberTransformation, injectedMembers );

                        break;

                    case IInjectInterfaceTransformation injectInterfaceTransformation:
                        var introducedInterface = injectInterfaceTransformation.GetSyntax();
                        transformationCollection.AddInjectedInterface( injectInterfaceTransformation, introducedInterface );

                        break;
                }

                IEnumerable<InjectedMember> PostProcessInjectedMembers( IEnumerable<InjectedMember> injectedMembers )
                {
                    if ( transformation is IntroducePropertyTransformation introducePropertyTransformation )
                    {
                        bool hasSynthesizedSetter;

                        lock ( buildersWithSynthesizedSetters )
                        {
                            hasSynthesizedSetter = buildersWithSynthesizedSetters.Contains( introducePropertyTransformation.IntroducedDeclaration );
                        }

                        if ( hasSynthesizedSetter )
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
                                                    return im.WithSyntax(
                                                        propertyDeclaration.WithSynthesizedSetter( this._compilationContext.DefaultSyntaxGenerationContext ) );

                                                case { Semantic: InjectedMemberSemantic.InitializerMethod }:
                                                    return im;

                                                default:
                                                    throw new AssertionFailedException( $"Unexpected semantic for '{im.Declaration}'." );
                                            }
                                        } );
                        }
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
            TransformationCollection transformationCollection,
            HashSet<PropertyBuilder> buildersWithSynthesizedSetters )
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
                        transformationCollection.AddAutoPropertyWithSynthesizedSetter(
                            (PropertyDeclarationSyntax) codeProperty.GetPrimaryDeclarationSyntax().AssertNotNull() );

                        break;

                    case BuiltProperty { PropertyBuilder: var builder }:
                        lock ( buildersWithSynthesizedSetters )
                        {
                            buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );
                        }

                        break;

                    case PropertyBuilder builder:
                        lock ( buildersWithSynthesizedSetters )
                        {
                            buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );
                        }

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected declaration: '{overriddenAutoProperty}'." );
                }
            }
        }

        private void IndexInsertStatementTransformation(
            AspectLinkerInput input,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            ITransformation transformation,
            TransformationCollection transformationCollection )
        {
            if ( transformation is not IInsertStatementTransformation insertStatementTransformation )
            {
                return;
            }

            switch ( insertStatementTransformation.TargetMember )
            {
                case Constructor { IsPrimary: true } primaryConstructor:
                    {
                        var primaryDeclaration = primaryConstructor.GetPrimaryDeclarationSyntax().AssertNotNull();

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext( primaryDeclaration );

                        var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                        // Convert each statement into a separate SetInitializerExpressionTransformation and index those.
                        ProcessStatements( insertedStatements.Select( s => s.Statement ) );

                        void ProcessStatements( IEnumerable<StatementSyntax> statements )
                        {
                            foreach ( var statement in statements )
                            {
                                if ( statement is BlockSyntax block )
                                {
                                    ProcessStatements( block.Statements );

                                    return;
                                }

                                if ( statement is not ExpressionStatementSyntax
                                    {
                                        Expression: AssignmentExpressionSyntax
                                        {
                                            RawKind: (int) SyntaxKind.SimpleAssignmentExpression,
                                            Left: var leftExpression,
                                            Right: var rightExpression
                                        }
                                    } )
                                {
                                    diagnostics.Report(
                                        AspectLinkerDiagnosticDescriptors.CannotAddStatementToPrimaryConstructor.CreateRoslynDiagnostic(
                                            primaryConstructor.DiagnosticLocation,
                                            (statement, primaryConstructor.DeclaringType) ) );

                                    break;
                                }

                                var identifier = leftExpression switch
                                {
                                    IdentifierNameSyntax identifierName => identifierName,
                                    MemberAccessExpressionSyntax
                                    {
                                        RawKind: (int) SyntaxKind.SimpleMemberAccessExpression,
                                        Expression: ThisExpressionSyntax,
                                        Name: IdentifierNameSyntax thisIdentifierName
                                    } => thisIdentifierName,
                                    _ => null
                                };

                                if ( identifier == null )
                                {
                                    diagnostics.Report(
                                        AspectLinkerDiagnosticDescriptors.CannotAssignToExpressionFromPrimaryConstructor.CreateRoslynDiagnostic(
                                            primaryConstructor.DiagnosticLocation,
                                            (leftExpression, primaryConstructor.DeclaringType,
                                             "Only the 'memberName' and 'this.memberName' forms are supported.") ) );

                                    break;
                                }

                                var memberName = identifier.Identifier.ValueText;
                                var fieldOrProperty = primaryConstructor.DeclaringType.FieldsAndProperties.OfName( memberName ).Single();

                                if ( fieldOrProperty.RefKind != RefKind.None )
                                {
                                    diagnostics.Report(
                                        AspectLinkerDiagnosticDescriptors.CannotAssignToExpressionFromPrimaryConstructor.CreateRoslynDiagnostic(
                                            primaryConstructor.DiagnosticLocation,
                                            (leftExpression, primaryConstructor.DeclaringType, "It is a ref member.") ) );

                                    break;
                                }

                                if ( fieldOrProperty.IsAutoPropertyOrField == false )
                                {
                                    diagnostics.Report(
                                        AspectLinkerDiagnosticDescriptors.CannotAssignToExpressionFromPrimaryConstructor.CreateRoslynDiagnostic(
                                            primaryConstructor.DiagnosticLocation,
                                            (leftExpression, primaryConstructor.DeclaringType, "It is not an auto-property.") ) );

                                    break;
                                }

                                IndexMemberLevelTransformation(
                                    input,
                                    diagnostics,
                                    lexicalScopeFactory,
                                    new SetInitializerExpressionTransformation( insertStatementTransformation.ParentAdvice, fieldOrProperty, rightExpression ),
                                    transformationCollection );
                            }
                        }

                        break;
                    }

                case MethodBase methodBase:
                    {
                        var primaryDeclaration = methodBase.GetPrimaryDeclarationSyntax().AssertNotNull();

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext( primaryDeclaration );

                        var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                        transformationCollection.AddInsertedStatements( methodBase, insertedStatements );

                        break;
                    }

                case IConstructor { } builtOrBuilderConstructor:
                    {
                        var constructorBuilder = insertStatementTransformation.TargetMember as ConstructorBuilder
                                                 ?? ((BuiltConstructor) insertStatementTransformation.TargetMember).ConstructorBuilder;

                        Invariant.Assert( !((IConstructor) constructorBuilder).IsPrimary );

                        var positionInSyntaxTree = GetSyntaxTreePosition( constructorBuilder.ToInsertPosition() );

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                            constructorBuilder.PrimarySyntaxTree.AssertNotNull(),
                            positionInSyntaxTree );

                        var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                        transformationCollection.AddInsertedStatements( constructorBuilder, insertedStatements );

                        break;
                    }

                case IMethod { } builtOrBuilderMethod:
                    {
                        var methodBuilder = insertStatementTransformation.TargetMember as MethodBuilder
                                            ?? (MethodBuilder) ((BuiltMethod) insertStatementTransformation.TargetMember).Builder;

                        var positionInSyntaxTree = GetSyntaxTreePosition( methodBuilder.ToInsertPosition() );

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                            methodBuilder.PrimarySyntaxTree.AssertNotNull(),
                            positionInSyntaxTree );

                        var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                        transformationCollection.AddInsertedStatements( methodBuilder, insertedStatements );

                        break;
                    }

                case Property property:
                    {
                        var primaryDeclaration = property.GetPrimaryDeclarationSyntax().AssertNotNull();

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext( primaryDeclaration );

                        var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                        transformationCollection.AddInsertedStatements( property, insertedStatements );

                        break;
                    }

                case IProperty { } builtOrBuilderProperty:
                    {
                        var propertyBuilder = insertStatementTransformation.TargetMember as PropertyBuilder
                                              ?? (PropertyBuilder) ((BuiltProperty) insertStatementTransformation.TargetMember).Builder;

                        var positionInSyntaxTree = GetSyntaxTreePosition( propertyBuilder.ToInsertPosition() );

                        var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                            propertyBuilder.PrimarySyntaxTree.AssertNotNull(),
                            positionInSyntaxTree );

                        var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                        transformationCollection.AddInsertedStatements( propertyBuilder, insertedStatements );

                        break;
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected target: {insertStatementTransformation.TargetMember}." );
            }

            IReadOnlyList<InsertedStatement> GetInsertedStatements(
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

        private static void IndexMemberLevelTransformation(
            AspectLinkerInput input,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            ITransformation transformation,
            TransformationCollection transformationCollection )
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
                memberLevelTransformations = transformationCollection.GetOrAddMemberLevelTransformations( declarationSyntax );
            }
            else
            {
                var parentDeclarationBuilder = (memberLevelTransformation.TargetMember as DeclarationBuilder
                                                ?? (memberLevelTransformation.TargetMember as BuiltDeclaration)?.Builder)
                    .AssertNotNull();

                memberLevelTransformations = transformationCollection.GetOrAddMemberLevelTransformations( parentDeclarationBuilder );
            }

            switch (transformation, memberLevelTransformation.TargetMember)
            {
                case (IntroduceParameterTransformation introduceParameterTransformation, _):
                    memberLevelTransformations.Add( introduceParameterTransformation );
                    transformationCollection.AddIntroducedParameter( introduceParameterTransformation );

                    break;

                case (IntroduceConstructorInitializerArgumentTransformation appendArgumentTransformation, _):
                    memberLevelTransformations.Add( appendArgumentTransformation );

                    break;

                case (SetInitializerExpressionTransformation setInitializerExpressionTransformation, _):
                    memberLevelTransformations.Add( setInitializerExpressionTransformation );

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected combination: ('{transformation}', '{memberLevelTransformation.TargetMember}')." );
            }
        }
    }
}