// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Runtime.InteropServices;

namespace Metalama.Framework.DesignTime.Contracts
{
    [ComImport]
    [Guid( "56DF8D75-6AA9-4669-976A-1BB79D5D783C" )]
    public interface IPreviewTransformationResult
    {
        bool IsSuccessful { get; set; }

        string? TransformedSourceText { get; set; }

        string[]? ErrorMessages { get; set; }
    }
}