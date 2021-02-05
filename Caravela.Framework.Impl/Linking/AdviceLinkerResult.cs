using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    record AdviceLinkerResult( CompilationModel Compilation, IReactiveCollection<Diagnostic> Diagnostics );
}
