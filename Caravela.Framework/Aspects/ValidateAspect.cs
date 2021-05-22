// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;
using System.Linq;

namespace Caravela.Framework.Aspects
{
    
    // This file is a demo use of aspect markers and template subroutines.
    
    public abstract class ValidateAttribute : Attribute, IAspectMarker<IParameter, IMethod, ValidateAspect>
    {
        public virtual void BuildEligibility( IEligibilityBuilder<IParameter> builder ) { }

        public virtual int Priority { get; }
        
        // This is a template subroutine.
        public abstract void Validate( string name, dynamic value );

    }

    public class GreaterThanZeroAttribute : ValidateAttribute
    {
        public override void Validate( string name, dynamic value )
        {
            if ( value < 0 )
            {
                throw new ArgumentOutOfRangeException( name, "The value must be strictly greater than zero." );
            }
        }
    }

    internal class ValidateAspect : IAspect<IMethod>, IAspect<IFieldOrProperty>
    {

        [OverrideMethodTemplate]
        private dynamic ValidateMethod()
        {
            var typedMarkers = meta.Markers
                .Select( m => (Parameter: (IParameter) m.MarkedDeclaration, Marker: (ValidateAttribute) m.Marker) )
                .ToList();
            
            var markersOnParameters = typedMarkers
                .Where( m => m.Parameter.Index >= 0 )
                .OrderBy( m => m.Parameter.Index )
                .ThenBy( m => m.Marker.Priority );

            var markersOnReturnValue = typedMarkers.Where( m => m.Parameter.Index < 0 );

            foreach ( var marker in markersOnParameters )
            {
                var adviceParameter = meta.Parameters[marker.Parameter.Index];
                marker.Marker.Validate( marker.Parameter.Name, adviceParameter.Value );
            }

            var returnValue = meta.Proceed();
            
            foreach ( var marker in markersOnReturnValue )
            {
                marker.Marker.Validate( "return value", returnValue );
            }

            return returnValue;
        }

        [IntroduceMethod]
        private void ValidateDynamic( object value )
        {
            dynamic castValue = meta.FieldOrProperty.Type.Cast( value );
            
            foreach ( var marker in meta.Markers )
            {
                ((ValidateAttribute) marker.Marker).Validate( meta.FieldOrProperty.Name, castValue );
            }
        }

        [OverrideFieldOrPropertySetTemplate]
        private void ValidateFieldOrPropertySetter()
        {
            // Call any other validation method.
            meta.Proceed();
            
            foreach ( var marker in meta.Markers )
            {
                ((ValidateAttribute) marker.Marker).Validate( meta.FieldOrProperty.Name, meta.FieldOrProperty.Value );
            }
        }

        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( builder.TargetDeclaration, nameof(this.ValidateMethod) );
        }

        public void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            // if not dependency property
            // {
            builder.AdviceFactory.OverrideFieldOrPropertyAccessors( builder.TargetDeclaration, null, nameof(this.ValidateFieldOrPropertySetter) );
            // } else {
            var introduceAdvice = builder.AdviceFactory.IntroduceMethod( builder.TargetDeclaration.DeclaringType, nameof(this.ValidateDynamic) );
            introduceAdvice.Builder.Name = "Validate_" + builder.TargetDeclaration.Name;
            // the dependency property advice would be supposed to use the method Validate_Property if it exists.
            // }
        }

        public void BuildEligibility( IEligibilityBuilder<IMethod> method )
        {
            method.MustBeNonAbstract();
        }

        public void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> member )
        {
            member.MustBeNonAbstract();
        }
    }
}