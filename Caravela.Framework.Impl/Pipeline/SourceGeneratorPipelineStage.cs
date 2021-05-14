// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="SourceGeneratorPipelineStage"/> called from source generators.
    /// </summary>
    internal class SourceGeneratorPipelineStage : HighLevelPipelineStage
    {
        public SourceGeneratorPipelineStage(
            CompileTimeProject compileTimeProject,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            IAspectPipelineProperties properties )
            : base( compileTimeProject, aspectLayers, properties ) { }

        /// <inheritdoc/>
        protected override PipelineStageResult GenerateCode( PipelineStageResult input, IPipelineStepsResult pipelineStepResult )
        {
            var transformations = pipelineStepResult.Compilation.GetAllObservableTransformations();
            DiagnosticSink diagnostics = new( this.CompileTimeProject );
            var syntaxFactory = ReflectionMapper.GetInstance( input.PartialCompilation.Compilation );

            var additionalSyntaxTrees = new List<IntroducedSyntaxTree>();

            LexicalScopeFactory lexicalScopeFactory = new( pipelineStepResult.Compilation );

            foreach ( var transformationGroup in transformations )
            {
                if ( transformationGroup.DeclaringElement is not INamedType declaringType )
                {
                    // We only support introductions to types.
                    continue;
                }

                /*
                if ( !declaringType.IsPartial )
                {
                    // If the type is not marked as partial, we can emit a diagnostic and a code fix, but not a partial class itself.
                    // TODO: emit diagnostic.
                    continue;
                }
                */

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
                foreach ( var transformation in transformationGroup.Transformations )
                {
                    switch ( transformation )
                    {
                        case IMemberIntroduction memberIntroduction:
                            // TODO: Provide other implementations or allow nulls (because this pipeline should not execute anything .
                            var introductionContext = new MemberIntroductionContext(
                                diagnostics,
                                new LinkerIntroductionNameProvider(),
                                lexicalScopeFactory.GetLexicalScope( memberIntroduction ),
                                syntaxFactory,
                                this.PipelineProperties.ServiceProvider );

                            classDeclaration = classDeclaration.AddMembers(
                                memberIntroduction.GetIntroducedMembers( introductionContext ).Select( m => m.Syntax ).ToArray() );

                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }

                // Add the class to a namespace.
                SyntaxNode topDeclaration = classDeclaration;

                if ( declaringType.Namespace != null )
                {
                    topDeclaration = SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName( declaringType.Namespace ),
                        default,
                        default,
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>( classDeclaration ) );
                }

                // Choose the best syntax tree
                var originalSyntaxTree = ((ICodeElementInternal) declaringType).DeclaringSyntaxReferences.Select( r => r.SyntaxTree )
                    .OrderBy( s => s.FilePath.Length )
                    .First();

                var generatedSyntaxTree = SyntaxFactory.SyntaxTree( topDeclaration.NormalizeWhitespace(), encoding: Encoding.UTF8 );
                var syntaxTreeName = declaringType.FullName + ".cs";

                additionalSyntaxTrees.Add( new IntroducedSyntaxTree( syntaxTreeName, originalSyntaxTree, generatedSyntaxTree ) );
            }

            return new PipelineStageResult(
                input.PartialCompilation,
                input.AspectLayers,
                input.Diagnostics.Concat( pipelineStepResult.Diagnostics ),
                Array.Empty<ResourceDescription>(),
                input.AspectSources.Concat( pipelineStepResult.ExternalAspectSources ),
                input.AdditionalSyntaxTrees.Concat( additionalSyntaxTrees ) );
        }
    }
}