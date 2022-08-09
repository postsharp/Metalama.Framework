internal class Output
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal class C { }
    [global::System.SerializableAttribute]

    internal class D : C { }
}

internal record TargetRecord
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal record C { }
    [global::System.SerializableAttribute]

    internal record D : C { }
}

internal record class TargetRecordClass
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal record class C { }
    [global::System.SerializableAttribute]

    internal record class D : C { }
}