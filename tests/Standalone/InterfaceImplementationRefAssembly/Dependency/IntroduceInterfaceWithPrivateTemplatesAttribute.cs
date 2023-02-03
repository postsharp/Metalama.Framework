// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable IDE0051 // Remove unused private members

namespace Dependency;

public class IntroduceInterfaceWithPrivateTemplatesAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.ImplementInterface( builder.Target, typeof( IInterface ) );
    }

    [InterfaceMember]
    private int Property
    {
        get => 42;
        set { }
    }

    [InterfaceMember]
    private int Property_SetOnly
    {
        set { }
    }

    [InterfaceMember]
    private int Property_GetOnly => 42;

    [InterfaceMember]
    private int AutoProperty { get; set; }

    [InterfaceMember]
    private int AutoProperty_GetOnly { get; }

    [InterfaceMember]
    private void Method_Void() { }

    [InterfaceMember]
    private int Method_ValueType() => 42;

    [InterfaceMember]
    private object Method_NonNullable = new object();

    [InterfaceMember]
    private object? Method_Nullable() => null;

    [InterfaceMember]
    private event EventHandler Event;

    [InterfaceMember]
    private event EventHandler EventField;

    [InterfaceMember]
    private event EventHandler? Event_Nullable;

    [InterfaceMember]
    private event EventHandler? EventField_Nullable;

    [InterfaceMember]
    private async Task AsyncMethod_Void() { }

    [InterfaceMember]
    private async Task<int> AsyncMethod_ValueType() => 42;

    [InterfaceMember]
    private async Task<object> AsyncMethod_NonNullable() => new object();

    [InterfaceMember]
    private async Task<object?> AsyncMethod_Nullable() => null;

    [InterfaceMember]
    private IEnumerable<int> IteratorMethod_ValueType()
    {
        yield return 42;
    }

    [InterfaceMember]
    private IEnumerable<object> IteratorMethod_NonNullable()
    {
        yield return new object();
    }

    [InterfaceMember]
    private IEnumerable<object?> IteratorMethod_Nullable()
    {
        yield return null;
    }

    [InterfaceMember]
    private async IAsyncEnumerable<int> AsyncIteratorMethod_ValueType()
    {
        yield return 42;
    }

    [InterfaceMember]
    private async IAsyncEnumerable<object> AsyncIteratorMethod_NonNullable()
    {
        yield return new object();
    }

    [InterfaceMember]
    private async IAsyncEnumerable<object?> AsyncIteratorMethod_Nullable()
    {
        yield return null;
    }
}