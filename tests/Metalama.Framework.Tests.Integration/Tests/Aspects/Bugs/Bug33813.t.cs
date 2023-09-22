using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Concurrent;
namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33813;
[CompileTime]
public interface IBaseInterface
{
  int Foo { get; init; }
}
[CompileTime]
public interface IInterface : IBaseInterface
{
  int Bar { get; init; }
}
public class Aspect : TypeAspect, IInterface
{
  public int Foo { get; init; }
  public int Bar { get; init; }
}