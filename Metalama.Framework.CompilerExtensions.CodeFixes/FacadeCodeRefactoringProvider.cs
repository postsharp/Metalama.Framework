// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Composition;
using System.Threading.Tasks;

namespace Caravela.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof(FacadeCodeRefactoringProvider) )]
    [Shared]
    public class FacadeCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly CodeRefactoringProvider _impl;

        public FacadeCodeRefactoringProvider()
        {
            this._impl = (CodeRefactoringProvider) ResourceExtractor.CreateInstance( "Caravela.Framework.Impl.DesignTime.CentralCodeRefactoringProvider" );
        }

        public override Task ComputeRefactoringsAsync( CodeRefactoringContext context ) => this._impl.ComputeRefactoringsAsync( context );
    }
}