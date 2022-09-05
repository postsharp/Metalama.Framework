// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Fabrics
{
    /// <summary>
    /// Represents an instance of a <see cref="Fabrics.Fabric"/> type including its <see cref="TargetDeclaration"/>.
    /// </summary>
    [CompileTime]
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
        IRef<IDeclaration> TargetDeclaration { get; }
    }
}