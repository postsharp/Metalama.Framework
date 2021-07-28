using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug28973
{
      // <target>
      class TargetCode
    {
        [Import]
        IFormatProvider FormatProvider { get; }
    }

    class ServiceLocator : IServiceProvider
    {

        private static readonly ServiceLocator _instance = new();
        private readonly Dictionary<Type, object> _services = new();

        public static IServiceProvider ServiceProvider => _instance;


        object IServiceProvider.GetService(Type serviceType)
        {
            this._services.TryGetValue(serviceType, out var value);
            return value;
        }

        public static void AddService<T>(T service) => _instance._services[typeof(T)] = service;
    }
    
       internal class ImportAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic OverrideProperty
        {
            get => ServiceLocator.ServiceProvider.GetService(meta.FieldOrProperty.Type.ToType());

            set => meta.Proceed();
        }
    }

}