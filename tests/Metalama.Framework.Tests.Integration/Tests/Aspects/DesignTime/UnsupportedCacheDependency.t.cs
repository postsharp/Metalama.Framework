// Error LAMA0041 on `Nested`: `'MyAspect.BuildAspect' threw 'InvalidOperationException' when applied to 'Target.Nested': 'The 'INamespace.Types' API is not supported in the BuildAspect context at design time. It is only supported in the context of a adding new aspects (IAspectReceiverSelector.With)'.You can use MetalamaExecutionContext.Current.ExecutionScenario.IsDesignTime to run your code at design time only. Exception details are in '(none)'. To attach a debugger to the compiler, use the  '-p:MetalamaDebugCompiler=True' command-line option.`