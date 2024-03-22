using Metalama.Framework.Aspects;
using System;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.ImportService
{
    class ImportServiceAspect : OverrideFieldOrPropertyAspect
    {

        public override dynamic? OverrideProperty
        {
            get
            {
                return meta.This._serviceProvider.GetService(meta.Target.FieldOrProperty.Type.ToType());
            }

            set
            {
                throw new NotSupportedException();
            }
        }
    }

    // <target>
    class TargetClass
    {
        private readonly IServiceProvider? _serviceProvider;

        [ImportServiceAspect]
        private IFormatProvider? FormatProvider { get; }

        public string? Format(object? o)
        {
            return ((ICustomFormatter?)this.FormatProvider?.GetFormat(typeof(ICustomFormatter)))?.Format(null, o, this.FormatProvider);
        }
    }
}
