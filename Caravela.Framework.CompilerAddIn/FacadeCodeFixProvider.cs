// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl
{
    [ExportCodeFixProvider( LanguageNames.CSharp, Name = "Caravela" )]
    [Shared]
    public class FacadeCodeFixProvider : CodeFixProvider
    {
        private readonly CodeFixProvider _impl;

        public FacadeCodeFixProvider()
        {
            this._impl = (CodeFixProvider) ModuleInitializer.GetImplementationType( "Caravela.Framework.Impl.DesignTime.DesignTimeCodeFixProvider" );
        }

        public override Task RegisterCodeFixesAsync( CodeFixContext context ) => this._impl.RegisterCodeFixesAsync( context );

        public sealed override FixAllProvider? GetFixAllProvider() => this._impl.GetFixAllProvider();

        public override ImmutableArray<string> FixableDiagnosticIds => this._impl.FixableDiagnosticIds;
    }
}