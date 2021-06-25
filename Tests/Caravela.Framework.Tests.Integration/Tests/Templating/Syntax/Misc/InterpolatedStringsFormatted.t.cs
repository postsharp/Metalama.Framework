// Exception during template expansion: tree must have a root node with SyntaxKind.CompilationUnit (Parameter 'newTree')
System.ArgumentException: tree must have a root node with SyntaxKind.CompilationUnit (Parameter 'newTree')
   at Microsoft.CodeAnalysis.CSharp.CSharpCompilation.ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
   at Microsoft.CodeAnalysis.CSharp.CSharpCompilation.CommonReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
   at Microsoft.CodeAnalysis.Compilation.ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
   at Microsoft.CodeAnalysis.SolutionState.UpdateDocumentInCompilationAsync(Compilation compilation, DocumentState oldDocument, DocumentState newDocument, CancellationToken cancellationToken)
   at Microsoft.CodeAnalysis.SolutionState.CompilationTracker.BuildDeclarationCompilationFromInProgressAsync(SolutionServices solutionServices, InProgressState state, Compilation inProgressCompilation, CancellationToken cancellationToken)
   at Microsoft.CodeAnalysis.SolutionState.CompilationTracker.BuildFinalStateFromInProgressStateAsync(SolutionState solution, InProgressState state, Compilation inProgressCompilation, CancellationToken cancellationToken)
   at Microsoft.CodeAnalysis.SolutionState.CompilationTracker.GetOrBuildCompilationInfoAsync(SolutionState solution, Boolean lockGate, CancellationToken cancellationToken)
   at Microsoft.CodeAnalysis.SolutionState.CompilationTracker.GetCompilationSlowAsync(SolutionState solution, CancellationToken cancellationToken)
   at Microsoft.CodeAnalysis.Document.GetSemanticModelAsync(CancellationToken cancellationToken)
   at Microsoft.CodeAnalysis.Simplification.AbstractSimplificationService`3.ReduceAsync(Document document, ImmutableArray`1 spans, OptionSet optionSet, ImmutableArray`1 reducers, CancellationToken cancellationToken)
   at Microsoft.CodeAnalysis.Simplification.Simplifier.ReduceAsync(Document document, OptionSet optionSet, CancellationToken cancellationToken)
   at Caravela.Framework.Impl.Formatting.OutputCodeFormatter.FormatAsync(Document document, CancellationToken cancellationToken) in C:\src\Caravela\Caravela.Framework.Impl\Formatting\OutputCodeFormatter.cs:line 17
   at Caravela.TestFramework.TestSyntaxTree.SetRunTimeCode(SyntaxNode syntaxNode) in C:\src\Caravela\Caravela.TestFramework\TestSyntaxTree.cs:line 106
   at Caravela.Framework.Tests.Integration.Runners.TemplatingTestRunner.RunTest(TestInput testInput) in C:\src\Caravela\Tests\Caravela.Framework.Tests.Integration\Runners\TemplatingTestRunner.cs:line 205 
