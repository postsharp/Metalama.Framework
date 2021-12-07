// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class ManagedResourceBuilder : INonObservableTransformation
    {
        Advice ITransformation.Advice => throw new NotImplementedException();

        public ManagedResource ToManagedResource() => throw new NotImplementedException();
    }
}