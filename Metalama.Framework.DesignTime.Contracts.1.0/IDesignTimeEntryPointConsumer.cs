// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts;

public interface IDesignTimeEntryPointConsumer : IObservable<ICompilerServiceProvider>
{
    /// <summary>
    /// Gets the <see cref="ICompilerServiceProvider"/> for a specific project. This method is called by the VSX.
    /// </summary>
    /// <param name="version">Version of Metalama for which the service is required.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<ICompilerServiceProvider?> GetServiceProviderAsync( Version version, CancellationToken cancellationToken );

    /// <summary>
    /// Gets all compatible registered service providers.
    /// </summary>
    /// <returns></returns>
    IEnumerable<ICompilerServiceProvider> GetRegisteredProviders();

    /// <summary>
    /// Event raised when a provider is registered and it has an invalid contract version (which can happen
    /// in pre-release versions only). The VSX can handle this event and display an error message.
    /// </summary>
    event Action<ICompilerServiceProvider> ContractVersionMismatchDetected;
}