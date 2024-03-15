using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33813;

public class MyAttribute : Attribute
{
}

// <target>
[type: My]
public class TestClass<[typevar: My] T>
{
    [field: My]
    public int Field;

    [field: My]
    [property: My]
    public int AutoProperty
    {
        [method: My]
        [return: My]
        get;

        [method: My]
        [return: My]
        [param: My]
        set;
    }

    [property: My]
    public int Property
    {
        [method: My]
        [return: My]
        get => 42;

        [method: My]
        [return: My]
        [param: My]
        set { }
    }

    [field:My]
    [event:My]
    [method:My]
    public event EventHandler? EventField;

    [event: My]
    public event EventHandler? Event
    {
        [method: My]
        [return: My]
        [param: My]
        add { }

        [method: My]
        [return: My]
        [param: My]
        remove { }
    }

    [method: My]
    [return: My]
    public int Foo<[typevar: My] U>([param: My] int x) => 42;
}


// <target>
[type: My]
[method: My]
public record class TestRecord(
    [param: My][field: My][property: My] int X)
{
}

// <target>
[type: My]
[return: My]
public delegate void TestDelegate([param: My] int x);