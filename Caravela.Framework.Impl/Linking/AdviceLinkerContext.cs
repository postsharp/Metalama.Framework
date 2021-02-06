using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;

namespace Caravela.Framework.Impl.Linking
{
    internal record AdviceLinkerContext( CompilationModel Compilation, IReactiveCollection<Transformation> Transformations );
}
