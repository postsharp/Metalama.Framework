// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using PostSharp.Backstage.Extensibility;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    internal interface IInheritableAspectManifestProvider : IService
    {
        IInheritableAspectsManifest? GetInheritableAspectsManifest( Compilation compilationReferenceCompilation, CancellationToken cancellationToken );
    }
}