class C
{
  [Outer.Inner.Log]
  void M()
  {
    global::System.Console.WriteLine("C.M() started.");
    return;
  }
}