// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global
    
    [Transformer]
    public sealed class FacadeSourceTransformer : ISourceTransformer
    {
        private readonly ISourceTransformer _impl;

        public FacadeSourceTransformer()
        {
            this._impl = (ISourceTransformer) ResourceExtractor.CreateInstance( "Caravela.Framework.Impl.Pipeline.SourceTransformer" );
        }

        public Compilation Execute( TransformerContext context ) => this._impl.Execute( context );
    }
}