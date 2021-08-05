class Yack
    {


private IGreetingService? _service1;
        [Import]
        IGreetingService? _service {get    {
        var value = this._service_Source;
        if (value == null)
        {
            value = ((global::Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService? )(global::Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.ServiceLocator.ServiceProvider.GetService(typeof(global::Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService))));
            this._service_Source= value ?? throw new global::System.InvalidOperationException($"Cannot get a service of type Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService?.");
        }

        return (Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905.IGreetingService? )value;
    }

set    {
        this._service_Source= value;
    }
}

private IGreetingService? _service_Source
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