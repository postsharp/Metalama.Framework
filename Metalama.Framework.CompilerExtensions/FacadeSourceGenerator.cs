// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [Generator]
    public class FacadeSourceGenerator : ISourceGenerator
    {
        private readonly ISourceGenerator _impl;

        public FacadeSourceGenerator()
        {
            this._impl = (ISourceGenerator) ResourceExtractor.CreateInstance( "Metalama.Framework.Engine.DesignTime.DesignTimeSourceGenerator" );
        }

        void ISourceGenerator.Execute( GeneratorExecutionContext context ) => this._impl.Execute( context );

        void ISourceGenerator.Initialize( GeneratorInitializationContext context ) => this._impl.Initialize( context );
    }
}