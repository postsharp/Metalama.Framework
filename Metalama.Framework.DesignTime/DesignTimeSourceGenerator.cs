﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="ISourceGenerator"/>. Provides the source code generated by the pipeline.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract partial class DesignTimeSourceGenerator : ISourceGenerator
    {
        private readonly ConcurrentDictionary<string, SourceGeneratorImpl?> _generators = new();

        static DesignTimeSourceGenerator()
        {
            Logger.Initialize();
        }

        protected bool TryGetImpl( string projectId, [NotNullWhen( true )] out SourceGeneratorImpl? impl )
            => this._generators.TryGetValue( projectId, out impl );

        protected abstract SourceGeneratorImpl CreateSourceGeneratorImpl( IProjectOptions projectOptions );

        void ISourceGenerator.Execute( GeneratorExecutionContext context )
        {
            if ( context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                Logger.DesignTime.Trace?.Log(
                    $"{this.GetType().Name}.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )})." );

                var projectOptions = new ProjectOptions( context.AnalyzerConfigOptions );

                if ( !this._generators.TryGetValue( projectOptions.ProjectId, out var generator ) )
                {
                    generator = this._generators.GetOrAdd(
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
                                    return new OfflineSourceGeneratorImpl( projectOptions );
                                }
                            }
                            else
                            {
                                return null;
                            }
                        } );
                }

                generator?.GenerateSources( compilation, context );
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context ) { }
    }
}