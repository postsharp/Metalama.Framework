[IntroduceInterface]
internal class BaseClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TryFindImplementationForInterfaceMember_FromBase_Generic.IInterface<global::System.Int32>
{
  public global::System.Int32 M2()
  {
    return (global::System.Int32)0;
  }
  void global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TryFindImplementationForInterfaceMember_FromBase_Generic.IInterface<global::System.Int32>.M1(global::System.Int32 i)
  {
  }
}
[CheckInterfaceAttribute]
internal class TargetClass : BaseClass
{
}