// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Fabrics
{
    /// <summary>
    /// Argument of <see cref="ProjectFabric.AmendProject"/>. Allows reporting diagnostics and adding aspects to the target project. 
    /// </summary>
    public interface IProjectAmender : IAmender<ICompilation> { }
}