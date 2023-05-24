internal class TargetClass
{
  // Overrides should not have default values, only the original declaration.
  [MyOverride1Aspect]
  [MyOverride2Aspect]
  public int Method([MyContractAspect] int intParam = 42, [MyContractAspect] object? objectParam = null, [MyContractAspect] TestEnum enumParam = TestEnum.Default)
  {
    global::System.Console.WriteLine($"Override1");
    _ = this.Method_MyContractAspect2(intParam, objectParam, enumParam);
    return this.Method_MyContractAspect2(intParam, objectParam, enumParam);
  }
  private int Method_Source(int intParam = 42, object? objectParam = null, TestEnum enumParam = TestEnum.Default)
  {
    return 42;
  }
  private global::System.Int32 Method_MyContractAspect2([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on enumParam");
    global::System.Console.WriteLine("Contract on objectParam");
    global::System.Console.WriteLine("Contract on intParam");
    global::System.Console.WriteLine($"Override2");
    _ = this.Method_Source(intParam, objectParam, enumParam);
    return this.Method_Source(intParam, objectParam, enumParam);
  }
  // Overrides should not have default values, only the original declaration.
  [MyOverride1Aspect]
  [MyOverride2Aspect]
  public async Task<int> AsyncMethod([MyContractAspect] int intParam = 42, [MyContractAspect] object? objectParam = null, [MyContractAspect] TestEnum enumParam = TestEnum.Default)
  {
    global::System.Console.WriteLine($"Override1");
    _ = (await this.AsyncMethod_MyContractAspect2(intParam, objectParam, enumParam));
    return (await this.AsyncMethod_MyContractAspect2(intParam, objectParam, enumParam));
  }
  private async Task<int> AsyncMethod_Source(int intParam = 42, object? objectParam = null, TestEnum enumParam = TestEnum.Default)
  {
    await Task.Yield();
    return 42;
  }
  private async global::System.Threading.Tasks.Task<global::System.Int32> AsyncMethod_MyOverride2Aspect([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine($"Override2");
    _ = (await this.AsyncMethod_Source(intParam, objectParam, enumParam));
    return (await this.AsyncMethod_Source(intParam, objectParam, enumParam));
  }
  private async global::System.Threading.Tasks.Task<global::System.Int32> AsyncMethod_MyContractAspect([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on intParam");
    return (await this.AsyncMethod_MyOverride2Aspect(intParam, objectParam, enumParam));
  }
  private async global::System.Threading.Tasks.Task<global::System.Int32> AsyncMethod_MyContractAspect1([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on objectParam");
    return (await this.AsyncMethod_MyContractAspect(intParam, objectParam, enumParam));
  }
  private async global::System.Threading.Tasks.Task<global::System.Int32> AsyncMethod_MyContractAspect2([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on enumParam");
    return (await this.AsyncMethod_MyContractAspect1(intParam, objectParam, enumParam));
  }
  // Overrides should not have default values, only the original declaration.
  [MyOverride1Aspect]
  [MyOverride2Aspect]
  public IEnumerable<int> IteratorMethod([MyContractAspect] int intParam = 42, [MyContractAspect] object? objectParam = null, [MyContractAspect] TestEnum enumParam = TestEnum.Default)
  {
    global::System.Console.WriteLine($"Override1");
    _ = global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.IteratorMethod_MyContractAspect2(intParam, objectParam, enumParam));
    return global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.IteratorMethod_MyContractAspect2(intParam, objectParam, enumParam));
  }
  private IEnumerable<int> IteratorMethod_Source(int intParam = 42, object? objectParam = null, TestEnum enumParam = TestEnum.Default)
  {
    yield return 42;
  }
  private global::System.Collections.Generic.IEnumerable<global::System.Int32> IteratorMethod_MyOverride2Aspect([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine($"Override2");
    _ = global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.IteratorMethod_Source(intParam, objectParam, enumParam));
    return global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.IteratorMethod_Source(intParam, objectParam, enumParam));
  }
  private global::System.Collections.Generic.IEnumerable<global::System.Int32> IteratorMethod_MyContractAspect([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on intParam");
    foreach (var returnItem in this.IteratorMethod_MyOverride2Aspect(intParam, objectParam, enumParam))
    {
      yield return returnItem;
    }
  }
  private global::System.Collections.Generic.IEnumerable<global::System.Int32> IteratorMethod_MyContractAspect1([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on objectParam");
    foreach (var returnItem_1 in this.IteratorMethod_MyContractAspect(intParam, objectParam, enumParam))
    {
      yield return returnItem_1;
    }
  }
  private global::System.Collections.Generic.IEnumerable<global::System.Int32> IteratorMethod_MyContractAspect2([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on enumParam");
    foreach (var returnItem_2 in this.IteratorMethod_MyContractAspect1(intParam, objectParam, enumParam))
    {
      yield return returnItem_2;
    }
  }
  // Overrides should not have default values, only the original declaration.
  [MyOverride1Aspect]
  [MyOverride2Aspect]
  public async IAsyncEnumerable<int> AsyncIteratorMethod([MyContractAspect] int intParam = 42, [MyContractAspect] object? objectParam = null, [MyContractAspect] TestEnum enumParam = TestEnum.Default)
  {
    global::System.Console.WriteLine($"Override1");
    _ = (await global::Metalama.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.AsyncIteratorMethod_MyContractAspect2(intParam, objectParam, enumParam)));
    await foreach (var r_1 in (await global::Metalama.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.AsyncIteratorMethod_MyContractAspect2(intParam, objectParam, enumParam))))
    {
      yield return r_1;
    }
    yield break;
  }
  private async IAsyncEnumerable<int> AsyncIteratorMethod_Source(int intParam = 42, object? objectParam = null, TestEnum enumParam = TestEnum.Default)
  {
    await Task.Yield();
    yield return 42;
  }
  private async global::System.Collections.Generic.IAsyncEnumerable<global::System.Int32> AsyncIteratorMethod_MyOverride2Aspect([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine($"Override2");
    _ = (await global::Metalama.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.AsyncIteratorMethod_Source(intParam, objectParam, enumParam)));
    await foreach (var r in (await global::Metalama.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.AsyncIteratorMethod_Source(intParam, objectParam, enumParam))))
    {
      yield return r;
    }
    yield break;
  }
  private async global::System.Collections.Generic.IAsyncEnumerable<global::System.Int32> AsyncIteratorMethod_MyContractAspect([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on intParam");
    await foreach (var returnItem in this.AsyncIteratorMethod_MyOverride2Aspect(intParam, objectParam, enumParam))
    {
      yield return returnItem;
    }
  }
  private async global::System.Collections.Generic.IAsyncEnumerable<global::System.Int32> AsyncIteratorMethod_MyContractAspect1([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on objectParam");
    await foreach (var returnItem_1 in this.AsyncIteratorMethod_MyContractAspect(intParam, objectParam, enumParam))
    {
      yield return returnItem_1;
    }
  }
  private async global::System.Collections.Generic.IAsyncEnumerable<global::System.Int32> AsyncIteratorMethod_MyContractAspect2([global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Int32 intParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::System.Object? objectParam, [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.MyContractAspect] global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32707.TestEnum enumParam)
  {
    global::System.Console.WriteLine("Contract on enumParam");
    await foreach (var returnItem_2 in this.AsyncIteratorMethod_MyContractAspect1(intParam, objectParam, enumParam))
    {
      yield return returnItem_2;
    }
  }
}