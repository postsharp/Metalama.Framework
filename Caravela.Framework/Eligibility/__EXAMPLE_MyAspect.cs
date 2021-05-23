// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Eligibility
{
    // Example of an aspect with complex eligibility rules.
    public class MyAspect : Attribute, IAspect<IMethod>
    {
        public void BuildAspect( IAspectBuilder<IMethod> builder ) => throw new System.NotImplementedException();

        public virtual void BuildEligibility( IEligibilityBuilder<IMethod> builder )
        {
            builder.MustBeNonStatic();
            
            builder
                .DeclaringType()
                .MustSatisfyAll(
                    and =>
                    {
                        and.MustHaveAccessibility( Accessibility.Public );
                        and.MustBeNonAbstract();
                    } );
            
            builder.ReturnType().MustBe( typeof(void) );
            builder.ExceptForInheritance().MustBeNonAbstract();

            builder.Parameter( 0 )
                .Type()
                .MustSatisfyAny(
                    or =>
                    {
                        or.MustBe( typeof(int) );
                        or.MustBe( typeof(string) );
                    } );
        }
    }

    // Example of an aspect that restricts the eligibility of its parent (it cannot loosen it).
    public class MyDerivedAspect : MyAspect
    {
        public override void BuildEligibility( IEligibilityBuilder<IMethod> builder )
        {
            // Must call the base method as the first call.
            base.BuildEligibility( builder );

            builder.MustHaveAccessibility( Accessibility.Public );
        }
    }
}