using System;
using System.Linq;
using System.ComponentModel;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32975;

[assembly: AspectOrder( typeof(TrackChangesAttribute), typeof(NotifyPropertyChangedAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32975;

public interface IChangeTracking
{
    bool? HasChanges { get; }
    bool IsTrackingChanges { get; set; }
    void ResetChanges();
}


[TrackChanges]
[NotifyPropertyChanged]
public partial class Comment
{
    public Guid Id { get; }
    public string Author { get; set; }
    public string Content { get; set; }

    public Comment( Guid id, string author, string content )
    {
        this.Id = id;
        this.Author = author;
        this.Content = content;
    }
}

[TrackChanges]
public class ModeratedComment : Comment
{
    public ModeratedComment( Guid id, string author, string content ) : base( id, author, content )
    {
    }

    public bool? IsApproved { get; set; }
}


[Inheritable]
internal class NotifyPropertyChangedAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.ImplementInterface( builder.Target, typeof(INotifyPropertyChanged), OverrideStrategy.Ignore );

        foreach ( var property in builder.Target.Properties.Where( p =>
                                                                       !p.IsAbstract && p.Writeability == Writeability.All ) )
        {
            builder.Advice.OverrideAccessors( property, null, nameof(this.OverridePropertySetter) );
        }
    }

    [InterfaceMember]
    public event PropertyChangedEventHandler? PropertyChanged;

    [Introduce( WhenExists = OverrideStrategy.Ignore )]
    protected void OnPropertyChanged( string name ) =>
        this.PropertyChanged?.Invoke( meta.This, new PropertyChangedEventArgs( name ) );

    [Template]
    private dynamic OverridePropertySetter( dynamic value )
    {
        if ( value != meta.Target.Property.Value )
        {
            meta.Proceed();
            this.OnPropertyChanged( meta.Target.Property.Name );
        }

        return value;
    }
}


public class TrackChangesAttribute : TypeAspect
{
    private static readonly DiagnosticDefinition<INamedType> _mustHaveOnChangeMethod = new(     
        "MY001",
        Severity.Error,
        $"The '{nameof(IChangeTracking)}' interface is implemented manually on type '{{0}}', but the type does not have an '{nameof(OnChange)}()' method." );

    private static readonly DiagnosticDefinition _onChangeMethodMustBeProtected = new(
        "MY002",
        Severity.Error,
        $"The '{nameof(OnChange)}()' method must be have the 'protected' accessibility." );

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Implement the IChangeTracking interface.         
        var implementInterfaceResult = builder.Advice.ImplementInterface( builder.Target, typeof( IChangeTracking ), OverrideStrategy.Ignore );

        // If the type already implements IChangeTracking, it must have a protected method called OnChanged, without parameters, otherwise
        // this is a contract violation, so we report an error.
        if ( implementInterfaceResult.Outcome == AdviceOutcome.Ignored )
        {
            var onChangeMethod = builder.Target.AllMethods.OfName( nameof( OnChange ) ).Where( m => m.Parameters.Count == 0 ).SingleOrDefault();

            if ( onChangeMethod == null )
            {
                builder.Diagnostics.Report( _mustHaveOnChangeMethod.WithArguments( builder.Target ) );
            }
            else if ( onChangeMethod.Accessibility != Accessibility.Protected )
            {
                builder.Diagnostics.Report( _onChangeMethodMustBeProtected );
            }
        }

        var onPropertyChanged = this.GetOnPropertyChangedMethod( builder.Target);

        if ( onPropertyChanged == null )
        {
            // Override all writable fields and automatic properties.
            var fieldsOrProperties = builder.Target.FieldsAndProperties
                .Where( f => !f.IsImplicitlyDeclared && f.Writeability == Writeability.All && f.IsAutoPropertyOrField == true );

            foreach ( var fieldOrProperty in fieldsOrProperties )
            {
                builder.Advice.OverrideAccessors( fieldOrProperty, null, nameof( this.OverrideSetter ) );
            }
        }
    }

    [InterfaceMember]
    public bool? HasChanges { get; protected set; }

    [InterfaceMember]
    public bool IsTrackingChanges 
    {
        get => this.HasChanges.HasValue;
        set
        {
            if ( this.IsTrackingChanges != value )
            {
                this.HasChanges = value ? false : null;

                var onPropertyChanged = this.GetOnPropertyChangedMethod( meta.Target.Type );

                if ( onPropertyChanged != null )
                {
                    onPropertyChanged.Invoke( nameof( this.IsTrackingChanges ) );
                }
            }
        }
    }

    [InterfaceMember]
    public void ResetChanges()
    {
        if ( this.IsTrackingChanges )
        {
            this.HasChanges = false;
        }
    }

    private IMethod? GetOnPropertyChangedMethod( INamedType type ) 
        => type.AllMethods
                .OfName( "OnPropertyChanged" )
                .Where( m => m.Parameters.Count == 1 )
                .SingleOrDefault();


    [Introduce( WhenExists = OverrideStrategy.Ignore )]
    protected void OnChange()
    {
        if ( this.HasChanges == false )
        {
            this.HasChanges = true;

            var onPropertyChanged = this.GetOnPropertyChangedMethod( meta.Target.Type );

            if ( onPropertyChanged != null )
            {
                onPropertyChanged.Invoke( nameof( this.HasChanges ));
            }
        }
    }

    [Template]
    private void OverrideSetter( dynamic? value )
    {
        meta.Proceed();

        if ( value != meta.Target.Property.Value )
        {
            this.OnChange();
        }
    }
}