// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Preview
{
    internal class PreviewTransformationResult : IPreviewTransformationResult
    {
        public bool IsSuccessful { get; }

        public SyntaxTree? SyntaxTree { get; }

        public string? ErrorMessage { get; }

        private PreviewTransformationResult( bool success, SyntaxTree? syntaxTree, string? errorMessage )
        {
            this.IsSuccessful = success;
            this.SyntaxTree = syntaxTree;
            this.ErrorMessage = errorMessage;
        }

        public static PreviewTransformationResult Failure( string errorMessage ) => new( false, null, errorMessage );

        public static PreviewTransformationResult Success( SyntaxTree syntaxTree ) => new( true, syntaxTree, null );
    }
}