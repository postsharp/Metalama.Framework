[TestIntroduction]
[Test]
internal class TargetClass : BaseClass
{
  public void VoidMethod()
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker.TargetClass? local = null;
    local?.VoidMethod();
    return;
  }
  public int? ExistingMethod()
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker.TargetClass? local = null;
    return local?.ExistingMethod();
  }
  public int? ExistingMethod_Parameterized(int? x)
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker.TargetClass? local = null;
    return local?.ExistingMethod_Parameterized(x);
  }
  public override global::System.Int32? BaseClass_ExistingMethod()
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker.TargetClass? local = null;
    return local?.BaseClass_ExistingMethod();
  }
  public override global::System.Int32? BaseClass_ExistingMethod_Parameterized(global::System.Int32? x)
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker.TargetClass? local = null;
    return local?.BaseClass_ExistingMethod_Parameterized(x);
  }
  public override void BaseClass_VoidMethod()
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker.TargetClass? local = null;
    local?.BaseClass_VoidMethod();
    return;
  }
  public void Print()
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker.TargetClass? local = null;
    local?.Print();
    return;
  }
}