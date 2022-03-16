// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Step of the aspect linker pipeline.
    /// </summary>
    /// <typeparam name="TInput">Input of the step.</typeparam>
    /// <typeparam name="TOutput">Output of the step.</typeparam>
    internal abstract class AspectLinkerPipelineStep<TInput, TOutput>
    {
        public abstract TOutput Execute( TInput input );
    }
}