{
    var rt = global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle("T:System.String"));
    global::System.Console.WriteLine("rt=" + rt);
    global::System.Console.WriteLine("ct=System.String");
    global::System.Console.WriteLine(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle("T:Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.TypeOf.MyClass1")).FullName);
    return this.Method(a);
}
