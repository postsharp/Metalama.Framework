using Caravela.Framework.Impl.CodeModel;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal record AdviceLinkerResult( CompilationModel Compilation, IReactiveCollection<Diagnostic> Diagnostics );
}
