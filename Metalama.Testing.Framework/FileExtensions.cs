// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Testing.Framework
{
    /// <summary>
    /// List of file extensions used by the test framework.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Transformed C# code (<c>.t.cs</c>).
        /// </summary>
        public const string TransformedCode = ".t.cs";

        /// <summary>
        /// Program output (<c>.t.txt</c>).
        /// </summary>
        public const string ProgramOutput = ".t.txt";

        /// <summary>
        /// HTML rendering of the input C# (<c>.cs.html</c>).
        /// </summary>
        public const string InputHtml = ".cs.html";

        /// <summary>
        /// HTML rendering of the transformed C# (<c>.cs.html</c>).
        /// </summary>
        public const string OutputHtml = ".out.cs.html";
    }
}