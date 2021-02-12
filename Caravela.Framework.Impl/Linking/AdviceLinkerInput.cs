﻿using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal record AdviceLinkerInput(
        Compilation Compilation,
        CompilationModel CompilationModel,
        IReadOnlyList<INonObservableTransformation> NonObservableTransformations,
        IReadOnlyList<AspectPart> OrderedAspectParts
        );
}
