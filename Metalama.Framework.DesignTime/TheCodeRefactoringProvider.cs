// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime
{
    // ReSharper disable UnusedType.Global

    [ExcludeFromCodeCoverage]
    public class TheCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly ILogger _logger;
        private readonly ICodeActionDiscoveryService _codeActionDiscoveryService;
        private readonly ICodeActionExecutionService _codeActionExecutionService;

        public TheCodeRefactoringProvider() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

        public TheCodeRefactoringProvider( IServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeRefactoring" );
            this._codeActionDiscoveryService = serviceProvider.GetRequiredService<ICodeActionDiscoveryService>();
            this._codeActionExecutionService = serviceProvider.GetRequiredService<ICodeActionExecutionService>();
        }

        public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
        {
            this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}')" );

            try
            {
                var projectOptions = new ProjectOptions( context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider );

                if ( string.IsNullOrEmpty( projectOptions.ProjectId ) )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): not a Metalama project." );

                    return;
                }

                var result = await this._codeActionDiscoveryService.ComputeRefactoringsAsync(
                    projectOptions.ProjectId,
                    context.Document.FilePath!,
                    context.Span,
                    context.CancellationToken );

                if ( !result.CodeActions.IsDefaultOrEmpty )
                {
                    var invocationContext = new CodeActionInvocationContext(
                        this._codeActionExecutionService,
                        context.Document,
                        this._logger,
                        projectOptions.ProjectId );

                    foreach ( var actionModel in result.CodeActions )
                    {
                        foreach ( var action in actionModel.ToCodeActions( invocationContext ) )
                        {
                            context.RegisterRefactoring( action );
                        }
                    }
                }
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }
    }
}