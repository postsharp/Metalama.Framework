[IntroduceInterface]
class BaseClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TryFindImplementationForInterfaceMember_FromBase.IInterface
{
  public global::System.Int32 M2()
  {
    return (global::System.Int32)0;
  }
  void global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TryFindImplementationForInterfaceMember_FromBase.IInterface.M1(global::System.Int32 i)
  {
  }
}
[CheckInterfaceAttribute]
class TargetClass : BaseClass
{
}