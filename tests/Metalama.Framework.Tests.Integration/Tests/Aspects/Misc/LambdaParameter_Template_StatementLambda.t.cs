// CompileTimeAspectPipeline.ExecuteAsync failed.
// Error LAMA0101 on `=>`: `'statement lambda' is not supported in a template.`
// Error LAMA0104 on `return p.ExpressionBody;`: `The expression 'return p.ExpressionBody;' is run-time but it is expected to be compile-time because the expression appears in a compile-time expression '.DeclaringSyntaxReferences
//             .Select(r => r.GetSyntax())
//             .Cast<PropertyDeclarationSyntax>()
//             .Select'.`
// Error LAMA0104 on `return (SyntaxNode?)getter?.ExpressionBody ?? getter?.Body;`: `The expression 'return (SyntaxNode?)getter?.ExpressionBody ?? getter?.Body;' is run-time but it is expected to be compile-time because the expression appears in a compile-time expression '.DeclaringSyntaxReferences
//             .Select(r => r.GetSyntax())
//             .Cast<PropertyDeclarationSyntax>()
//             .Select'.`
