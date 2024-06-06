
using System;
using System.Collections.Immutable;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.SerializableNullableImmutableArray;

#pragma warning disable CS0169

class C : ICompileTimeSerializable
{
    ImmutableArray<string>? f;
}