// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CodeModel.References;
using Metalama.Framework.Impl.DesignTime.CodeFixes;
using Metalama.Framework.Impl.DesignTime.Pipeline;
using Metalama.Framework.Impl.DesignTime.Refactoring;
using Metalama.Framework.Impl.DesignTime.Utilities;
using Metalama.Framework.Impl.Options;
using Metalama.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Impl.DesignTime
{
    // ReSharper disable UnusedType.Global

    [ExcludeFromCodeCoverage]
    public class CentralCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
        {
            try
            {
                var projectOptions = new ProjectOptions( context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider );

                DebuggingHelper.AttachDebugger( projectOptions );

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

                var symbol = semanticModel.GetDeclaredSymbol( node, cancellationToken );

                if ( symbol == null )
                {
                    return;
                }

                // Execute the pipeline.

                var compilation = await context.Document.Project.GetCompilationAsync( cancellationToken );
                var eligibleAspects = DesignTimeAspectPipelineFactory.Instance.GetEligibleAspects( compilation!, symbol, projectOptions, cancellationToken );

                var aspectActions = new CodeActionMenuModel( "Add aspect" );
                var liveTemplatesActions = new CodeActionMenuModel( "Apply live template" );

                foreach ( var aspect in eligibleAspects )
                {
                    aspectActions.Items.Add( new CodeActionModel( aspect.DisplayName, ct => AddAspectAttributeAsync( aspect, symbol, context.Document, ct ) ) );

                    if ( aspect.IsLiveTemplate )
                    {
                        liveTemplatesActions.Items.Add(
                            new CodeActionModel(
                                aspect.DisplayName,
                                ct => ApplyLiveTemplateAsync( projectOptions, aspect, symbol, context.Document, ct.IgnoreIfDebugging() ) ) );
                    }
                }

                var supportsHierarchicalItems = HostProcess.Current.Product != HostProduct.Rider;

                foreach ( var action in aspectActions.ToCodeActions( supportsHierarchicalItems ) )
                {
                    context.RegisterRefactoring( action );
                }

                foreach ( var action in liveTemplatesActions.ToCodeActions( supportsHierarchicalItems ) )
                {
                    context.RegisterRefactoring( action );
                }
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        private static Task<Solution> AddAspectAttributeAsync(
            AspectClass aspect,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            var attributeDescription = new AttributeDescription(
                AttributeRef.GetShortName( aspect.AspectType.Name ),
                imports: ImmutableList.Create( aspect.AspectType.Namespace ) );

            return CSharpAttributeHelper.AddAttributeAsync( targetDocument, targetSymbol, attributeDescription, cancellationToken ).AsTask();
        }

        private static async Task<Solution> ApplyLiveTemplateAsync(
            ProjectOptions projectOptions,
            AspectClass aspectClass,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            var compilation = await targetDocument.Project.GetCompilationAsync( cancellationToken );

            if ( compilation == null )
            {
                return targetDocument.Project.Solution;
            }

            if ( DesignTimeAspectPipelineFactory.Instance.TryApplyAspectToCode(
                projectOptions,
                aspectClass,
                aspectClass.CreateDefaultInstance(),
                compilation,
                targetSymbol,
                cancellationToken,
                out var outputCompilation,
                out var diagnostics ) )
            {
                var project = targetDocument.Project;

                var solution = await CodeFixHelper.ApplyChangesAsync( outputCompilation, project, cancellationToken );

                return solution;
            }
            else
            {
                // How to report errors here? We will add a comment to the target symbol.
                return await CodeFixHelper.ReportDiagnosticsAsCommentsAsync( targetSymbol, targetDocument, diagnostics, cancellationToken );
            }
        }
    }
}