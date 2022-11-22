// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts.Preview
{
    /// <summary>
    /// Defines a method that allows to transform a single syntax tree in a compilation. This service
    /// is used to produce the diff view between original code and transform code.
    /// </summary>
    [ComImport]
    [Guid( "982B62AD-5BB5-4B44-A7B2-2E3BEB19DE9E" )]
    public interface ITransformationPreviewService : ICompilerService
    {
        /// <summary>
        /// Transforms a single syntax tree in a compilation.
        /// </summary>
        Task PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            IPreviewTransformationResult[] result,
            CancellationToken cancellationToken );
    }
}