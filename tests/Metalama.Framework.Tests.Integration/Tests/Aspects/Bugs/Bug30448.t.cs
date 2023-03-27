internal class Foo
{
  public void Method1([Trim] string nonNullableString, [Trim] string? nullableString)
  {
    nullableString = nullableString?.Trim();
    nonNullableString = nonNullableString.Trim();
    Console.WriteLine($"nonNullableString='{nonNullableString}', nullableString='{nullableString}'");
  }
  public string? Property { get; set; }
}