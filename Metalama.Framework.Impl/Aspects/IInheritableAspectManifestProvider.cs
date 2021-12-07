// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Impl.Aspects
{
    internal interface IInheritableAspectManifestProvider : IService
    {
        IInheritableAspectsManifest? GetInheritableAspectsManifest( Compilation compilationReferenceCompilation, CancellationToken cancellationToken );
    }
}