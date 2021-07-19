class Yack
{

    private IGreetingService? __service1;

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
            _ = (object)this.___service__OriginalImpl= value;
            return;
        }
    }
    
    private IGreetingService? ___service__OriginalImpl
    {
        get
        {
            return this.__service1;
        }

        set
        {
            this.__service1 = value;
        }
    }    }