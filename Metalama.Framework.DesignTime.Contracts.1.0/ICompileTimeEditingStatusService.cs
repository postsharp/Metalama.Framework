// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts;

/// <summary>
/// Interface that supports the feature where the pipeline is paused when the user edits the compile-time code.
/// The interface exposes an event and a property to observe the status, and methods to change the status.
/// </summary>
public interface ICompileTimeEditingStatusService : ICompilerService
{
    /// <summary>
    /// Gets a value indicating whether the user is currently editing compile-time code, i.e.
    /// whether the pipeline is paused.
    /// </summary>
    bool IsEditing { get; }

    /// <summary>
    /// Event raised when the value of the <see cref="IsEditing"/> property changes.
    /// </summary>
    event Action<bool> IsEditingChanged;

    /// <summary>
    /// Signals that the user has finished to edit compile-time code, and that the pipeline can be resumed.
    /// </summary>
    Task OnEditingCompletedAsync( CancellationToken cancellationToken = default );

    /// <summary>
    /// Signals that a user interface is bound to the compiler-side components. When this happens, the
    /// pipeline will use errors to report the situation where the pipeline is paused because the
    /// user is editing compile-time code.
    /// </summary>
    Task OnUserInterfaceAttachedAsync( CancellationToken cancellationToken = default );
}