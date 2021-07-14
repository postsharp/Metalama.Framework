int Method(int a)
{
    var neutral = $"Zero={0, -5:x}";
    var ct = $"ParameterCount=1    ";
    var rt = $"Value={a, -5:x}";
    var both = $"field={a}";
    global::System.Console.WriteLine(ct);
    return (int)this.Method(a);
}