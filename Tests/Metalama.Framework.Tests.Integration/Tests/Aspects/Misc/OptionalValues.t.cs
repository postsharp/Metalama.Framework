    using System;
    using System.Linq;
    using Metalama.Framework.Aspects;
    using Metalama.Framework.Code;
    using Metalama.Framework.Diagnostics;
    
    namespace Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues
    {
    #pragma warning disable CS0067
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
    public dynamic? OverridePropertyTemplate { get => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time."); set => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time."); }
    
        }
    #pragma warning restore CS0067
    
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
            public string? Name 
    { get
    { 
            return (global::System.String? )((global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional)this.OptionalValues).Name.Value;
    
    }
    set
    { 
    
    }
    }
    
            public Account? Parent 
    { get
    { 
            return (global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account)((global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional)this.OptionalValues).Parent.Value;
    
    }
    set
    { 
    
    }
    }
    
            public class Optional { 
    
    private global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::System.String?> _name;
    
    
    public global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::System.String?> Name 
    { get
    { 
            return this._name;
    }
    set
    { 
            this._name=value;
    }
    }
    
    private global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account> _parent;
    
    
    public global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.OptionalValue<global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account> Parent 
    { get
    { 
            return this._parent;
    }
    set
    { 
            this._parent=value;
    }
    }}
    
    
    private global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional _optionalValues = new Optional();
    
    
    public global::Metalama.Framework.Tests.Integration.Aspects.Misc.OptionalValues.Account.Optional OptionalValues 
    { get
    { 
            return this._optionalValues;
    }
    set
    { 
            this._optionalValues=value;
    }
    }     }
    }