// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;

/// <summary>
/// Defines the remote API implemented by the user process.
/// </summary>
internal interface IUserProcessApi : IProjectHandlerCallbackApi
{
    /// <summary>
    /// Signals that the user has notified that he or she finished to edit compile-time code,
    /// and that the pipeline can be resumed.
    /// </summary>
    void OnIsEditingCompileTimeCodeChanged( bool isEditing );

    void OnCompileTimeErrorsChanged( ProjectKey projectKey, IReadOnlyCollection<DiagnosticData> errors );

    void OnAspectClassesChanged( ProjectKey projectKey );

    void OnAspectInstancesChanged( ProjectKey projectKey );
}