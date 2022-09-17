# AspectLinker

AspectLinker combines all transformations, input (pre-aspect framework) Roslyn compilation and final compilation 
model and creates the final Roslyn compilation, while executing all transformations and linking the results.

The first step produces intermediate compilation, which is a Roslyn compilation containing code of all transformations 
(introductions, overrides, replacements etc.). This is a syntactically and semantically correct compilation that contains all introduced declarations, omits all removed declarations and contains all expanded override templates.

The second step analyzes the intermediate compilation. The goal is to collect aspect references to declaration
semantics (which reference a "state" of a declaration as seen by an aspect that produced it), analyze reachability of semantics and inlineability of references and do other preprocessing for the last step.

The third step links all syntax introduced by the the first step together, inlining and prettifying what is
possible to produce the final compilation, which is the output of Metalama Framework.

## Step 1 - Introduction (`LinkerIntroductionStep` class):

* Execute every transformation, each of which results in a set of `IntroducedMember` objects. This
  uses `LinkerIntroductionNameProvider`, `LinkerProceedImplementationFactory`, `LinkerProceedImpl`, `LinkerLexicalScope`
  and `LinkerIntroductionNameProvides` to create `MemberIntroductionContext` object, which is consumed by
  transformations.
* Every `IntroducedMember`'s syntax need to be marked so that we can translate between transformations and declarations
  in the intermediate compilation. Resulting node with assigned identifier is wrapped into `LinkerIntroducedMember`
  object and stored (in `LinkerIntroductionStep.IntroducedMemberCollection`).
* Original syntax trees are rewritten (using `LinkerIntroductionStep.Rewriter` class) to include syntax of
  IntroducedMember in the correct place.
* All of collected information results in the creation of `LinkerIntroductionRegistry`, which is used during the
  analysis step.

## Step 2 - Analysis (`LinkerAnalysisStep` class):

* Method bodies are searched for nodes with `AspectReferenceAnnotation`s, which are generated during template expansion 
  whenever an aspect references another declaration. Reference is always resolved from point of view of the aspect that generated it. This results in a ResolvedAspectReference object, which contains IntermediateSymbolSemantic structure(further shortened to "semantic"). This structure describes the version of the declaration that the aspect references.
  gives `SymbolVersion` (Symbol*AspectLayer), occurrences of which are counted.
* Note: Different versions of a declaration are only visible to transformations of the containing type, all other types (including derived and nested types) always reference only the final semantic.  
* Analysis then determines:
  * Which semantics may be inlined in place of references and which cannot be. 
  * Which semantics can be completely removed.
  * Which properties and events fields should get a backing field and identifiers of such fields.
* This is done using following process:
  * Phase I. - Reachability analysis
    * Semantics form vertices of a graph where resolved references are edges.
    * Do a depth first search of such graph, starting in semantics that correspond to declarations present in the original code or to those that were introduced.
    * Unreachable semantics can be removed. This usually means that a template did not use `meta.Proceed` or invokers to access the semantic.
  * Phase II. - Inlineability of references and semantics
    * For every reference try to find a finalizer that would be able to inline the expression. LIMITATION: inliner should always inline a statement, expressions that are not parts of a specific statement are never inlineable. This may need to change for call site aspects in the future. The real reason is that for inlining expressions one need to deconstruct expression into statements, which would be quite complex and quickly leads to low readability.
    * For every reachable semantic determine whether the semantic is inlineable. This is currently limited to number of references to the semantic being exactly one.
  * Phase III. - Inlined semantics
    * Semantic is inlined if and only if it is inlineable and all references to it are inlineable.
    * For inlined semantics, determine inlining specifications. An inliner usually has several ways of inlining based on structure of the inlined code. See below for details.
    
### Inlining

An inliner replaces a statement that contains an aspect reference with another statement (usually a block) which contains the body of the target semantic. This process is recursive.

There are several general considerations:
 * If the inlined semantic body contains multiple return statements, the inliner may require using `goto` and declare label after the inlined body.
 * If the inlined semantic body returns a value (is non-void), the inliner may require using a result variable.

Proposition 1: Transformation of return statements of the inlined method is necessary if and only if aspect reference is not the last operation of the endpoint of target method's control flow graph.

Transformations:
 * T1: `return <expr>;` is transformed to `<return_variable> = <expr>; goto <return_label>;`.
 * T2: `return <expr>;` is transformed to `<return_variable> = <expr>;`.
 * T3: `return;` is transformed to `goto <return_label>;`.
 * T4: `return;` is transformed into empty statement.

T3 and T4 are valid for methods with void return types. T1 and T2 are valid for all other methods.

Proposition 2: 
  Transformations T2 and T4 are possible if and only if the return statement of the inlined method is last operation of an unconditional endpoint of method's control flow graph.

