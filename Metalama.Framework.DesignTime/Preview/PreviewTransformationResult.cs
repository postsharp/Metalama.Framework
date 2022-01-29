// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;

namespace Metalama.Framework.DesignTime.Preview
{
    public class PreviewTransformationResult : IPreviewTransformationResult
    {
        public bool IsSuccessful { get; init; }

        public string? TransformedCode { get; init; }

        public string[]? ErrorMessages { get; init; }

        // ReSharper disable once MemberCanBePrivate.Global
        // This constructor must remain public because it is used by the JSON deserializer.
        public PreviewTransformationResult() { }

        private PreviewTransformationResult( bool success, string? transformedCode, string[]? errorMessages )
        {
            this.IsSuccessful = success;

            if ( success )
            {
                this.TransformedCode = transformedCode ?? throw new ArgumentNullException( nameof(transformedCode) );
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
