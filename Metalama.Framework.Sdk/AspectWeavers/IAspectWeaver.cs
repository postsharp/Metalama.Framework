// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Engine.AspectWeavers
{
    /// <summary>
    /// Aspect weavers are responsible for applying low-level aspects to the Roslyn transformation. They
    /// are used for low-level transformations only, and don't totally integrate with high-level aspects. Implementations
    /// of this class must be public, have a default constructor, and be annotated with the <see cref="AspectWeaverAttribute"/>
    /// and <see cref="MetalamaPlugInAttribute"/> custom attributes.
    /// </summary>
    [CompileTimeOnly] /* For cases when the weaver is not in a separate compile-time assembly */
    public interface IAspectWeaver : IAspectDriver
    {
        /// <summary>
        /// Transforms a Roslyn compilation according to some given aspects.
        /// </summary>
        /// <param name="context"></param>
        void Transform( AspectWeaverContext context );
    }
}