// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ServiceProvider;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Gets an instance of <see cref="CompileTimeDomain"/>.
    /// </summary>
    public interface ICompileTimeDomainFactory : IService
    {
        /// <summary>
        /// Gets an instance of <see cref="CompileTimeDomain"/>. 
        /// </summary>
        CompileTimeDomain CreateDomain();
    }
}