// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime
{
    // ReSharper disable UnusedType.Global

    /// <summary>
    /// Our implementation of <see cref="CodeRefactoringProvider"/>. All the work is essentially delegated to the implementation
    /// of the <see cref="ICodeRefactoringDiscoveryService"/> and <see cref="ICodeActionExecutionService"/> services, which
    /// run in the analysis process. The current implementation only wraps these interfaces for Visual Studio.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TheCodeRefactoringProvider : CodeRefactoringProvider
    {
        static TheCodeRefactoringProvider()
        {
            DesignTimeServices.Initialize();
        }

        private readonly ILogger _logger;
        private readonly ICodeRefactoringDiscoveryService _codeRefactoringDiscoveryService;
        private readonly ICodeActionExecutionService _codeActionExecutionService;

        public TheCodeRefactoringProvider() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

        public TheCodeRefactoringProvider( IServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeRefactoring" );
            this._codeRefactoringDiscoveryService = serviceProvider.GetRequiredService<ICodeRefactoringDiscoveryService>();
            this._codeActionExecutionService = serviceProvider.GetRequiredService<ICodeActionExecutionService>();
        }

        public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
        {
            this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}')" );

            try
            {
                var projectOptions = new MSBuildProjectOptions( context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider );

                if ( string.IsNullOrEmpty( projectOptions.ProjectId ) )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): not a Metalama project." );

                    return;
                }

                if ( !context.Document.SupportsSemanticModel )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): no semantic model." );

                    return;
                }

                // Find the declaring node.
                var syntaxRoot = await context.Document.GetSyntaxRootAsync( context.CancellationToken );

                if ( syntaxRoot == null )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): no syntax root." );

                    return;
                }

                var node = syntaxRoot.FindNode( context.Span );

                // Do not provide refactorings on the method body, only on the declaration.
                if ( node.AncestorsAndSelf().Any( x => x is ExpressionSyntax or StatementSyntax ) )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): caret is in a method body or expression body ({node.Kind()})." );

                    return;
                }

                // Do not attempt a remote call if we cannot get the declared symbol.
                var semanticModel = await context.Document.GetSemanticModelAsync( context.CancellationToken );

                if ( semanticModel == null )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): no semantic model." );

                    return;
                }

                // Get the symbol.
                var declaredSymbol = semanticModel.GetDeclaredSymbol( node );

                if ( declaredSymbol == null )
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): no symbol for node '{node.Kind()}'." );

                    return;
                }

                this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): we are on symbol '{declaredSymbol}'." );

                // Call the service.
                var result = await this._codeRefactoringDiscoveryService.ComputeRefactoringsAsync(
                    projectOptions.ProjectId,
                    context.Document.FilePath!,
                    context.Span,
                    context.CancellationToken );

                // Translate the model objects into VS refactorings.
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
                            this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): registering '{action.Title}'." );

                            context.RegisterRefactoring( action );
                        }
                    }
                }
                else
                {
                    this._logger.Trace?.Log( $"ComputeRefactorings('{context.Document.Name}'): no refactoring available." );
                }
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }
    }
}