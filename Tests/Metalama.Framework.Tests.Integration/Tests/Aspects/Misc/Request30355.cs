#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER)
#endif

#pragma warning disable CS8618

using System.ComponentModel;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355;

[assembly: AspectOrder(typeof(OptionalValueTypeAttribute), typeof(NotifyPropertyChangedAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Request30355;

[Inherited]
internal class NotifyPropertyChangedAttribute : TypeAspect
{
    #region Events

    [InterfaceMember]
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Private Methods

    [Template]
    private dynamic? OverridePropertySetter( dynamic value )
    {
        if (value != meta.Target.Property.Value)
        {
            meta.Proceed();
            OnPropertyChanged( meta.Target.Property.Name );
        }

        return value;
    }

    #endregion

    #region Protected Methods

    [Introduce( WhenExists = OverrideStrategy.Ignore )]
    protected void OnPropertyChanged( string name )
    {
        PropertyChanged?.Invoke( meta.This, new PropertyChangedEventArgs( name ) );
    }

    #endregion

    #region Public Methods

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.ImplementInterface( builder.Target, typeof(INotifyPropertyChanged) );

        foreach (var property in builder.Target.Properties.Where( p => !p.IsAbstract && p.Writeability == Writeability.All ))
        {
            builder.Advice.OverrideAccessors( property, null, nameof(OverridePropertySetter) );
        }
    }

    #endregion
}

internal class OptionalValueTypeAttribute : TypeAspect
{
    #region Fields

    private static readonly DiagnosticDefinition<INamedType> _missingNestedTypeError = new(
        "OPT001",
        Severity.Error,
        "The [OptionalValueType] aspect requires '{0}' to contain a nested type named 'Optional'" );

    #endregion

    #region Public Methods

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
        var optionalValuesProperty =
            builder.Advice.IntroduceProperty( builder.Target, nameof(OptionalValues) );

        optionalValuesProperty.Type = nestedType;
        optionalValuesProperty.InitializerExpression = ExpressionFactory.Parse( $"new {nestedType.Name}()" );

        var optionalValueType = (INamedType) TypeFactory.GetType( typeof(OptionalValue<>) );

        // For all automatic properties of the target type.
        foreach (var property in builder.Target.Properties.Where( p => p.IsAutoPropertyOrField ))
        {
            // Add a property of the same name, but of type OptionalValue<T>, in the nested type.
            var propertyBuilder = builder.Advice.IntroduceProperty(
                nestedType,
                nameof(OptionalPropertyTemplate) );

            propertyBuilder.Name = property.Name;
            propertyBuilder.Type = optionalValueType.ConstructGenericInstance( property.Type );

            // Override the property in the target type so that it is forwarded to the nested type.
            builder.Advice.Override(
                property,
                nameof(OverridePropertyTemplate),
                tags: new { optionalProperty = propertyBuilder } );
        }
    }

    #endregion

    #region Public Properties

    [Template]
    public dynamic? OptionalPropertyTemplate { get; set; }

    [Template]
    public dynamic? OptionalValues { get; set; }

    [Template]
    public dynamic? OverridePropertyTemplate
    {
        get
        {
            var optionalProperty = (IProperty) meta.Tags["optionalProperty"]!;

            return optionalProperty.Invokers.Final.GetValue( meta.This.OptionalValues ).Value;
        }

        set
        {
            var optionalProperty = (IProperty) meta.Tags["optionalProperty"]!;
            var optionalValueBuilder = new ExpressionBuilder();
            optionalValueBuilder.AppendVerbatim( "new " );
            optionalValueBuilder.AppendTypeName( optionalProperty.Type );
            optionalValueBuilder.AppendVerbatim( "( value )" );
            optionalProperty.Invokers.Final.SetValue( meta.This.OptionalValues, optionalValueBuilder.ToValue() );
        }
    }

    #endregion
}

public struct OptionalValue<T>
{
    #region Constructors

    public OptionalValue( T value )
    {
        Value = value;
        IsSpecified = true;
    }

    #endregion

    #region Public Properties

    public bool IsSpecified { get; private set; }

    public T Value { get; }

    #endregion
}

// <target>
[NotifyPropertyChanged]
internal partial class Person
{
    #region Constructors

    public Person() { }

    #endregion

    #region Public Properties

    public string FirstName { get; set; }

    public string FullName
    {
        get
        {
            return $"{FirstName} {LastName}";
        }
    }

    public string LastName { get; set; }

    #endregion
}

// <target>
[OptionalValueType]
public partial class Account
{
    #region Public Properties

    public string? Name { get; set; }

    public Account? Parent { get; set; }

    #endregion

    public partial class Optional { }
}