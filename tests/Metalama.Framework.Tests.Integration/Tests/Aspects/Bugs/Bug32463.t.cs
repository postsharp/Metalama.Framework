// Warning CS0414 on `f`: `The field 'S.f' is assigned but its value is never used`
[BeforeCtor]
struct S
{
  private global::System.Int32 f = default;
  public S()
  {
    global::System.Console.WriteLine("before ctor");
  }
}