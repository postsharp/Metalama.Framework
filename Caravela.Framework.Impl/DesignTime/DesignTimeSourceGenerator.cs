// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
            var results = DesignTimeAspectPipelineCache.Instance.GetDesignTimeResults(
                compilation,
                new BuildOptions( context.AnalyzerConfigOptions ),
                context.CancellationToken );

            // Add introduced syntax trees.
            foreach ( var syntaxTreeResult in results.SyntaxTreeResults )
            {
                if ( syntaxTreeResult != null )
                {
                    foreach ( var additionalSyntaxTree in syntaxTreeResult.Introductions )
                    {
                        context.AddSource( additionalSyntaxTree.Name, additionalSyntaxTree.GeneratedSyntaxTree.GetText() );
                    }
                }
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context )
        {
            this._isEnabled = !CaravelaCompilerInfo.IsActive;
        }
    }
}