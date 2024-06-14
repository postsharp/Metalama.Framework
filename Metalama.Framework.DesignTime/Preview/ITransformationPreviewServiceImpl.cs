// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.Preview;

public interface ITransformationPreviewServiceImpl : IGlobalService
{
    /// <param name="projectKey">Key for the project that contains the file that is being previewed.</param>
    /// <param name="syntaxTreeName">Path for the syntax tree that is being previewed. This file may or may not exist in the original project.</param>
    /// <param name="additionalSyntaxTreeNames">Paths to syntax trees in the original project that lead to the content of <paramref name="syntaxTreeName"/> being generated.
    /// Typically, this is used when <paramref name="syntaxTreeName"/> does not exist in the original project and stays empty otherwise.</param>
    Task<SerializablePreviewTransformationResult> PreviewTransformationAsync(
        ProjectKey projectKey,
        string syntaxTreeName,
        IEnumerable<string> additionalSyntaxTreeNames,
        CancellationToken cancellationToken );
}