// Warning CS0105 on `Metalama.Framework.Code.SyntaxBuilders`: `The using directive for 'Metalama.Framework.Code.SyntaxBuilders' appeared previously in this namespace`
[assembly: GenerateProxyAspect(typeof(ISomeInterface), "Metalama.Samples.Proxy.Tests", "SomeProxy")]
public interface ISomeInterface
{
  void VoidMethod(int a, string b);
  int NonVoidMethod(int a, string b);
  void VoidNoParamMethod();
}