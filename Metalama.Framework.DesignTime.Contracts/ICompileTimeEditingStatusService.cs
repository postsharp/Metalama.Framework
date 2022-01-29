// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts;

[Guid( "d934afa4-3a43-440d-ad6c-ea364a6311f8" )]
[ComImport]
public interface ICompileTimeEditingStatusService : ICompilerService
{
    bool IsEditing { get; }

    void RegisterCallback( ICompileTimeEditingStatusServiceCallback callback );

    Task OnEditingCompletedAsync( CancellationToken cancellationToken );
}