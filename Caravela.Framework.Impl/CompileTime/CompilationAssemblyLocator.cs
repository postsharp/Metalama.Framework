// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="IAssemblyLocator"/> that looks in metadata references of a <see cref="Compilation"/>.
    /// </summary>
    internal class CompilationAssemblyLocator : IAssemblyLocator
    {
        private readonly Compilation _compilation;

        public CompilationAssemblyLocator( Compilation compilation )
        {
            this._compilation = compilation;
        }

        public bool TryFindAssembly( AssemblyIdentity assemblyIdentity, [NotNullWhen( true )] out MetadataReference? reference )
        {
            reference = this._compilation.References.FirstOrDefault(
                r => this._compilation.GetAssemblyOrModuleSymbol( r ) is IAssemblySymbol assemblySymbol &&
                     assemblySymbol.Identity == assemblyIdentity );

            // TODO: This implementation looks for exact matches only. More testing is required with assembly binding redirections.
            // However, this is should be tested from MSBuild.

            return reference != null;
        }
    }
}