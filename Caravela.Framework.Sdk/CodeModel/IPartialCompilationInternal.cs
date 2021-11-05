using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface IPartialCompilationInternal : IPartialCompilation
    {
        Compilation InitialCompilation { get; }
    }
}