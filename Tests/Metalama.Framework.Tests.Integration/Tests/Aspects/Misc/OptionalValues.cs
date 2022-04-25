using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues
{
    internal class OptionalValueTypeAttribute : TypeAspect
    {
        private static readonly DiagnosticDefinition<INamedType> _missingNestedTypeError = new(
            "OPT001",
            Severity.Error,
            "The [OptionalValueType] aspect requires '{0}' to contain a nested type named 'Optional'" );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            // Find the nested type.
            var nestedType = builder.Target.NestedTypes.OfName( "Optional" ).FirstOrDefault();

            if (nestedType == null)
            {
                builder.Diagnostics.Report( _missingNestedTypeError.WithArguments( builder.Target ), builder.Target );

                return;
            }

            // Introduce a property in the main type to store the Optional object.
            var optionalValuesProperty = builder.Advices.IntroduceProperty( builder.Target, nameof(OptionalValues) );
            optionalValuesProperty.Type = nestedType;
            optionalValuesProperty.InitializerExpression = meta.ParseExpression( $"new {nestedType.Name}()" );

            var optionalValueType = (INamedType)builder.Target.Compilation.TypeFactory.GetTypeByReflectionType( typeof(OptionalValue<>) );

            // For all automatic properties of the target type.
            foreach (var property in builder.Target.Properties.Where( p => p.IsAutoPropertyOrField ))
            {
                // Add a property of the same name, but of type OptionalValue<T>, in the nested type.
                var propertyBuilder = builder.Advices.IntroduceProperty( nestedType, nameof(OptionalPropertyTemplate) );
                propertyBuilder.Name = property.Name;
                propertyBuilder.Type = optionalValueType.ConstructGenericInstance( property.Type );

                // Override the property in the target type so that it is forwarded to the nested type.
                builder.Advices.Override(
                    property,
                    nameof(OverridePropertyTemplate),
                    tags: new { optionalProperty = propertyBuilder } );
            }
        }

        [Template]
        public dynamic? OptionalValues { get; private set; }

        [Template]
        public dynamic? OptionalPropertyTemplate { get; private set; }

        [Template]
        public dynamic? OverridePropertyTemplate
        {
            get
            {
                var optionalProperty = (IProperty)meta.Tags["optionalProperty"]!;

                return optionalProperty.Invokers.Final.GetValue( meta.This.OptionalValues ).Value;
            }

            set
            {
                var optionalProperty = (IProperty)meta.Tags["optionalProperty"]!;
                var optionalValueBuilder = new ExpressionBuilder();
                optionalValueBuilder.AppendVerbatim( "new " );
                optionalValueBuilder.AppendTypeName( optionalProperty.Type );
                optionalValueBuilder.AppendVerbatim( "( value )" );
                optionalProperty.Invokers.Final.SetValue( meta.This.OptionalValues, optionalValueBuilder.ToValue() );
            }
        }
    }

    public struct OptionalValue<T>
    {
        public bool IsSpecified { get; private set; }

        public T Value { get; }

        public OptionalValue( T value )
        {
            Value = value;
            IsSpecified = true;
        }
    }

    // <target>
    [OptionalValueType]
    internal class Account
    {
        public string? Name { get; set; }

        public Account? Parent { get; set; }

        // Currently Metalama cannot generate new classes, so we need to have
        // an empty class in the code.
        public class Optional { }
    }
}