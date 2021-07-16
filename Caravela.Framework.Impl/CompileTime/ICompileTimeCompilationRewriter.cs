// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Defines a method <see cref="Rewrite"/> called by the compile-time assembly builder
    /// when emitting the binary compile-time assembly. This interface is used by TryCaravela.
    /// </summary>
    public interface ICompileTimeCompilationRewriter : IService
    {
        /// <summary>
        /// Method invoked by by the compile-time assembly builder when emitting the binary compile-time assembly.
        /// </summary>
        /// <param name="input">Input stream (for reading).</param>
        /// <param name="output">Stream where the implementation must write the output.</param>
        /// <param name="directory">Output directory, where the implementation can write its own files.</param>
        void Rewrite( Stream input, Stream output, string directory );
    }
}