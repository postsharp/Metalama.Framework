using System;

[IntroductionAttribute]
public abstract class TargetType
{
  public abstract int Property { get; set; }
  public abstract void Method();
  class TestNestedType : global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_Abstract.TargetType
  {
        public override int Property { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override void Method()
        {
        }
  }
}