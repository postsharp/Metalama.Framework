// CompileTimeAspectPipeline.ExecuteAsync failed.
// Error LAMA0280 on `[CompileTime] T`: `The template type parameter 'T' in 'DerivedAspect.CalledTemplate<T>(int, int)' can't be marked compile-time, because the type parameter in the overridden member is not marked.`
// Error LAMA0280 on `[CompileTime] int j`: `The template parameter 'int j' in 'DerivedAspect.CalledTemplate<T>(int, int)' can't be marked compile-time, because the parameter in the overridden member is not marked.`
// Error LAMA0245 on `j`: `'int j' is invalid because it combines run-time and compile-time elements.`
