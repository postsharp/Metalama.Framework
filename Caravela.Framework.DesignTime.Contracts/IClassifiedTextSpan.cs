// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;
using System.Runtime.InteropServices;

namespace Caravela.Framework.DesignTime.Contracts
{
    [Guid( "32b13d94-f38d-4bdc-a5f6-7c6db08d8584" )]
    [ComImport]
    public interface IClassifiedTextSpan
    {
        /// <summary>
        /// Gets the <see cref="TextSpan"/>.
        /// </summary>
        TextSpan Span { get; }

        /// <summary>
        /// Gets the classification of <see cref="Span"/>.
        /// </summary>
        TextSpanClassification Classification { get; }
    }
}