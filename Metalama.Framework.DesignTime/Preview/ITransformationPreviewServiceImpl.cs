// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.Preview;

public interface ITransformationPreviewServiceImpl : IGlobalService
{
    Task<SerializablePreviewTransformationResult> PreviewTransformationAsync(
        ProjectKey projectKey,
        string syntaxTreeName,
        IEnumerable<string> additionalSyntaxTreeNames,
        CancellationToken cancellationToken );
}