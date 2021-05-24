using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.TestApp.Aspects.Validation
{

    internal class ValidateAspect : IAspect<IMethod>, IAspect<IFieldOrProperty>
    {
        [OverrideMethodTemplate]
        private dynamic ValidateMethod()
        {
            var typedMarkers = meta.Markers
                .Select(m => (Parameter: (IParameter)m.MarkedDeclaration, Marker: (ValidateAttribute)m.Marker))
                .ToList();

            var markersOnParameters = typedMarkers
                .Where(m => m.Parameter.Index >= 0)
                .OrderBy(m => m.Parameter.Index)
                .ThenBy(m => m.Marker.Priority);

            var markersOnReturnValue = typedMarkers.Where(m => m.Parameter.Index < 0);

            foreach (var marker in markersOnParameters)
            {
                var adviceParameter = meta.Parameters[marker.Parameter.Index];
                marker.Marker.Validate(marker.Parameter.Name, adviceParameter.Value);
            }

            var returnValue = meta.Proceed();

            foreach (var marker in markersOnReturnValue)
            {
                marker.Marker.Validate("return value", returnValue);
            }

            return returnValue;
        }

        [IntroduceMethod]
        private void ValidateDynamic(object value)
        {
            dynamic castValue = meta.FieldOrProperty.Type.Cast(value);

            foreach (var marker in meta.Markers)
            {
                ((ValidateAttribute)marker.Marker).Validate(meta.FieldOrProperty.Name, castValue);
            }
        }

        [OverrideFieldOrPropertySetTemplate]
        private void ValidateFieldOrPropertySetter()
        {
         
            foreach (var marker in meta.Markers)
            {
                ((ValidateAttribute)marker.Marker).Validate(meta.FieldOrProperty.Name, meta.FieldOrProperty.GetValue(meta.This));
            }

            meta.Proceed();

        }

        public void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.AdviceFactory.OverrideMethod(builder.TargetDeclaration, nameof(this.ValidateMethod));
        }

        public void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            // if not dependency property
            // {
            builder.AdviceFactory.OverrideFieldOrPropertyAccessors(builder.TargetDeclaration, null, nameof(this.ValidateFieldOrPropertySetter));

            // } else {
            var introduceAdvice = builder.AdviceFactory.IntroduceMethod(builder.TargetDeclaration.DeclaringType, nameof(this.ValidateDynamic));
            introduceAdvice.Builder.Name = "Validate_" + builder.TargetDeclaration.Name;

            // the dependency property advice would be supposed to use the method Validate_Property if it exists.
            // }
        }

        public void BuildEligibility(IEligibilityBuilder<IMethod> method)
        {
            method.MustBeNonAbstract();
        }

        public void BuildEligibility(IEligibilityBuilder<IFieldOrProperty> member)
        {
            member.MustBeNonAbstract();
        }
    }
}
