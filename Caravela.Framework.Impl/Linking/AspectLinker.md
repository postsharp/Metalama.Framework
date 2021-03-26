# AspectLinker

AspectLinker combines set of all transformations, input (pre-aspect framework) Roslyn compilation and final compilation model. It creates the final Roslyn compilation, while executing all transformations 
and linking the results.

The first step produces intermediate compilation, which is a Roslyn compilation containing code of all transformations (introductions, overrides, etc.). This is semantically correct compilation
that is functionally equal to the original compilation if executed (NOTE: this may not be true indefinitely), but contains all required logic in annotated declarations.

The second step analyzes the intermediate compilation using it's `SemanticModel`s. The goal is to count references and method body analysis, which is information used during the next step.

The third step is to link all syntax introduced by the the first step together, inlining and prettifying what is possible to produce the final compilation, which is the output of Caravela.

Step 1 - Introduction (`LinkerIntroductionStep` class):
 * Execute every transformation, each of which results in a set of `IntroducedMember` objects. This uses `LinkerProceedImplementationFactory`, `LinkerProceedImpl`, `LinkerLexicalScope` and `LinkerIntroductionNameProvides` to create `MemberIntroductionContext` object, which is consumed by transformations.
 * Every `IntroducedMember`'s syntax need to be marked so that we can translate between transformations and declarations in the intermediate compilation. Resulting node with assigned identifier is wrapped into `LinkerIntroducedMember` object and stored (in `LinkerIntroductionStep.IntroducedMemberCollection`).
 * Original syntax trees are rewritten (using `LinkerIntroductionStep.Rewriter` class) to include syntax of IntroducedMember in the correct place.
 * All of collected information results in the creation of `LinkerIntroductionRegistry`, which is used during the analysis step.

Step 2 - Analysis (`LinkerAnalysisStep` class):
 * Method bodies are searched for nodes with `LinkerAnnotation`s, which are resolved to the target aspect layer. This gives `SymbolVersion` (Symbol*AspectLayer), occurrences of which are counted.
 * Method bodies are analyzed using `LinkerAnalysisStep.MethodBodyWalker` to count return statements which should determine the need for jumps in the inlined code. This algorithm is not correct and should be
   replaced with one using results of control flow analysis (which is implemented in Roslyn).
 * All of collected information is used to create `LinkerAnalysisRegistry`, which is used during the linking step.

Step 3 - Linking (`LinkerLinkingStep` class):
 * On each syntax tree execute `LinkerLinkingStep.LinkingRewriter`, which goes through every class and produces final sets of members:
    * Removes inlined members.
    * Produced members with inlined references using `LinkerLinkingStep.InliningRewriter`, which:
        * Rewrites the annotated method call using either call to a correct override or by creating another instance `Linker.LinkingStep.InliningRewriter` and executing it recursively.
        * Flattens blocks within the method that were added during inlining (this is temporary).
 * In the future the resulting syntax tree should run through the final prettifying rewriter to produce nice code that cannot be produced by above rewriters.

## Testing:

Linker unit tests are facilitated by a set of rewriters, which based on the input code produce the full linker input (see `Tests\Caravela.Framework.Tests.UnitTests\Linker\Helpers\LinkerTestBase.cs` for explanation).
This is mainly intended to bypass aspect framework and template engine, to test linker-specific scenarios in a very concise manner.

It currently takes a form of unit tests, but may be in the future adapted to the regular integration test format (even though it is not necessary).