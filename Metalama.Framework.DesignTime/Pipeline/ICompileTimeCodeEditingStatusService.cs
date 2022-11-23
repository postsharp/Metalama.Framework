// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.Pipeline;

public interface ICompileTimeCodeEditingStatusService : IGlobalService
{
    bool IsEditingCompileTimeCode { get; }

    event Action<bool>? IsEditingCompileTimeCodeChanged;

    Task OnEditingCompileTimeCodeCompletedAsync( CancellationToken cancellationToken );

    void OnUserInterfaceAttached();
}