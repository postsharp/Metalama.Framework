// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Preview;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.DesignTime;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.DesignTime.VisualStudio.Preview
{
    internal sealed class UserProcessTransformationPreviewService : ITransformationPreviewService
    {
        private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;

        public UserProcessTransformationPreviewService( GlobalServiceProvider serviceProvider )
        {
            this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
        }

        public async Task PreviewTransformationAsync(
            Document document,
            IPreviewTransformationResult[] result,
            CancellationToken cancellationToken )
        {
            var syntaxTree = await document.GetSyntaxTreeAsync( cancellationToken );

            if ( syntaxTree == null )
            {
                // This should never happen.
                result[0] = PreviewTransformationResult.Failure( "Cannot get the syntax tree." );

                return;
            }

            var projectKey = ProjectKeyFactory.FromProject( document.Project );

            if ( projectKey == null || !projectKey.IsMetalamaEnabled )
            {
                result[0] = PreviewTransformationResult.Failure( "Metalama is not enabled for this project." );

                return;
            }

            var analysisProcessApi = await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.PreviewTransformationAsync), cancellationToken );

            var unformattedResult = await analysisProcessApi.PreviewTransformationAsync( projectKey, syntaxTree.FilePath, cancellationToken );

            if ( !unformattedResult.IsSuccessful )
            {
                result[0] = PreviewTransformationResult.Failure( unformattedResult.ErrorMessages ?? Array.Empty<string>() );

                return;
            }

            var newSyntaxTree = unformattedResult.TransformedSyntaxTree!;

            var newDocument = document.WithSyntaxRoot(
                await newSyntaxTree.ToSyntaxTree( (CSharpParseOptions) syntaxTree.Options, cancellationToken ).GetRootAsync( cancellationToken ) );

            var formattedDocument = await OutputCodeFormatter.FormatAsync( newDocument, cancellationToken: cancellationToken, reformatAll: false );
            var formattedSyntaxTree = await formattedDocument.Document.GetSyntaxTreeAsync( cancellationToken );

            result[0] = PreviewTransformationResult.Success( formattedSyntaxTree.AssertNotNull(), unformattedResult.ErrorMessages );
        }
    }
}