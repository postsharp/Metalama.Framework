using System;

int Method(int a)
{
    var neutral = $"Zero={0,-5:x}";
    var ct = $"ParameterCount=1    ";
    var rt = $"Value={a,-5:x}";
    var both = $"field={a}";
    Console.WriteLine(ct);
    return this.Method(a);
}