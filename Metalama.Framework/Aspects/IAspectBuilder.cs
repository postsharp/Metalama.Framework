// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is the strongly-typed variant of the <see cref="IAspectBuilder"/> interface.
    /// </summary>
    public interface IAspectBuilder : IAspectLayerBuilder
    {
        /// <summary>
        /// Skips the application of the aspect to the code. Any provided advice is ignored, but provided children aspects
        /// and diagnostics are preserved. 
        /// </summary>
        /// <remarks>
        /// Note that reporting an error using
        /// <see cref="IDiagnosticSink.Report{T}"/>
        /// automatically causes the aspect to be skipped, but, additionally, provided children aspects are ignored.
        /// </remarks>
        void SkipAspect();
    }

    /// <summary>
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface.
    /// </summary>
    public interface IAspectBuilder<out TAspectTarget> : IAspectLayerBuilder<TAspectTarget>, IAspectBuilder
        where TAspectTarget : IDeclaration
    {
        /// <summary>
        /// Registers the build action for an aspect layer. The aspect layer must have been defined
        /// by the <see cref="IAspect.BuildAspectClass"/> method.
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="buildAction"></param>
        [Obsolete( "Not implemented." )]
        void SetAspectLayerBuildAction( string layerName, Action<IAspectLayerBuilder<TAspectTarget>> buildAction );
    }
}