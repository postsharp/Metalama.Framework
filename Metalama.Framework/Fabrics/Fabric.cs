// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Fabrics
{
    /// <summary>
    /// Allows adding aspects or analyzing a project, namespace, or type just by adding a type implementing this interface.
    /// You must not implement this interface directly, but <see cref="ProjectFabric"/>, <see cref="NamespaceFabric"/>,
    /// or <see cref="TypeFabric"/>. 
    /// </summary>
    /// <seealso href="@applying-aspects"/>
    [CompileTime]
    public abstract class Fabric : ILamaSerializable { }
}