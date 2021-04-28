using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Exposes a method <see cref="TryFindAssembly"/>, which must try to find an assembly that of a given identity.
    /// </summary>
    public interface IAssemblyLocator
    {
        /// <summary>
        /// Tries to find an assembly of a given identity.
        /// </summary>
        bool TryFindAssembly( AssemblyIdentity assemblyIdentity, [NotNullWhen( true )] out MetadataReference? reference );
    }
}