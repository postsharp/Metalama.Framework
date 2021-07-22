class Yack
    {


private IGreetingService? _service1;
        [Import]
        IGreetingService? _service {get    {
        var value = this.___service__OriginalImpl;
        if (value == null)
        {
            value = ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService? )(global::Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.ServiceLocator.ServiceProvider.GetService(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle("T:Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService")))));
            this.___service__OriginalImpl= value ?? throw new global::System.InvalidOperationException($"Cannot get a service of type Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService?.");
        }

        return (Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService? )value;
    }

set    {
        this.___service__OriginalImpl= value;
    }
}

private IGreetingService? ___service__OriginalImpl
{
    get
    {
        return this._service1;
    }

    set
    {
        this._service1 = value;
    }
}    }