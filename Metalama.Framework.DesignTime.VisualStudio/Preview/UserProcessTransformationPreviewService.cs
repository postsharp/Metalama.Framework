// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.Preview
{
    internal class UserProcessTransformationPreviewService : ITransformationPreviewService
    {
        private readonly ServiceClient _serviceClient;

        public UserProcessTransformationPreviewService( IServiceProvider serviceProvider )
        {
            this._serviceClient = serviceProvider.GetRequiredService<ServiceClient>();
        }

        public async Task PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            IPreviewTransformationResult?[] result,
            CancellationToken cancellationToken )
        {
            if ( !ProjectIdHelper.TryGetProjectId( compilation, out var projectId ) )
            {
                result[0] = PreviewTransformationResult.Failure( "This is not a Metalama project." );

                return;
            }

            var transformationResult =
                await (await this._serviceClient.GetServerApiAsync( cancellationToken )).PreviewTransformationAsync(
                    projectId,
                    syntaxTree.FilePath,
                    cancellationToken );

            result[0] = transformationResult;
        }
    }
}