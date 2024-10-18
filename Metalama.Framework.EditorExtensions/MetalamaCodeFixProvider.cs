// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace Metalama.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [ExportCodeFixProvider( LanguageNames.CSharp, Name = nameof(MetalamaCodeFixProvider) )]
    [Shared]
    public class MetalamaCodeFixProvider : CodeFixProvider
    {
        private readonly CodeFixProvider? _impl;

        public MetalamaCodeFixProvider()
        {
            switch ( ProcessKindHelper.CurrentProcessKind )
            {
                case ProcessKind.Compiler:
                    break;

                case ProcessKind.RoslynCodeAnalysisService:
                case ProcessKind.DevEnv:
                    this._impl = (CodeFixProvider) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime.VisualStudio",
                        "Metalama.Framework.DesignTime.VisualStudio.VsCodeFixProvider" );

                    break;

                case ProcessKind.Rider:
                    this._impl = (CodeFixProvider) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime",
                        "Metalama.Framework.DesignTime.Rider.RiderCodeFixProvider" );

                    break;

                default:
                    this._impl = (CodeFixProvider) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime",
                        "Metalama.Framework.DesignTime.TheCodeFixProvider" );

                    break;
            }
        }

#pragma warning disable VSTHRD110
        public override Task RegisterCodeFixesAsync( CodeFixContext context ) => this._impl?.RegisterCodeFixesAsync( context ) ?? Task.CompletedTask;
#pragma warning restore VSTHRD110

        public override ImmutableArray<string> FixableDiagnosticIds => this._impl?.FixableDiagnosticIds ?? ImmutableArray<string>.Empty;

        public override FixAllProvider? GetFixAllProvider() => this._impl?.GetFixAllProvider();
    }
}