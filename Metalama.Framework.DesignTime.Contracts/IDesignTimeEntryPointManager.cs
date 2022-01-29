// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Contract of the entry point of the API between the Metalama VSX and the Metalama analyzer, which can
    /// be both of different versions. This contract is strongly versioned. The reference to this API is stored
    /// on the <see cref="AppDomain"/> using <see cref="AppDomain.GetData"/> and <see cref="AppDomain.SetData"/>.
    /// </summary>
    [Guid( "c6c2ccd1-d8f8-4252-9df1-6c1528272efe" )]
    [ComImport]
    public interface IDesignTimeEntryPointManager : IObservable<ICompilerServiceProvider>
    {
        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for a specific project. This method is called by the VSX.
        /// </summary>
        /// <param name="version">Version of Metalama for which the service is required.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<ICompilerServiceProvider?> GetServiceProviderAsync( Version version, CancellationToken cancellationToken );

        IEnumerable<ICompilerServiceProvider> GetRegisteredProviders();

        /// <summary>
        /// Registers a <see cref="ICompilerServiceProvider"/>. This method is called by the analyzer assembly.
        /// </summary>
        void RegisterServiceProvider( ICompilerServiceProvider entryPoint );

        /// <summary>
        /// Gets the version of the implementation of this interface.
        /// </summary>
        Version Version { get; }
    }

    [ComImport]
    [Guid( "191046a8-fff7-41e5-9baa-75dff5c1e0c7" )]
    public interface IDesignTimeEntryPointManagerCallback
    {
        void OnProviderRegistered( ICompilerServiceProvider provider );
    }
}