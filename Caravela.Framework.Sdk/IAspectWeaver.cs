// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Project;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Aspect weavers are responsible for applying low-level aspects to the Roslyn transformation. They
    /// are used for low-level transformations only, and don't totally integrate with high-level aspects. Implementations
    /// of this class must be public, have a default constructor, and be annotated with the <see cref="AspectWeaverAttribute"/>
    /// and <see cref="CompilerPluginAttribute"/> custom attributes.
    /// </summary>
    [CompileTime]
    public interface IAspectWeaver : IAspectDriver
    {
        /// <summary>
        /// Transforms a Roslyn compilation according to some given aspects.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IPartialCompilation Transform( AspectWeaverContext context );
    }
}