class TargetClass
{
  [RequiresAttribute]
  void ShortName()
  {
    global::System.Console.WriteLine("Applied.");
    return;
  }
  [RequiresAttributeAttribute]
  void LongName()
  {
    global::System.Console.WriteLine("Applied.");
    return;
  }
}