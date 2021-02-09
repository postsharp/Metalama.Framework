using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal record AdviceLinkerInput(
        Compilation Compilation,
        CompilationModel CompilationModel );
}
