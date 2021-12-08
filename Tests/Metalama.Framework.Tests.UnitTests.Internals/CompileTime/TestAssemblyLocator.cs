// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.UnitTests.CompileTime
{
    internal class TestAssemblyLocator : IAssemblyLocator
    {
        public Dictionary<AssemblyIdentity, MetadataReference> Files { get; } = new();

#pragma warning disable 8767
        public bool TryFindAssembly( AssemblyIdentity assemblyIdentity, out MetadataReference? reference )
#pragma warning restore 8767
            => this.Files.TryGetValue( assemblyIdentity, out reference );
    }
}