using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS8618, CS0649

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.InjectionSample
{
    internal class InjectAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advice.OverrideAccessors( builder.Target, nameof(OverrideGet), args: new { T = builder.Target.Type } );
        }

        [Introduce( WhenExists = OverrideStrategy.Ignore )]
        private readonly IServiceProvider _serviceProvider = ServiceLocator.Current;

        [Template]
        public T OverrideGet<[CompileTime] T>()
        {
            // Get the property value.
            var value = meta.Proceed();

            if (value == null)
            {
                // Call the service locator.
                value = (T?)_serviceProvider.GetService( typeof(T) );

                // Set the field/property to the new value.
                meta.Target.FieldOrProperty.Value = value
                                             ?? throw new InvalidOperationException( $"Cannot get a service of type {typeof(T)}." );
            }

            return value;
        }
    }

    internal class ServiceLocator
    {
        private static readonly AsyncLocal<IServiceProvider?> _current = new();

        public static IServiceProvider Current
        {
            get => _current.Value ?? throw new InvalidOperationException();
            set => _current.Value = value;
        }
    }

    // <target>
    internal partial class Greeter
    {
        [Inject]
        private TextWriter? _console;

        public void Greet() => _console.WriteLine( "Hello, world." );
    }
}