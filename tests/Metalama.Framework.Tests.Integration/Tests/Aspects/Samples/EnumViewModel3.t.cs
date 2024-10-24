using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Diagnostics;
using System.Linq;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.EnumViewModel3;
[assembly: EnumViewModel(typeof(DayOfWeek), "Doc.EnumViewModel")]
namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.EnumViewModel3;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class EnumViewModelAttribute : CompilationAspect
{
  private Type _enumType;
  private readonly string _targetNamespace;
  public EnumViewModelAttribute(Type enumType, string targetNamespace)
  {
    this._enumType = enumType;
    this._targetNamespace = targetNamespace;
  }
  public override void BuildAspect(IAspectBuilder<ICompilation> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  [Template]
  public void ConstructorTemplate([CompileTime] IField underlyingValueField) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  [Template]
  public bool IsEnumValue([CompileTime] IField enumMember, [CompileTime] IField underlyingValueField) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}