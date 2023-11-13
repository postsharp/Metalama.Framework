﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.Suppression;

#if !TESTRUNNER
#pragma warning disable CS0414
#endif

[Suppress]
public class C
{
    // This normally reports CS0414 but it should not be written to HTML because we suppressed it.
    private int _f = 0;
    
}

public class Suppress : TypeAspect
{
    private static SuppressionDefinition _suppression = new( "CS0414" );

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Diagnostics.Suppress( _suppression );
    }
}