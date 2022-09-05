// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting;

/// <summary>
/// Defines the remote API implemented by the user process.
/// </summary>
internal interface IUserProcessApi : IProjectHandlerCallback
{
    /// <summary>
    /// Signals that the user has notified that he or she finished to edit compile-time code,
    /// and that the pipeline can be resumed.
    /// </summary>
    void OnIsEditingCompileTimeCodeChanged( bool isEditing );
}