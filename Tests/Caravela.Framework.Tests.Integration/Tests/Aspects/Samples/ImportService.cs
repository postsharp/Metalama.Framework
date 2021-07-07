using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS0169

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.ImportService
{
    class ImportServiceAspect : OverrideFieldOrPropertyAspect
    {

        public override dynamic? OverrideProperty
        {
            get
            {
                return meta.This._serviceProvider.GetService(meta.FieldOrProperty.Type.ToType());
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
