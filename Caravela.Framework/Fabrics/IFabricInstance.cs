// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// Represents an instance of a <see cref="Fabrics.Fabric"/> type including its <see cref="TargetDeclaration"/>.
    /// </summary>
    [CompileTimeOnly]
    public interface IFabricInstance : IAspectPredecessor
    {
        /// <summary>
        /// Gets the <see cref="Fabrics.Fabric"/> instance.
        /// </summary>
        Fabric Fabric { get; }

        /// <summary>
        /// Gets the declaration to which the fabric is applied. It can be an <see cref="INamedType"/>, an <see cref="INamespace"/>
        /// or the <see cref="ICompilation"/>.
        /// </summary>
        IRef<IDeclaration>? TargetDeclaration { get; }
    }
}