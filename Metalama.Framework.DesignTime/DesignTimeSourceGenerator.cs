﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="ISourceGenerator"/>. Provides the source code generated by the pipeline.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public  abstract partial class DesignTimeSourceGenerator : ISourceGenerator
    {
        private static readonly ProcessKind _processKind = DebuggingHelper.ProcessKind;
        private readonly ConcurrentDictionary<string, SourceGeneratorImpl> _generators = new();

        private bool _isEnabled;
        
        
        static DesignTimeSourceGenerator()
        {
            Logger.Initialize();
        }


        protected bool TryGetImpl( string projectId, [NotNullWhen( true )] out SourceGeneratorImpl? impl )
            => this._generators.TryGetValue( projectId, out impl );
        
        protected abstract SourceGeneratorImpl CreateSourceGeneratorImpl();
        
        void ISourceGenerator.Execute( GeneratorExecutionContext context )
        {
            if ( !this._isEnabled || context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                Logger.DesignTime.Trace?.Log(
                    $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )})." );

                var projectOptions = new ProjectOptions( context.AnalyzerConfigOptions );

                if ( !this._generators.TryGetValue( projectOptions.ProjectId, out var generator ) )
                {
                    generator = this._generators.GetOrAdd(
                        projectOptions.ProjectId,
                        _ => projectOptions.IsDesignTimeEnabled ? this.CreateSourceGeneratorImpl() : new OfflineSourceGeneratorImpl() );
                }

                generator.GenerateSources( projectOptions, compilation, context );
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context )
        {
            this._isEnabled = !MetalamaCompilerInfo.IsActive;

            // Start the remoting host or client.
            if ( _processKind == ProcessKind.RoslynCodeAnalysisService )
            {
               
            }
            else if ( _processKind == ProcessKind.DevEnv )
            {
              
            }
        }

    }
}