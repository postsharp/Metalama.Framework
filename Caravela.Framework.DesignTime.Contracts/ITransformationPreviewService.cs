// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace Caravela.Framework.DesignTime.Contracts
{
    [Guid( "63bba422-ac06-40c8-b0a9-4d2402942aec" )]
    [ComImport]
    public interface ITransformationPreviewService : ICompilerService
    {
        bool TryPreviewTransformation(
            Compilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out SyntaxTree? transformedSyntaxTree,
            [NotNullWhen( false )] out string? error );
    }
}