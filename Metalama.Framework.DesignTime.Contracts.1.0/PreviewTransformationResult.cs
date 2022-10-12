// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        public PreviewTransformationResult( bool isSuccessful, string? transformedSourceText, string[]? errorMessages )
        {
            this.IsSuccessful = isSuccessful;

            if ( isSuccessful )
            {
                this.TransformedSourceText = transformedSourceText ?? throw new ArgumentNullException( nameof(transformedSourceText) );
                this.ErrorMessages = errorMessages;
            }
            else
            {
                this.ErrorMessages = errorMessages ?? throw new ArgumentNullException( nameof(errorMessages) );
            }
        }

        public static PreviewTransformationResult Failure( params string[] errorMessage ) => new( false, null, errorMessage );

        public static PreviewTransformationResult Success( string transformedCode, string[]? errorMessages ) => new( true, transformedCode, errorMessages );
    }
}