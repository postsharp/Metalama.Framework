// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Preview;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.DesignTime.VisualStudio.Preview
{
    internal class UserProcessTransformationPreviewService : ITransformationPreviewService
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
            var compilation = await document.Project.GetCompilationAsync( cancellationToken );
            var syntaxTree = await document.GetSyntaxTreeAsync( cancellationToken );
            
            if ( compilation == null || syntaxTree == null )
            {
                // This should never happen.
                result[0] = new PreviewTransformationResult( false, null, new[] { "Cannot get the compilation or the syntax tree." } );

                return;
            }
            
            var projectKey = compilation.GetProjectKey();

            if ( !projectKey.IsMetalamaEnabled )
            {
                result[0] = new PreviewTransformationResult( false, null, new[] { "Metalama is not enabled for this project." } );

                return;
            }

            var unformattedResult =
                await (await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.PreviewTransformationAsync), cancellationToken ))
                    .PreviewTransformationAsync(
                        projectKey,
                        syntaxTree.FilePath,
                        cancellationToken );

            if ( !unformattedResult.IsSuccessful )
            {
                result[0] = new PreviewTransformationResult( unformattedResult.IsSuccessful, null, unformattedResult.ErrorMessages );

                return;
            }

            var newSyntaxTree = unformattedResult.TransformedSyntaxTree!;

            var newDocument = document.WithSyntaxRoot( await newSyntaxTree.ToSyntaxTree( (CSharpParseOptions)syntaxTree.Options ).GetRootAsync(cancellationToken) );

            var formattedDocument = await OutputCodeFormatter.FormatToDocumentAsync( newDocument, cancellationToken: cancellationToken );
            var formattedSyntaxTree = await formattedDocument.Document.GetSyntaxTreeAsync( cancellationToken );

            result[0] = new PreviewTransformationResult( true, formattedSyntaxTree, unformattedResult.ErrorMessages );

        }
    }
}