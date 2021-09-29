// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// Argument of <see cref="ITypeFabric.AmendType"/>. Allows to report diagnostics and add aspects to the target declaration of the fabric. 
    /// </summary>
    public interface ITypeAmender : IAmender<INamedType>
    {
        /// <summary>
        /// Gets the target type of the current fabric (i.e. the declaring type of the nested type).
        /// </summary>
        INamedType Type { get; }

        /// <summary>
        /// Gets an object that allows to create advices, e.g. overriding members, introducing members, or implementing new interfaces.
        /// </summary>
        IAdviceFactory Advices { get; }
    }
}