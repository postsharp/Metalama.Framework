// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Formatting
{
    public record HtmlCodeWriterOptions( bool AddTitles = false, string? Prolog = null, string? Epilogue = null );
}