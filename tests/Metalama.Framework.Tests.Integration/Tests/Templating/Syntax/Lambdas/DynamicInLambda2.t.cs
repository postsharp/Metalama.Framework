// TestTemplateCompiler.TryCompile failed.
// Error LAMA0263 on `p => p.Value?.ToString()`: `Lambdas or anonymous functions returning a dynamic type are not supported except. Consider casting the result to IExpression.`
// Error LAMA0104 on `p`: `The expression 'p' is run-time but it is expected to be compile-time because the expression appears in a compile-time-only member 'Value'.`