// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class TestCompilationVersion : ICompilationVersion
{
    public TestCompilationVersion( AssemblyIdentity assemblyIdentity )
    {
        this.AssemblyIdentity = assemblyIdentity;
    }

    public AssemblyIdentity AssemblyIdentity { get; }

    public bool TryGetSyntaxTreeHash( string path, out ulong hash ) => throw new NotSupportedException();
}