using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel
{
    public class EnumViewModelAttribute : TypeAspect
    {
        private static DiagnosticDefinition<INamedType> _missingFieldError = new(
            "ENUM01",
            Severity.Error,
            "The [EnumViewModel] aspect requires the type '{0}' to have a field named '_value'." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var valueField = builder.Target.Fields.OfName( "_value" ).FirstOrDefault();

            if (valueField == null)
            {
                builder.Diagnostics.Report( _missingFieldError.WithArguments( builder.Target ) );

                return;
            }

            var enumType = (INamedType)valueField.Type;

            foreach (var member in enumType.Fields)
            {
                var propertyBuilder = builder.Advices.IntroduceProperty(
                    builder.Target,
                    nameof(IsMemberTemplate),
                    tags: new { member = member } );

                propertyBuilder.Name = "Is" + member.Name;
            }
        }

        [Template]
        public bool IsMemberTemplate => meta.This._value == ( (IField)meta.Tags["member"]! ).Invokers.Final.GetValue( null );
    }

    internal enum Visibility
    {
        Visible,
        Hidden,
        Collapsed
    }

// <target>
    [EnumViewModel]
    internal class VisibilityViewModel
    {
        private Visibility _value;

        public VisibilityViewModel( Visibility value )
        {
            _value = value;
        }
    }
}