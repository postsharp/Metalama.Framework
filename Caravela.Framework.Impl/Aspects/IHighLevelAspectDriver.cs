// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Sdk;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    internal interface IHighLevelAspectDriver : IAspectDriver
    {
        AspectInstanceResult ExecuteAspect(
            AspectInstance aspectInstance,
            CompilationModel compilationModelRevision,
            CancellationToken cancellationToken );
    }
}