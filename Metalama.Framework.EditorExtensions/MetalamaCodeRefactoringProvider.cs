// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Composition;
using System.Threading.Tasks;

namespace Metalama.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof(MetalamaCodeRefactoringProvider) )]
    [Shared]
    public class MetalamaCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly CodeRefactoringProvider? _impl;

        public MetalamaCodeRefactoringProvider()
        {
            switch ( ProcessKindHelper.CurrentProcessKind )
            {
                case ProcessKind.Compiler:
                    break;

                case ProcessKind.RoslynCodeAnalysisService:
                case ProcessKind.DevEnv:
                    this._impl = (CodeRefactoringProvider) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime.VisualStudio",
                        "Metalama.Framework.DesignTime.VisualStudio.VsCodeRefactoringProvider" );

                    break;

                default:
                    this._impl = (CodeRefactoringProvider) ResourceExtractor.CreateInstance(
                        "Metalama.Framework.DesignTime",
                        "Metalama.Framework.DesignTime.TheCodeRefactoringProvider" );

                    break;
            }
        }

        public override Task ComputeRefactoringsAsync( CodeRefactoringContext context )
            => this._impl?.ComputeRefactoringsAsync( context ) ?? Task.CompletedTask;
    }
}