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

        public bool TryFindAssembly( AssemblyIdentity assemblyIdentity,  [NotNullWhen(true)] out MetadataReference? reference )
        {
            reference = this._compilation.References.FirstOrDefault(
                r =>
                {
                    var symbol = this._compilation.GetAssemblyOrModuleSymbol( r ) as IAssemblySymbol;

                    return symbol != null && symbol.Identity == assemblyIdentity;

                } );
            
            // TODO: This implementation looks for exact matches only. More testing is required with assembly binding redirections.
            // However, this is should be tested from MSBuild.

            return reference != null;

        }
    }
}