// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime
{
    [ExcludeFromCodeCoverage] // Not used in tests.
    internal sealed class DefaultCompileTimeDomainFactory : ICompileTimeDomainFactory
    {
        public CompileTimeDomain CreateDomain() => new();
    }
}