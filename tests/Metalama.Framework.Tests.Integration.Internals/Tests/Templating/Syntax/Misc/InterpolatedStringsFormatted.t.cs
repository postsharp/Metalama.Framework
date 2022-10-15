using System;

private int Method(int a)
{
    Console.WriteLine("ParameterCount=1    ");
    var rt = $"Value={a,-5:x}";
    var both = $"field={a}";
    return this.Method(a);
}