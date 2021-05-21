// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.DesignTime.Refactoring;
using Caravela.Framework.Impl.DesignTime.Utilities;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime
{
    // ReSharper disable UnusedType.Global

    public class CentralCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
        {
            if ( !context.Document.SupportsSemanticModel )
            {
                return;
            }

            var cancellationToken = context.CancellationToken.IgnoreIfDebugging();

            var syntaxTree = await context.Document.GetSyntaxTreeAsync( cancellationToken );

            if ( syntaxTree == null )
            {
                return;
            }

            var node = (await syntaxTree.GetRootAsync( cancellationToken )).FindNode( context.Span );

            var semanticModel = await context.Document.GetSemanticModelAsync( cancellationToken );

            if ( semanticModel == null )
            {
                return;
            }

            var compilation = semanticModel.Compilation;

            var symbol = semanticModel.GetDeclaredSymbol( node, cancellationToken );

            if ( symbol == null )
            {
                return;
            }

            var buildOptions = new ProjectOptions( context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider );

            DebuggingHelper.AttachDebugger( buildOptions );

            // TODO: Make sure we are on a background thread.

            // Execute the pipeline.

            var eligibleAspects = DesignTimeAspectPipelineCache.Instance.GetEligibleAspects( symbol, buildOptions, cancellationToken );

            var addAspectAttributeActions = ImmutableArray.CreateBuilder<CodeAction>();
            var expandAspectActions = ImmutableArray.CreateBuilder<CodeAction>();

            foreach ( var aspect in eligibleAspects )
            {
                addAspectAttributeActions.Add( CodeAction.Create( aspect.DisplayName, ct => AddAspectAttribute( aspect, symbol, context.Document, ct ) ) );

                if ( aspect.CanExpandToSource )
                {
                    expandAspectActions.Add(
                        CodeAction.Create(
                            aspect.DisplayName,
                            ct => ExpandAspectToCode( buildOptions, compilation, aspect, symbol, context.Document, ct.IgnoreIfDebugging() ) ) );
                }
            }

            if ( addAspectAttributeActions.Count > 0 )
            {
                context.RegisterRefactoring( CodeAction.Create( "Add aspect as attribute", addAspectAttributeActions.ToImmutable(), true ) );
            }

            if ( expandAspectActions.Count > 0 )
            {
                context.RegisterRefactoring( CodeAction.Create( "Expand aspect", expandAspectActions.ToImmutable(), false ) );
            }
        }

        private static Task<Solution> AddAspectAttribute(
            AspectClassMetadata aspect,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            var attributeDescription = new AttributeDescription(
                aspect.AspectType.Name.TrimEnd( "Attribute" ),
                imports: ImmutableList.Create( aspect.AspectType.Namespace ) );

            return CSharpAttributeHelper.AddAttributeAsync( targetDocument, targetSymbol, attributeDescription, cancellationToken ).AsTask();
        }

        private static Task<Solution> ExpandAspectToCode(
            ProjectOptions projectOptions,
            Compilation compilation,
            AspectClassMetadata aspect,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            if ( DesignTimeAspectPipelineCache.Instance.TryApplyAspectToCode(
                projectOptions,
                aspect,
                compilation,
                targetSymbol,
                cancellationToken,
                out var outputCompilation ) )
            {
                var project = targetDocument.Project;
                var solution = project.Solution;

                foreach ( var document in project.Documents )
                {
                    if ( document.SupportsSyntaxTree && document.TryGetSyntaxTree( out var oldSyntaxTree )
                                                     && !outputCompilation.ContainsSyntaxTree( oldSyntaxTree ) )
                    {
                        var newSyntaxTree = outputCompilation.SyntaxTrees.Single( t => t.FilePath == document.FilePath );
                        solution = solution.WithDocumentText( document.Id, newSyntaxTree.GetText() );
                    }
                }

                return Task.FromResult( solution );
            }
            else
            {
                // TODO: How to report an error in this case?
                return Task.FromResult( targetDocument.Project.Solution );
            }
        }
    }
}