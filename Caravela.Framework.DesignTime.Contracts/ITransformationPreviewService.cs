// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Defines a method that allows to transform a single syntax tree in a compilation. This service
    /// is used to produce the diff view between original code and transform code.
    /// </summary>
    [Guid( "63bba422-ac06-40c8-b0a9-4d2402942aec" )]
    [ComImport]
    public interface ITransformationPreviewService : ICompilerService
    {
        /// <summary>
        /// Transforms a single syntax tree in a compilation.
        /// </summary>
        bool TryPreviewTransformation(
            Compilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out SyntaxTree? transformedSyntaxTree,
            [NotNullWhen( false )] out string? error );
    }
}