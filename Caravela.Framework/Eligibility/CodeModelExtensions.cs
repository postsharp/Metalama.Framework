using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Eligibility
{
    public static class CodeModelExtensions
    {
        public static EligibilityValue GetEligibility<T>( this IAspectTarget declaration )
            where T : IAspect<IAspectTarget>
            => throw new NotImplementedException();
        
        public static EligibilityValue GetEligibility<T>( this IMethod declaration )
            where T : IAspect<IMethod>
            => throw new NotImplementedException();
        
        public static EligibilityValue GetEligibility<T>( this IMethodBase declaration )
            where T : IAspect<IMethodBase>
            => throw new NotImplementedException();
        
        public static EligibilityValue GetEligibility<T>( this IMember declaration )
            where T : IAspect<IMember>
            => throw new NotImplementedException();
        
        public static EligibilityValue GetEligibility<T>( this INamedType declaration )
            where T : IAspect<INamedType>
            => throw new NotImplementedException();

        public static EligibilityValue GetEligibility<T>( this IFieldOrProperty declaration )
            where T : IAspect<IFieldOrProperty>
            => throw new NotImplementedException();

        public static EligibilityValue GetEligibility<T>( this IField declaration )
            where T : IAspect<IField>
            => throw new NotImplementedException();

        public static EligibilityValue GetEligibility<T>( this IProperty declaration )
            where T : IAspect<IProperty>
            => throw new NotImplementedException();

        public static EligibilityValue GetEligibility<T>( this IEvent declaration )
            where T : IAspect<IEvent>
            => throw new NotImplementedException();

        
    }
}