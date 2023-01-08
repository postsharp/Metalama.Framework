﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Preview;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Preview;

/// <summary>
/// Result of the <see cref="ITransformationPreviewService.PreviewTransformationAsync"/> method.
/// </summary>
public sealed class PreviewTransformationResult : IPreviewTransformationResult
{
    public bool IsSuccessful { get; set; }

    public SyntaxTree? TransformedSyntaxTree { get; set; }

    public string[]? ErrorMessages { get; set; }

    public PreviewTransformationResult( bool isSuccessful, SyntaxTree? transformedSyntaxTree, string[]? errorMessages )
    {
        this.IsSuccessful = isSuccessful;

        if ( isSuccessful )
        {
            this.TransformedSyntaxTree = transformedSyntaxTree ?? throw new ArgumentNullException( nameof(transformedSyntaxTree) );
            this.ErrorMessages = errorMessages;
        }
        else
        {
            this.ErrorMessages = errorMessages ?? throw new ArgumentNullException( nameof(errorMessages) );
        }
    }

    public static PreviewTransformationResult Failure( params string[] errorMessage ) => new( false, null, errorMessage );

    public static PreviewTransformationResult Success( SyntaxTree transformedSyntaxTree, string[]? errorMessages )
        => new( true, transformedSyntaxTree, errorMessages );
}