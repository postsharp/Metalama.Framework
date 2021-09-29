// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// Argument of <see cref="IProjectFabric.AmendProject"/>. Allows to report diagnostics and add aspects to the target project. 
    /// </summary>
    public interface IProjectAmender : IAmender<ICompilation> { }
}