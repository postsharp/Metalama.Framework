class Target
{
  int Foo()
  {
    Console.WriteLine("Before");
    Console.WriteLine("Original");
    _ = 42;
    Console.WriteLine("After");
    return 42;
  }
}