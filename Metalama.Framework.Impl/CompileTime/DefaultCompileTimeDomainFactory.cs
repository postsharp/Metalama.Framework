// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    [ExcludeFromCodeCoverage] // Not used in tests.
    internal class DefaultCompileTimeDomainFactory : ICompileTimeDomainFactory
    {
        public CompileTimeDomain CreateDomain() => new();
    }
}