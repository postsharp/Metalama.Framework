// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;

namespace Metalama.Framework.CompilerExtensions
{
    // ReSharper disable UnusedType.Global

    [Transformer]
    public sealed class MetalamaSourceTransformer : ISourceTransformer
    {
        private readonly ISourceTransformer _impl;

        public MetalamaSourceTransformer()
        {
            this._impl = (ISourceTransformer) ResourceExtractor.CreateInstance(
                "Metalama.Framework.Engine",
                "Metalama.Framework.Engine.Pipeline.SourceTransformer" );
        }

        public void Execute( TransformerContext context ) => this._impl.Execute( context );
    }
}