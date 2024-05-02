private int Method(int a)
{
  object? result = null;
  global::Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.AnonymousMethodWithParameters.RunTimeClass.Execute(x =>
  {
    global::System.Console.WriteLine(x);
    result = this.Method(a);
  });
  return (global::System.Int32)result;
}