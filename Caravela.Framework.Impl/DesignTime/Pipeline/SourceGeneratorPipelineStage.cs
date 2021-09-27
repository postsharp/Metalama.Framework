// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="SourceGeneratorPipelineStage"/> called from source generators.
    /// </summary>
    internal class SourceGeneratorPipelineStage : HighLevelPipelineStage
    {
        public SourceGeneratorPipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            IServiceProvider serviceProvider )
            : base( compileTimeProject, aspectLayers, serviceProvider ) { }

        /// <inheritdoc/>
        protected override PipelineStageResult GenerateCode(
            PipelineStageResult input,
            IPipelineStepsResult pipelineStepResult,
            CancellationToken cancellationToken )
        {
            var transformations = pipelineStepResult.Compilation.GetAllObservableTransformations();
            UserDiagnosticSink diagnostics = new( this.CompileTimeProject );

            var additionalSyntaxTrees = new List<IntroducedSyntaxTree>();

            LexicalScopeFactory lexicalScopeFactory = new( pipelineStepResult.Compilation );
            var introductionNameProvider = new LinkerIntroductionNameProvider();

            foreach ( var transformationGroup in transformations )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( transformationGroup.DeclaringDeclaration is not INamedType declaringType )
                {
                    // We only support introductions to types.
                    continue;
                }

                if ( !declaringType.IsPartial )
                {
                    // If the type is not marked as partial, we can emit a diagnostic and a code fix, but not a partial class itself.
                    diagnostics.Report(
                        DesignTimeDiagnosticDescriptors.TypeNotPartial.CreateDiagnostic( declaringType.GetDiagnosticLocation(), declaringType ) );

                    continue;
                }

                // TODO: support struct, record.

                // Create a class.
                var classDeclaration = SyntaxFactory.ClassDeclaration(
                    default,
                    SyntaxTokenList.Create( SyntaxFactory.Token( SyntaxKind.PartialKeyword ) ),
                    SyntaxFactory.Identifier( declaringType.Name ),
                    null,
                    null,
                    default,
                    default );

                // Add members to the class.
                var syntaxGenerationContext = SyntaxGenerationContext.CreateDefault( input.PartialCompilation.Compilation );

                foreach ( var transformation in transformationGroup.Transformations )
                {
                    if ( transformation is IMemberIntroduction memberIntroduction )
                    {
                        // TODO: Provide other implementations or allow nulls (because this pipeline should not execute anything).
                        var introductionContext = new MemberIntroductionContext(
                            diagnostics,
                            introductionNameProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            this.ServiceProvider );

                        var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext )
                            .Select( m => m.Syntax.NormalizeWhitespace() )
                            .ToArray();

                        classDeclaration = classDeclaration.AddMembers( introducedMembers );
                    }

                    if ( transformation is IIntroducedInterface interfaceImplementation )
                    {
                        classDeclaration = classDeclaration.AddBaseListTypes( interfaceImplementation.GetIntroducedInterfaceImplementations().ToArray() );
                    }
                }

                // Add the class to a namespace.
                SyntaxNode topDeclaration = classDeclaration;

                if ( !declaringType.Namespace.IsGlobalNamespace )
                {
                    topDeclaration = SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName( declaringType.Namespace.FullName ),
                        default,
                        default,
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>( classDeclaration ) );
                }

                // Choose the best syntax tree
                var originalSyntaxTree = ((IDeclarationImpl) declaringType).DeclaringSyntaxReferences.Select( r => r.SyntaxTree )
                    .OrderBy( s => s.FilePath.Length )
                    .First();

                var generatedSyntaxTree = SyntaxFactory.SyntaxTree( topDeclaration.NormalizeWhitespace(), encoding: Encoding.UTF8 );
                var syntaxTreeName = declaringType.FullName + ".cs";

                additionalSyntaxTrees.Add( new IntroducedSyntaxTree( syntaxTreeName, originalSyntaxTree, generatedSyntaxTree ) );
            }

            return new PipelineStageResult(
                input.PartialCompilation,
                input.Project,
                input.AspectLayers,
                input.Diagnostics.Concat( pipelineStepResult.Diagnostics ).Concat( diagnostics.ToImmutable() ),
                input.AspectSources.Concat( pipelineStepResult.ExternalAspectSources ),
                input.AdditionalSyntaxTrees.Concat( additionalSyntaxTrees ) );
        }
    }
}