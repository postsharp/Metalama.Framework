// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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