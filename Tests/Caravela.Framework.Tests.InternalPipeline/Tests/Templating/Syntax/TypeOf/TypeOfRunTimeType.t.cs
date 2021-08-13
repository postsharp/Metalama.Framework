string Method(MyClass1 a)
{
    var rt = typeof(global::Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfRunTimeType.MyClass1);
    global::System.Console.WriteLine("rt=" + rt);
    global::System.Console.WriteLine("ct=Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfRunTimeType.MyClass1");
    global::System.Console.WriteLine("Oops");
    global::System.Console.WriteLine(typeof(global::Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfRunTimeType.MyClass1));
    global::System.Console.WriteLine(typeof(global::Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfRunTimeType.MyClass1).FullName);
    return this.Method(a);
}