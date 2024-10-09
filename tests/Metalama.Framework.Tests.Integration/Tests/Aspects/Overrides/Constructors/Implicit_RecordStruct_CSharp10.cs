﻿#if TEST_OPTIONS
// @LanguageVersion(10)
#endif

using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Implicit_RecordStruct_CSharp10;

/*
 * Tests single OverrideConstructor advice on an implicit constructor of a record struct.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.With( builder.Target.Constructors.Single( c => c is { Parameters.Count: 0 } ) ).Override( nameof(Template) );
    }

    [Template]
    public void Template()
    {
        Console.WriteLine( "This is the override." );
        meta.Proceed();
    }
}

// <target>
[Override]
public record struct TargetStruct { }