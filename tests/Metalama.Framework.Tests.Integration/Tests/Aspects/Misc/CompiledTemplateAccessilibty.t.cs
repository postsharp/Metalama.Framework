[MyAspect]
internal class Target
{
  internal global::System.String Internal()
  {
    return "Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty.InternalCompileTimeType";
  }
  private global::System.String Private()
  {
    return "Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty.MyAspect+PrivateCompileTimeType";
  }
  private protected global::System.String PrivateProtected()
  {
    return "Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty.MyAspect+ProtectedCompileTimeType, Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty.InternalCompileTimeType";
  }
  protected global::System.String Protected()
  {
    return "Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty.MyAspect+ProtectedCompileTimeType";
  }
  protected internal global::System.String ProtectedInternal()
  {
    return "Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty.MyAspect+ProtectedInternalCompileTimeType";
  }
  public global::System.String Public()
  {
    return "Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CompiledTemplateAccessilibty.PublicCompileTimeType";
  }
}