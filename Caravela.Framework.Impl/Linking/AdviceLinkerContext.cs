using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    record AdviceLinkerContext(CompilationModel Compilation, IReactiveCollection<Transformation> Transformations);
}
