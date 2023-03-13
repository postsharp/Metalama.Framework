// Warning CS0414 on `types`: `The field 'BaseClass.types' is assigned but its value is never used`
// Warning CS0414 on `types`: `The field 'TestClass.types' is assigned but its value is never used`
[ListImplementedTypes(typeof(ICloneable))]
[ListImplementedTypes(typeof(ISomeInterface))]
[ListImplementedTypes(typeof(ISomeInterface.INested))]
[ListImplementedTypes(typeof(ISomeInterface[]))]
[ListImplementedTypes(typeof(List<ISomeInterface>))]
[ListImplementedTypes(typeof(ISomeInterface<int>))]
[ListImplementedTypes(typeof(ISomeInterface<DateTime>))]
[ListImplementedTypes(typeof(ISomeInterface<Type>))]
[ListImplementedTypes(typeof(E))]
public abstract class BaseClass : ISomeInterface
{
    private global::System.String types = "System.ICloneable: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface: implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface+INested: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface[]: not implemented; System.Collections.Generic.List`1[Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface`1[System.Int32]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface`1[System.DateTime]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface`1[System.Type]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.E: not implemented; ";
}
public sealed class TestClass : BaseClass, ICloneable
{
    public object Clone() => throw new NotImplementedException();
    private global::System.String types = "System.ICloneable: implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface: implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface+INested: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface[]: not implemented; System.Collections.Generic.List`1[Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface`1[System.Int32]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface`1[System.DateTime]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.ISomeInterface`1[System.Type]: not implemented; Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization.E: not implemented; ";
}
