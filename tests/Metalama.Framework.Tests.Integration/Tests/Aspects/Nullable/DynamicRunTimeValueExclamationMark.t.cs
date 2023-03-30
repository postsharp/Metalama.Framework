[Aspect]
void M()
{
    global::System.Console.WriteLine("foo".ToString());
    global::System.Console.WriteLine("bar"!.ToString());
    _ = (object)(null!);
    return;
}
