class Target
{
  int x;
  int Foo()
  {
    Console.WriteLine("Before");
    x = this.Foo_Source();
    Console.WriteLine("After");
    return x;
  }
  private int Foo_Source()
  {
    Console.WriteLine("Original");
    return 42;
  }
}