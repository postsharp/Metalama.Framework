// CompileTimeAspectPipeline.TryExecute failed. 
// Error CR0026 on `Method`: `The aspect 'Aspect' has thrown an exception of the 'InvalidOperationException': System.InvalidOperationException: Cannot access UseEnumerableTemplateForAnyEnumerable because the BuildAspect method has already been invoked.
//    at Caravela.Framework.Aspects.OverrideMethodAspect.EnsureBuildAspectNotCalled(String caller)
//    at Caravela.Framework.Aspects.OverrideMethodAspect.set_UseEnumerableTemplateForAnyEnumerable(Boolean value)
//    at Caravela.Framework.Tests.PublicPipeline.Aspects.Overrides.Methods.ConfigureAfterBuild2.Aspect.BuildAspect(IAspectBuilder`1 builder)`

