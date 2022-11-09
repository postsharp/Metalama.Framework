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

        public async Task PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            IPreviewTransformationResult[] result,
            CancellationToken cancellationToken )
        {
            var projectKey = compilation.GetProjectKey();

            result[0] =
                await (await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.PreviewTransformationAsync), cancellationToken ))
                    .PreviewTransformationAsync(
                        projectKey,
                        syntaxTree.FilePath,
                        cancellationToken );
        }
    }
}