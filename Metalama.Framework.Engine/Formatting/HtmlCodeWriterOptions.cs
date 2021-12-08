// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Impl.Formatting
{
    public record HtmlCodeWriterOptions( bool AddTitles = false, string? Prolog = null, string? Epilogue = null );
}