// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace Caravela.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [ExportCodeFixProvider( LanguageNames.CSharp, Name = nameof(FacadeCodeFixProvider) )]
    [Shared]
    public class FacadeCodeFixProvider : CodeFixProvider
    {
        private CodeFixProvider _impl;

        public FacadeCodeFixProvider()
        {
            this._impl = (CodeFixProvider) ResourceExtractor.CreateInstance( "Caravela.Framework.Impl.DesignTime.CentralCodeFixProvider" );
        }

        public override Task RegisterCodeFixesAsync( CodeFixContext context ) => this._impl.RegisterCodeFixesAsync( context );

        public override ImmutableArray<string> FixableDiagnosticIds => this._impl.FixableDiagnosticIds;
    }
}