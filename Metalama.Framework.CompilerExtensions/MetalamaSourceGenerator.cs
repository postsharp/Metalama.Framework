// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [Generator( LanguageNames.CSharp )]
    public class MetalamaSourceGenerator : ISourceGenerator
    {
        private readonly ISourceGenerator? _impl;

        public MetalamaSourceGenerator()
        {
            switch ( ProcessKindHelper.CurrentProcessKind )
            {
                case ProcessKind.Compiler:
                    // No implementation required.
                    break;

                case ProcessKind.DevEnv:
                    this._impl = (ISourceGenerator) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime.VisualStudio",
                        "Metalama.Framework.DesignTime.VisualStudio.VsUserProcessSourceGenerator" );

                    break;

                case ProcessKind.RoslynCodeAnalysisService:
                    this._impl = (ISourceGenerator) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime.VisualStudio",
                        "Metalama.Framework.DesignTime.VisualStudio.VsAnalysisProcessSourceGenerator" );

                    break;

                default:
                    this._impl = (ISourceGenerator) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime",
                        "Metalama.Framework.DesignTime.AnalysisProcessSourceGenerator" );

                    break;
            }
        }

        void ISourceGenerator.Execute( GeneratorExecutionContext context ) => this._impl?.Execute( context );

        void ISourceGenerator.Initialize( GeneratorInitializationContext context ) => this._impl?.Initialize( context );
    }
}