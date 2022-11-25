// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System.IO;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Defines a method <see cref="Rewrite"/> called by the compile-time assembly builder
    /// when emitting the binary compile-time assembly. This interface is used by TryMetalama.
    /// </summary>
    public interface ICompileTimeAssemblyBinaryRewriter : IGlobalService
    {
        /// <summary>
        /// Method invoked by by the compile-time assembly builder when emitting the binary compile-time assembly.
        /// </summary>
        /// <param name="input">Input stream (for reading).</param>
        /// <param name="output">Stream where the implementation must write the output.</param>
        /// <param name="path">Path of the output file. Can be used by the implementation to store additional information, but not to emit the assembly itself.</param>
        void Rewrite( Stream input, Stream output, string path );
    }
}