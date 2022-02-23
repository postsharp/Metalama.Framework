// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.Preview;

public interface ITransformationPreviewServiceImpl : IService
{
    Task<PreviewTransformationResult> PreviewTransformationAsync( string projectId, string syntaxTreeName, CancellationToken cancellationToken );
}