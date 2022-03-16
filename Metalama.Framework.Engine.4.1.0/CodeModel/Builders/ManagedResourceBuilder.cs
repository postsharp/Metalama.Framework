// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class ManagedResourceBuilder : INonObservableTransformation
    {
        Advice ITransformation.Advice => throw new NotImplementedException();

        public ManagedResource ToManagedResource() => throw new NotImplementedException();
    }
}