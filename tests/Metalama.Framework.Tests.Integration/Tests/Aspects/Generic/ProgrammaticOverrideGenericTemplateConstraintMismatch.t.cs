// CompileTimeAspectPipeline.ExecuteAsync failed.
// Error LAMA0041 on `builder.Advice.Override( builder.Target, nameof(OverrideMethod) );`: `'Aspect.BuildAspect' threw 'InvalidTemplateSignatureException' when applied to 'TargetCode.Method<T>(T)': Cannot use the template 'Aspect.OverrideMethod<T>(T)' to override the method 'TargetCode.Method<T>(T)': the constraints on the template parameter 'T' are not compatible with the constraints on the target method parameter 'T'. Exception details are in '(none)'. To attach a debugger to the compiler, use the '-p:MetalamaDebugCompiler=True' command-line option.`