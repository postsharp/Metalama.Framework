// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Step of the aspect linker pipeline.
    /// </summary>
    /// <typeparam name="TInput">Input of the step.</typeparam>
    /// <typeparam name="TOutput">Output of the step.</typeparam>
    public abstract class AspectLinkerPipelineStep<TInput, TOutput>
    {
        public abstract TOutput Execute( TInput input );
    }
}
