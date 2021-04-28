// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
{
    internal class TestAssemblyLocator : IAssemblyLocator
    {
        public Dictionary<AssemblyIdentity, MetadataReference> Files { get; } = new();

        public bool TryFindAssembly( AssemblyIdentity assemblyIdentity, [NotNullWhen( true )] out MetadataReference? reference )
            => this.Files.TryGetValue( assemblyIdentity, out reference );
    }
}