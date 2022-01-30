// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.Serialization;

namespace Metalama.Framework.DesignTime.Contracts
{
    [DataContract]
    public class PreviewTransformationResult
    {
        [DataMember( Order = 0 )]
        public bool IsSuccessful { get; set; }

        [DataMember( Order = 1 )]
        public string? TransformedCode { get; set; }

        [DataMember( Order = 2 )]
        public string[]? ErrorMessages { get; set; }

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