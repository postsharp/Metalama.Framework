// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Result of the <see cref="ITransformationPreviewService.PreviewTransformationAsync"/> method.
    /// </summary>
    public class PreviewTransformationResult
    {
        public bool IsSuccessful { get; set; }

        public string? TransformedSourceText { get; set; }

        public string[]? ErrorMessages { get; set; }

        public PreviewTransformationResult( bool isSuccessful, string? transformedCode, string[]? errorMessages )
        {
            this.IsSuccessful = isSuccessful;

            if ( isSuccessful )
            {
                this.TransformedSourceText = transformedCode ?? throw new ArgumentNullException( nameof(transformedCode) );
            }
            else
            {
                this.ErrorMessages = errorMessages ?? throw new ArgumentNullException( nameof(errorMessages) );
            }
        }

        public static PreviewTransformationResult Failure( params string[] errorMessage ) => new( false, null, errorMessage );

        public static PreviewTransformationResult Success( string transformedCode ) => new( true, transformedCode, null );
    }
}