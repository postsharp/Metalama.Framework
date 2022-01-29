// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Refactoring;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime
{
    // ReSharper disable UnusedType.Global

    [ExcludeFromCodeCoverage]
    public class CentralCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly ILogger _logger;
        private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

        public CentralCodeRefactoringProvider() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

        public CentralCodeRefactoringProvider( IServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeRefactoring" );
            this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        }

        public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
        {
            this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}')" );

            try
            {
                var projectOptions = new ProjectOptions( context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider );

                if ( !context.Document.SupportsSemanticModel )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): the document does not have a semantic model." );

                    return;
                }

                var cancellationToken = context.CancellationToken.IgnoreIfDebugging();

                var syntaxTree = await context.Document.GetSyntaxTreeAsync( cancellationToken );

                if ( syntaxTree == null )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): cannot get the syntax tree." );

                    return;
                }

                var node = (await syntaxTree.GetRootAsync( cancellationToken )).FindNode( context.Span );

                var semanticModel = await context.Document.GetSemanticModelAsync( cancellationToken );

                if ( semanticModel == null )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): cannot get the semantic model." );

                    return;
                }

                var symbol = semanticModel.GetDeclaredSymbol( node, cancellationToken );

                if ( symbol == null )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): cannot resolve the symbol." );

                    return;
                }

                // Execute the pipeline.

                var compilation = await context.Document.Project.GetCompilationAsync( cancellationToken );
                var eligibleAspects = this._pipelineFactory.GetEligibleAspects( compilation!, symbol, projectOptions, cancellationToken );

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
                                ct => this.ApplyLiveTemplateAsync( projectOptions, aspect, symbol, context.Document, ct.IgnoreIfDebugging() ) ) );
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
                AttributeHelper.GetShortName( aspect.AspectType.Name ),
                imports: ImmutableList.Create( aspect.AspectType.Namespace ) );

            return CSharpAttributeHelper.AddAttributeAsync( targetDocument, targetSymbol, attributeDescription, cancellationToken ).AsTask();
        }

        private async Task<Solution> ApplyLiveTemplateAsync(
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

            if ( this._pipelineFactory.TryApplyAspectToCode(
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