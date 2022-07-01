internal class Output
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal class C { }
    [global::System.SerializableAttribute]

    internal class D : C { }
}

internal struct TargetStruct
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal class C { }
    [global::System.SerializableAttribute]

    internal class D : C { }
}

internal record TargetRecord
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal class C { }
    [global::System.SerializableAttribute]

    internal class D : C { }
}

internal record struct TargetRecordStruct
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal class C { }
    [global::System.SerializableAttribute]

    internal class D : C { }
}

internal record class TargetRecordClass
{
    [MyAspect]
    [global::System.SerializableAttribute]    internal class C { }
    [global::System.SerializableAttribute]

    internal class D : C { }
}