// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Contracts
{
    [Guid( "c6c2ccd1-d8f8-4252-9df1-6c1528272efe" )]
    public interface IDesignTimeEntryPointManager
    {
        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for a specific project.
        /// </summary>
        /// <param name="version">Version of Caravela for which the service is required.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<ICompilerServiceProvider?> GetServiceProviderAsync( Version version, CancellationToken cancellationToken );

        void RegisterServiceProvider( ICompilerServiceProvider entryPoint );

        Version Version { get; }
    }
}