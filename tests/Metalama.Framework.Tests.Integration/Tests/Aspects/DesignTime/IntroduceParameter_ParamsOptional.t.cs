namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_ParamsOptional
{
  partial class TestClass
  {
    public TestClass(global::System.Int32 param1, global::System.Int32 optParam = 42, global::System.Int32 introduced1 = 42, global::System.String introduced2 = "42", global::System.Int32[] param2) : this(param1, optParam: optParam, param2)
    {
    }
    public TestClass(global::System.Int32 param1, global::System.Int32[] param2) : this(param1, param2, optParam: default)
    {
    }
  }
}