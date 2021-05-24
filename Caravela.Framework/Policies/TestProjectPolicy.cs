// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Eligibility;
using System;
using System.Linq;

namespace Caravela.Framework.Policies
{
    internal class TestProjectPolicy : IProjectPolicy
    {
        public void BuildPolicy( IProjectPolicyBuilder builder )
        {
            builder.WithTypes( compilation => compilation.DeclaredTypes.DerivedFrom( typeof(IDisposable) ).Where( t => !t.IsAbstract ) )
                .WithMembers( t => t.Methods.Where( m => m.GetEligibility<MyAspect>() == EligibilityValue.Eligible ) )
                .AddAspect<MyAspect>();
        }
    }
}