// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Result of the <see cref="ITransformationPreviewService.PreviewTransformationAsync"/> method.
    /// </summary>
    [Guid( "0a004c50-4afc-4b18-a37d-d5ae9932e0e3" )]
    [ComImport]
    public interface IPreviewTransformationResult
    {
        bool IsSuccessful { get; }

        SyntaxTree? SyntaxTree { get; }

        string? ErrorMessage { get; }
    }
}