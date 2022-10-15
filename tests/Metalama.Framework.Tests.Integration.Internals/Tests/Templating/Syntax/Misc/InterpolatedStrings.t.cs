private int Method(int a)
{
  global::System.Console.WriteLine("Zero=0    ");
  global::System.Console.WriteLine("ParameterCount=1    ");
  var dy = $"Value={a, -5:x}";
  var rt = $"Value={(global::System.Environment.Version)}";
  var both = $"field={a}";
  return this.Method(a);
}