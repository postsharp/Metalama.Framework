// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.Preview
{
    internal class UserProcessTransformationPreviewService : ITransformationPreviewService
    {
        private readonly UserProcessEndpoint _userProcessEndpoint;

        public UserProcessTransformationPreviewService( IServiceProvider serviceProvider )
        {
            this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessEndpoint>();
        }

        public async ValueTask<PreviewTransformationResult> PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken )
        {
            var projectKey = ProjectKey.FromCompilation( compilation );

            var transformationResult =
                await (await this._userProcessEndpoint.GetServerApiAsync( cancellationToken )).PreviewTransformationAsync(
                    projectKey,
                    syntaxTree.FilePath,
                    cancellationToken );

            return transformationResult;
        }
    }
}