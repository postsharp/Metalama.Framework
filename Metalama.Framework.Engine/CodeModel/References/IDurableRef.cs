// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface IDurableRef : IRefImpl
{
    string Id { get; }

    IFullRef ToFullRef( CompilationContext compilationContext );
}