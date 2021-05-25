// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

// ReSharper disable UnusedTypeParameter

namespace Caravela.Framework.Eligibility
{
    [Obsolete( "Not implemented." )]
    public static class CodeModelExtensions
    {
        public static EligibilityValue GetEligibility<T>( this IDeclaration declaration )
            where T : IAspect<IDeclaration>
            => throw new NotImplementedException();

        public static EligibilityValue GetEligibility<T>( this IMethod declaration )
            where T : IAspect<IMethod>
            => throw new NotImplementedException();

        public static EligibilityValue GetEligibility<T>( this IMethodBase declaration )
            where T : IAspect<IMethodBase>
            => throw new NotImplementedException();

        public static EligibilityValue GetEligibility<T>( this IMemberOrNamedType declaration )
            where T : IAspect<IMemberOrNamedType>
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