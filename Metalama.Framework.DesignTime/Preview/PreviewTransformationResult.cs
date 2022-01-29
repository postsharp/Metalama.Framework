// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;

namespace Metalama.Framework.DesignTime.Preview
{
    public class PreviewTransformationResult : IPreviewTransformationResult
    {
        public bool IsSuccessful { get; }

        public string? TransformedCode { get; }

        public string[]? ErrorMessages { get; }

        private PreviewTransformationResult( bool success, string? transformedCode, string[]? errorMessages )
        {
            this.IsSuccessful = success;
            this.TransformedCode = transformedCode;
            this.ErrorMessages = errorMessages;
        }

        public static PreviewTransformationResult Failure( params string[] errorMessage ) => new( false, null, errorMessage );

        public static PreviewTransformationResult Success( string transformedCode ) => new( true, transformedCode, null );
    }
}