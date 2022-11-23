// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Project
{
    /// <summary>
    /// Base interface to be inherited by all classes and interfaces that implement globally-scoped services.
    /// </summary>
    /// <seealso cref="IProjectService"/>
    [CompileTime]
    public interface IService { }
}