int Method(int a)
{
  int i = 0;
  do
  {
    i++;
    break;
  }
  while (i < 1);
  global::System.Console.WriteLine("Test result = " + i);
  var result = this.Method(a);
  return (global::System.Int32)result;
}