// CompileTimeAspectPipeline.ExecuteAsync failed.
// Error LAMA0236 on `JsonSerializer`: `Cannot reference 'JsonSerializer' in 'Aspect.BuildInterpolatedString()' (except for templates) because 'JsonSerializer' is run-time-only but 'Aspect.BuildInterpolatedString()' is compile-time.`
// Error LAMA0233 on `Value`: `Cannot use 'IExpression.Value' in 'Aspect.BuildInterpolatedString()' because it is only allowed inside a template. Use 'param' directly instead of accessing 'param.Value'.`