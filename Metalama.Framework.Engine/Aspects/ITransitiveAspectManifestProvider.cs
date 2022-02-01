// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects
{
    public interface ITransitiveAspectManifestProvider : IService
    {
        ITransitiveAspectsManifest? GetTransitiveAspectsManifest( Compilation compilationReferenceCompilation, CancellationToken cancellationToken );
    }
}