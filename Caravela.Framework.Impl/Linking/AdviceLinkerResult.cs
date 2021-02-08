using Caravela.Framework.Impl.CodeModel;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    internal record AdviceLinkerResult( CompilationModel Compilation, IReactiveCollection<Diagnostic> Diagnostics );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}
