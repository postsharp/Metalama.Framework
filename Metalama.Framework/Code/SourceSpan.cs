// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code;

/// <summary>
/// Represents a span of source code.
/// </summary>
public sealed class SourceSpan
{
    private readonly ISyntaxReferenceImpl _syntaxReferenceImpl;

    internal SourceSpan(
        string filePath,
        object syntaxTree,
        int start,
        int end,
        int startLine,
        int endLine,
        int startColumn,
        int endColumn,
        ISyntaxReferenceImpl syntaxReferenceImpl )
    {
        this.SyntaxTree = syntaxTree;
        this.Start = start;
        this.End = end;
        this._syntaxReferenceImpl = syntaxReferenceImpl;
        this.FilePath = filePath;
        this.StartLine = startLine;
        this.EndLine = endLine;
        this.StartColumn = startColumn;
        this.EndColumn = endColumn;
    }

    /// <summary>
    /// Gets the path of the source file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the start line (zero-based).
    /// </summary>
    public int StartLine { get; }

    /// <summary>
    /// Gets the end line (zero-based).
    /// </summary>
    public int EndLine { get; }

    /// <summary>
    /// Gets the start column (zero-based).
    /// </summary>
    public int StartColumn { get; }

    /// <summary>
    /// Gets the start end (zero-based).
    /// </summary>
    public int EndColumn { get; }

    // The following properties allow to map back to the SyntaxTree

    internal object SyntaxTree { get; }

    internal int Start { get; }

    internal int End { get; }

    /// <summary>
    /// Gets the text representation (i.e. the source code) of the current syntax node or token.
    /// </summary>
    /// <returns>The source code of the current syntax node.</returns>
    public string GetText() => this._syntaxReferenceImpl.GetText( this );

    public override string ToString() => $"{this.FilePath}";
}