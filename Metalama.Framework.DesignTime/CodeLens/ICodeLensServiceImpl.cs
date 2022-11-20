// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.CodeLens;

/// <summary>
/// Cross-process interface equivalent to <see cref="ICodeLensService"/>.
/// </summary>
public interface ICodeLensServiceImpl : IService
{
    Task<CodeLensSummary> GetCodeLensInfoAsync( ProjectKey projectKey, SerializableDeclarationId symbolId, CancellationToken cancellationToken );

    Task<ICodeLensDetailsTable> GetCodeLensDetailsAsync( ProjectKey projectKey, SerializableDeclarationId symbolId, CancellationToken cancellationToken );
}