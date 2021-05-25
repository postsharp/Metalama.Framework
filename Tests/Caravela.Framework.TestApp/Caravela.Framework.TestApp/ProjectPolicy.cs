using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Policies;
using Caravela.Framework.TestApp.Aspects.Eligibility;

namespace Caravela.Framework.TestApp
{
     internal class ProjectPolicy : IProjectPolicy
    {
        public void BuildPolicy(IProjectPolicyBuilder builder)
        {
            builder
                .WithTypes(compilation => compilation.DeclaredTypes.DerivedFrom(typeof(IDisposable)).Where(t => !t.IsAbstract))
                .WithMembers(t => t.Methods.Where(m => m.GetEligibility<MyAspect>() == EligibilityValue.Eligible))
                .AddAspect<MyAspect>();
        }
    }
}
