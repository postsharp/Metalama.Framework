using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.RecordClass_Error;

/*
 * The record class error.
 */

//<target>
public record class TargetClass(int Foo) : ICompileTimeSerializable
{
}