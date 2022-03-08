using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
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
            var nestedType = builder.Target.NestedTypes.OfName( "Optional" ).FirstOrDefault();

            if (nestedType == null)
            {
                builder.Diagnostics.Report( _missingNestedTypeError.WithArguments( builder.Target ), builder.Target );

                return;
            }

            var optionalValuesProperty = builder.Advices.IntroduceProperty( builder.Target, nameof(OptionalValues) );
            optionalValuesProperty.Type = nestedType;
            optionalValuesProperty.InitializerExpression = meta.ParseExpression( $"new {nestedType.Name}()" );

            var optionalType = (INamedType)builder.Target.Compilation.TypeFactory.GetTypeByReflectionType( typeof(OptionalValue<>) );

            foreach (var property in builder.Target.Properties.Where( p => p.IsAutoPropertyOrField ))
            {
                var propertyBuilder = builder.Advices.IntroduceProperty( nestedType, nameof(OptionalPropertyTemplate) );
                propertyBuilder.Name = property.Name;
                var constructedOptionalType = optionalType.ConstructGenericInstance(property.Type);
                propertyBuilder.Type = constructedOptionalType;
                var optionalTypeConstructor = constructedOptionalType.Constructors.Single(x => x.Parameters.Count == 1);

                builder.Advices.OverrideFieldOrProperty(
                    property,
                    nameof(OverridePropertyTemplate),
                    tags: new TagDictionary 
                    { 
                        ["optionalProperty"] = propertyBuilder,
                        ["optionalTypeConstructor"] = optionalTypeConstructor,
                    } );
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
                var constructor = (IConstructor)meta.Tags["optionalTypeConstructor"]!;

                // TODO: I guess here we would need to execute ctor, but how?.
                //optionalProperty.Invokers.Final.SetValue( meta.This.OptionalValues, constructor. value);
            }
        }
    }

    public struct OptionalValue<T>
    {
        private T? _value;

        public bool IsSpecified { get; }

        public OptionalValue(T value)
        {
            if (value != null)
            {
                this._value = value;
                this.IsSpecified = true;
            }
            else
            {
                this._value = default;
                this.IsSpecified = false;
            }
        }

        public T Value => _value ?? throw new InvalidOperationException();
    }

    [OptionalValueType]
    internal class Account
    {
        public string? Name { get; set; }

        public Account? Parent { get; set; }

        public class Optional { }
    }
}