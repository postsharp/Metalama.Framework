// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ServiceProvider;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Exposes a method <see cref="TryFindAssembly"/>, which must try to find an assembly that of a given identity.
    /// </summary>
    public interface IAssemblyLocator : IService
    {
        /// <summary>
        /// Tries to find an assembly of a given identity.
        /// </summary>
        bool TryFindAssembly( AssemblyIdentity assemblyIdentity, [NotNullWhen( true )] out MetadataReference? reference );
    }
}