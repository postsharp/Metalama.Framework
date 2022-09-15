// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.Preview
{
    internal class UserProcessTransformationPreviewService : ITransformationPreviewService
    {
        private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;

        public UserProcessTransformationPreviewService( IServiceProvider serviceProvider )
        {
            this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
        }

        public async ValueTask<PreviewTransformationResult> PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken )
        {
            var projectKey = ProjectKeyExtensions.GetProjectKey( compilation );

            var transformationResult =
                await (await this._userProcessEndpoint.GetApiAsync( projectKey, cancellationToken )).PreviewTransformationAsync(
                    projectKey,
                    syntaxTree.FilePath,
                    cancellationToken );

            return transformationResult;
        }
    }
}