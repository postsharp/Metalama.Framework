[Outer.ReturnNumbers]
private object Method()
{
  var numbers = new object[]
  {
    42
  };
  return (global::System.Object)global::Metalama.Framework.Tests.Integration.Tests.Aspects.ExtensionMethods.NoReceiver.Outer.MyToList(numbers);
}