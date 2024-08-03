// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.CompileTime.Serialization;

internal interface ISerializationContext
{
    CompilationContext CompilationContext { get; }
}