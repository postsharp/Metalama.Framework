using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug28905
{
    internal class ImportAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                // Get the property value.
                var value = meta.Proceed();

                if ( value == null )
                {
                    // Call the service locator.
                    value = meta.Cast( meta.Target.FieldOrProperty.Type, ServiceLocator.ServiceProvider.GetService(meta.Target.Property.Type.ToType() ) );

                    // Set the field/property to the new value.
                    meta.Target.Property.Value = value
                                          ?? throw new InvalidOperationException($"Cannot get a service of type {meta.Target.Property.Type}.");
                }

                return value;
            }

            set => meta.Proceed();
        }
    }
    
    // <target>
    class Yack
    {
        [Import]
        IGreetingService? _service { get; set; }
    }
    
    interface IGreetingService
    {
        void Greet(string name);
    }
    
    
    class ServiceLocator
    {
        public static readonly IServiceProvider ServiceProvider = null!;

    }
}