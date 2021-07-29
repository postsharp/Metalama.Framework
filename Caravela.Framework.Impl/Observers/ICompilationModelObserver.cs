// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Observers
{
    /// <summary>
    /// An interface that can be injected into the service provider to get callbacks from the aspect pipeline when the initial <see cref="ICompilation"/> is created.
    /// For testing only.
    /// </summary>
    public interface ICompilationModelObserver : IService
    {
        /// <summary>
        /// Method called by the aspect pipeline when the initial <see cref="ICompilation"/> is created.
        /// </summary>
        /// <param name="compilation"></param>
        void OnInitialCompilationModelCreated( ICompilation compilation );
    }
}