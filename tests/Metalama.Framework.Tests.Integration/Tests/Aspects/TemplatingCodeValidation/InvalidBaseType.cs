using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.InvalidBaseType;

[CompileTime]
public class MyCompileTimeType : IDisposable, IRunTimeOnlyInterface, ICompileTimeOnlyInterface
{
    public void Dispose() { }
}

[RunTimeOrCompileTime]
public class MyRunTimeOrCompileTimeType : IDisposable, IRunTimeOnlyInterface, ICompileTimeOnlyInterface
{
    public void Dispose() { }
}

public class TypeWithConflicts : IDisposable, IRunTimeOnlyInterface, ICompileTimeOnlyInterface
{
    public void Dispose() { }
}

public interface IRunTimeOnlyInterface { }

[CompileTime]
public interface ICompileTimeOnlyInterface { }