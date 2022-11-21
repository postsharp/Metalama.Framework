// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts;

public static class TransformationPreviewServiceExtensions
{
    public static async Task<IPreviewTransformationResult> PreviewTransformationAsync(
        this ITransformationPreviewService service,
        Compilation compilation,
        SyntaxTree syntaxTree,
        CancellationToken cancellationToken = default )
    {
        var result = new IPreviewTransformationResult[1];
        await service.PreviewTransformationAsync( compilation, syntaxTree, result, cancellationToken );

        return result[0];
    }
}