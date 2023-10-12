using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.RecordStruct_Error;

/*
 * The record struct error.
 */

//<target>
public record struct TargetStruct : ICompileTimeSerializable
{
}