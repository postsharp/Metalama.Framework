﻿using Metalama.Framework.Serialization;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.RecordStruct_Error;

/*
 * The record struct error.
 */

//<target>
public record struct TargetStruct(int Foo) : ICompileTimeSerializable
{
}