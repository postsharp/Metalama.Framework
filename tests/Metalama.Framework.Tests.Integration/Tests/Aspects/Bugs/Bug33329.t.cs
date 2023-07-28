[CompileTime]
class C
{
  [CompileTime]
  void M() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}