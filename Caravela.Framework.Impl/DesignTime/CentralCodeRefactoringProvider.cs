// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.DesignTime.Refactoring;
using Caravela.Framework.Impl.DesignTime.Utilities;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiagnosticFormatter = Caravela.Framework.Impl.Diagnostics.DiagnosticFormatter;

namespace Caravela.Framework.Impl.DesignTime
{
    // ReSharper disable UnusedType.Global

    public class CentralCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
        {
            try
            {
                var buildOptions = new ProjectOptions( context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider );

                DebuggingHelper.AttachDebugger( buildOptions );
                
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
                                ct => ExpandAspectToCode( buildOptions, aspect, symbol, context.Document, ct.IgnoreIfDebugging() ) ) );
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
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        private static Task<Solution> AddAspectAttribute(
            AspectClass aspect,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            var attributeDescription = new AttributeDescription(
                aspect.AspectType.Name.TrimEnd( "Attribute" ),
                imports: ImmutableList.Create( aspect.AspectType.Namespace ) );

            return CSharpAttributeHelper.AddAttributeAsync( targetDocument, targetSymbol, attributeDescription, cancellationToken ).AsTask();
        }

        private static async Task<Solution> ExpandAspectToCode(
            ProjectOptions projectOptions,
            AspectClass aspect,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            var compilation = await targetDocument.Project.GetCompilationAsync( cancellationToken );

            if ( compilation == null )
            {
                return targetDocument.Project.Solution;
            }

            if ( DesignTimeAspectPipelineCache.Instance.TryApplyAspectToCode(
                projectOptions,
                aspect,
                compilation,
                targetSymbol,
                cancellationToken,
                out var outputCompilation,
                out var diagnostics ) )
            {
                var project = targetDocument.Project;
                var solution = project.Solution;

                foreach ( var modifiedSyntaxTree in outputCompilation.ModifiedSyntaxTrees )
                {
                    var document = project.GetDocument( modifiedSyntaxTree.Value.OldTree );

                    if ( document == null || !document.SupportsSyntaxTree )
                    {
                        continue;
                    }

                    var newSyntaxRoot = await modifiedSyntaxTree.Value.NewTree.GetRootAsync( cancellationToken );

                    var newDocument = document.WithSyntaxRoot( newSyntaxRoot );
                    var formattedSyntaxRoot = await OutputCodeFormatter.FormatAsync( newDocument, false, cancellationToken );

                    solution = solution.WithDocumentSyntaxRoot( document.Id, formattedSyntaxRoot );
                }

                return solution;
            }
            else
            {
                // How to report errors here? We will add a comment to the target symbol.
                var targetNode = await targetSymbol.GetPrimarySyntaxReference().AssertNotNull().GetSyntaxAsync( cancellationToken );

                var commentedNode = targetNode.WithLeadingTrivia(
                    diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error )
                        .SelectMany( d => new[] { SyntaxFactory.Comment( "// " + d.GetMessage( DiagnosticFormatter.Instance ) ), SyntaxFactory.LineFeed } ) );

                var newSyntaxRoot = (await targetDocument.GetSyntaxRootAsync( cancellationToken ))!.ReplaceNode( targetNode, commentedNode );

                var newDocument = targetDocument.WithSyntaxRoot( newSyntaxRoot );

                return newDocument.Project.Solution;
            }
        }
    }
}