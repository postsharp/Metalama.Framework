// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.Pipeline;

public interface ICompileTimeCodeEditingStatusService : IService
{
    bool IsEditingCompileTimeCode { get; }

    event Action<bool>? IsEditingCompileTimeCodeChanged;

    void OnEditingCompileTimeCodeCompleted();

    void OnUserInterfaceAttached();
}