using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="SourceGeneratorPipelineStage"/> called from source generators.
    /// </summary>
    internal class SourceGeneratorPipelineStage : HighLevelPipelineStage
    {
        public SourceGeneratorPipelineStage( IReadOnlyList<OrderedAspectLayer> aspectLayers, CompileTimeAssemblyLoader assemblyLoader, IAspectPipelineProperties properties )
            : base( aspectLayers, assemblyLoader, properties )
        {
        }

        /// <inheritdoc/>
        protected override PipelineStageResult GenerateCode( PipelineStageResult input, IPipelineStepsResult pipelineStepResult )
        {
            var transformations = pipelineStepResult.Compilation.GetAllObservableTransformations();
            DiagnosticList diagnostics = new();

            var additionalSyntaxTrees = ImmutableDictionary.CreateBuilder<string, SyntaxTree>();

            foreach ( var transformationGroup in transformations )
            {
                if ( !(transformationGroup.DeclaringElement is INamedType declaringType) )
                {
                    // We only support introductions to types.
                    continue;
                }

                if ( !declaringType.IsPartial )
                {
                    // If the type is not marked as partial, we can emit a diagnostic and a code fix, but not a partial class itself.
                    // TODO: emit diagnostic.
                    continue;
                }

                var classDeclaration = SyntaxFactory.ClassDeclaration(
                    default,
                    SyntaxTokenList.Create( SyntaxFactory.Token( SyntaxKind.PartialKeyword ) ),
                    SyntaxFactory.Identifier( declaringType.Name ),
                    null,
                    null,
                    default,
                    default );

                foreach ( var transformation in transformationGroup.Transformations )
                {
                    switch ( transformation )
                    {
                        case IMemberIntroduction memberIntroduction:
                            classDeclaration = classDeclaration.AddMembers( memberIntroduction.GetIntroducedMembers( new MemberIntroductionContext( diagnostics ) ).Select( m => m.Syntax ).ToArray() );
                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }

                SyntaxNode topDeclaration = classDeclaration;

                if ( declaringType.Namespace != null )
                {

                    topDeclaration = SyntaxFactory.NamespaceDeclaration(
                                                SyntaxFactory.ParseName( declaringType.Namespace, 0, true ),
                                                default,
                                                default,
                                                SyntaxFactory.SingletonList<MemberDeclarationSyntax>( classDeclaration ) );
                }

                var syntaxTree = SyntaxFactory.SyntaxTree( topDeclaration );

                var syntaxTreeName = declaringType.FullName + ".cs";

                additionalSyntaxTrees.Add( syntaxTreeName, syntaxTree );
            }

            return new PipelineStageResult(
                input.Compilation,
                input.AspectLayers,
                input.Diagnostics.Concat( pipelineStepResult.Diagnostics ),
                Array.Empty<ResourceDescription>(),
                input.AspectSources.Concat( pipelineStepResult.ExternalAspectSources ),
                input.AdditionalSyntaxTrees.AddRange( additionalSyntaxTrees ) );
        }
    }
}