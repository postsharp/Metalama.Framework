// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Defines a method that allows to transform a single syntax tree in a compilation. This service
    /// is used to produce the diff view between original code and transform code.
    /// </summary>
    public interface ITransformationPreviewService : ICompilerService
    {
        /// <summary>
        /// Transforms a single syntax tree in a compilation.
        /// </summary>
        ValueTask<PreviewTransformationResult> PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken );
    }
}