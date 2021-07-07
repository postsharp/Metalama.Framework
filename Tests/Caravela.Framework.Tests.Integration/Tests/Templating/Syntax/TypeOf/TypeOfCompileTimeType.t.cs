string Method(string a)
{
    var rt = global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle("T:System.String"));
    global::System.Console.WriteLine("rt=" + rt);
    global::System.Console.WriteLine("ct=System.String");
    return this.Method(a);
}