Algorithm 1: 
  Let M1 be a with aspect reference A that points to method M2, then inlining is done as follows:
    1) M2 and/or A are not inlineable ⇒ no inlining is done.
    2) A is a return statement and the last operation of an endpoint of M1 ⇒ M2 is inlined without any transformations.
    3) Otherwise, M2 is inlined while:
      a) Every return statement R (including implicit return in the end of void methods) is transformed as follows:
        * R is the last operation of an unconditional end-point of M2 ⇒ transform R using T2 or T4.
        * R is not the last operation of an unconditional end-point of M2 ⇒ transform R using T1 or T3.
      b) Add the following code:
        * If T1 or T2 was used, add return variable declaration to the beginning of the inlined body.
        * If T1 or T3 was used, add labeled empty statement to the end of the inlined body.

Observation 1: 
  Let M1, M2, M3 be methods and Case 2) applies to M1 and M2 and also to M2 and M3. Then inlining M3 to M2 and then the result to M1 results in M1' that is functionally equivalent with M1.

Observation 2: 
  Let M1, M2, M3 be method and Case 2) applies to M2 and M3 and Case 3) applies to M1 and M2 (aspect reference A in M1 is not a return statement). Then, Case 2) cannot be used to inline M3 to M2.

Algorithm 2: 
  Let M[1], ..., M[n] be methods and A[1], A[n-1] be aspect references where A[i] is part of M[i] and points to M[i+1] and M[i+1] is inlineable to M[i] through A[i].
  Let k be an integer for which for every i <= k: A[i] is a return statement and the last operation of an end point of M[i]. If there is no such A[i], k is 0.
  For i := n down to k + 2, inline M[i] into M[i-1] while:  
      a) Every return statement R (including implicit return in the end of void methods) is transformed as follows:
        * R is the last operation of end-point of M[i] ⇒ transform R using T2 or T4.
        * R is not the last operation of end-point of M[i] ⇒ transform R using T1 or T3.
      b) Add the following code:
        * If T1 or T2 was used, add return variable declaration to the beginning of the inlined body.
        * If T1 or T3 was used, add labeled empty statement to the end of the inlined body.
  For i := k down to 1, inline M[i+1] into M[i] without any transformation.
  By inlining M into N is meant that the body of N is replaced and in subsequent inlining the new body is taken.

#### Example:
```
public int Method1(int x)
{
  return Method2(x); // aspect reference

  // The aspect reference is the last statement of the only end-point of the method and 
  // the reference never requires transforming return statement of the target.
}

public int Method2(int x)
{   
  var r = Method3(); // aspect reference

  if ( r < 0 )
  {
    return -r;
  }
  else
  {
    return r;
  }
}

public int Method3(int x)
{
  if (x > 10)
  {
    return 10;
  }
  
  return x;
}
```

Algorithm 2 is executed to sequence Method1, Method2, Method3:
1) k is 1, because only Method1 returns directly the value of the aspect reference.
2) Method3 is inlined into Method2. 
  * The first return statement is in the conditional end-point and is transformed using T1.
  * The second return statement is in the unconditional end-point and is transformed using T2.
  * Since T1 or T2 was used, the return variable is necessary.
  * Since T1 was used, the return label is necessary.
  * Method2 becomes the following:

```
public int Method2(int x)
{   
  int r; // return variable
  
  if (x > 10)
  {
    goto return_label;
    r = 10;
  }
  
  r = x;
return_label:
  ; // empty labeled statement

  if ( r < 0 )
  {
    return -r;
  }
  else
  {
    return r;
  }
}
```

3) Method2 is inlined into Method1.
  * Since aspect reference is a return statement, we can inline just by replacing it.
  * Method 1 becomes:

```
public int Method1(int x)
{   
  int r; // return variable
  
  if (x > 10)
  {
    goto return_label;
    r = 10;
  }
  
  r = x;
return_label:
  ; // empty labeled statement

  if ( r < 0 )
  {
    return -r;
  }
  else
  {
    return r;
  }
}
```

## Step 3 - Linking (`LinkerLinkingStep` class):

* On each syntax tree execute `LinkerLinkingStep.LinkingRewriter`, which goes through every class and produces final
  sets of members:
    * Removes inlined members.
    * Produced final members by applying substitutions
* In the future the resulting syntax tree should run through the final prettifying rewriter to produce nice code that
  cannot be produced by above rewriters.

## Testing:

Linker unit tests are facilitated by a set of rewriters, which based on the input code produce the full linker input (
see `Tests\Metalama.Framework.Tests.UnitTests\Linker\Helpers\LinkerTestBase.cs` for explanation). This is mainly
intended to bypass aspect framework and template engine, to test linker-specific scenarios in a very concise manner.

It currently takes a form of unit tests, but may be in the future adapted to the regular integration test format (even
though it is not necessary).

## Handling of introduced methods

```
class Base
{
    int Foo()
    {
    }
}

class Derived
{
    int new Foo() // Introduced by Aspect1
    {
        return default;
    }

    public int __Foo__Override__Aspect1()
    {
        return Foo(); // Linker annotation.
    }

    public int __Foo__Override__Aspect2()
    {
        return Foo(); // Linker annotation.
    }
}
```