using Castle.Components.DictionaryAdapter;
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.RecordClass_Manual;

/*
 * The record class with a custom serializer.
 */

//<target>
[RunTimeOrCompileTime]
public record class SerializableClass : ICompileTimeSerializable
{
    public int Foo { get; set; }

    public class Serializer_Custom : ReferenceTypeSerializer
    {
        public override object CreateInstance(Type type, IArgumentsReader constructorArguments) => new SerializableClass();

        public override void DeserializeFields(object obj, IArgumentsReader initializationArguments) => ((SerializableClass)obj).Foo = initializationArguments.GetValue<int>("Foo");

        public override void SerializeObject(object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments) => initializationArguments.SetValue("Foo", ((SerializableClass)obj).Foo);
    }
}

[Inheritable]
public class TestAspect : OverrideMethodAspect
{
    public SerializableClass SerializedValue;

    public TestAspect(int x)
    {
        SerializedValue = new SerializableClass() { Foo = 42 };
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.CompileTime(SerializedValue.Foo));
        return meta.Proceed();
    }

}

public class BaseClass
{
    [TestAspect(42)]
    public virtual void Foo()
    {
    }
}