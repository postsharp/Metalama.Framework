// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Invokers;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed record CompilationModelOptions( bool ShowExternalPrivateMembers = false, InvokerOptions InvokerOptions = InvokerOptions.Default )
{
    public static readonly CompilationModelOptions Default = new();
}