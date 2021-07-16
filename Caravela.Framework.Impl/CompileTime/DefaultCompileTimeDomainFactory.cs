// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime
{
    internal class DefaultCompileTimeDomainFactory : ICompileTimeDomainFactory
    {
        public CompileTimeDomain CreateDomain() => new CompileTimeDomain();
    }
}