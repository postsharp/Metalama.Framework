// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts;

public interface ICompileTimeEditingStatusService : ICompilerService
{
    bool IsEditing { get; }

    event Action<bool> IsEditingChanged;

    Task OnEditingCompletedAsync( CancellationToken cancellationToken );
}