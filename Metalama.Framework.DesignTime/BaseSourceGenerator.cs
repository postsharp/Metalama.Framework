// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime
{
    /// <summary>
    /// Our base implementation of <see cref="ISourceGenerator"/>, which essentially delegates the work to a <see cref="ProjectHandler"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract partial class BaseSourceGenerator : ISourceGenerator
    {
        protected ServiceProvider ServiceProvider { get; }

        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ProjectHandler?> _projectHandlers = new();

        protected BaseSourceGenerator( ServiceProvider serviceProvider )
        {
            this.ServiceProvider = serviceProvider;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        }

        protected abstract ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions );

        void ISourceGenerator.Execute( GeneratorExecutionContext context )
        {
            if ( context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                this._logger.Trace?.Log(
                    $"{this.GetType().Name}.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )})." );

                var projectOptions = new ProjectOptions( context.AnalyzerConfigOptions );

                if ( string.IsNullOrEmpty( projectOptions.ProjectId ) )
                {
                    // Metalama is not enabled for this project.

                    return;
                }

                // Get or create an IProjectHandler instance.
                if ( !this._projectHandlers.TryGetValue( projectOptions.ProjectId, out var projectHandler ) )
                {
                    projectHandler = this._projectHandlers.GetOrAdd(
                        projectOptions.ProjectId,
                        _ =>
                        {
                            if ( projectOptions.IsFrameworkEnabled )
                            {
                                if ( projectOptions.IsDesignTimeEnabled )
                                {
                                    return this.CreateSourceGeneratorImpl( projectOptions );
                                }
                                else
                                {
                                    return new OfflineProjectHandler( this.ServiceProvider, projectOptions );
                                }
                            }
                            else
                            {
                                return null;
                            }
                        } );
                }

                // Invoke GenerateSources for the project handler.
                projectHandler?.GenerateSources( compilation, context );
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context ) { }
    }
}