// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.Pipeline;

public interface ICompileTimeCodeEditingStatusService : IService
{
    bool IsEditingCompileTimeCode { get; }

    event Action<bool>? IsEditingCompileTimeCodeChanged;

    void OnEditingCompileTimeCodeCompleted();

    void OnUserInterfaceAttached();
}