﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    [Generator]
    public class DesignTimeSourceGenerator : ISourceGenerator
    {
        private bool _isEnabled;

        void ISourceGenerator.Execute( GeneratorExecutionContext context )
        {
            if ( !this._isEnabled || context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            // Execute the pipeline.
            var pipelineResult = DesignTimeAspectPipelineCache.GetPipelineResult(
                PartialCompilation.CreateComplete( compilation ),
                new AnalyzerBuildOptionsSource( context.AnalyzerConfigOptions ),
                ImmutableArray<object>.Empty, 
                context.CancellationToken );

            // Add introduced syntax trees.
            if ( pipelineResult.AdditionalSyntaxTrees != null )
            {
                foreach ( var additionalSyntaxTree in pipelineResult.AdditionalSyntaxTrees )
                {
                    context.AddSource( additionalSyntaxTree.Key, additionalSyntaxTree.Value.GetText() );
                }
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context )
        {
            this._isEnabled = !CaravelaCompilerInfo.IsActive;
        }
    }
}