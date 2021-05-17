// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [Transformer]
    internal sealed class FacadeSourceTransformer : ISourceTransformer
    {
        private readonly ISourceTransformer _impl;

        public FacadeSourceTransformer()
        {
            this._impl = (ISourceTransformer) ModuleInitializer.GetImplementationType( "Caravela.Framework.Impl.Pipeline.SourceTransformer" );
        }

        public Compilation Execute( TransformerContext transformerContext ) => this._impl.Execute( transformerContext );
    }
}