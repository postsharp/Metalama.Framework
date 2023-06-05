using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateParameters.CompileTimeParameterWithRunTimeType;

internal class MyAspect : TypeAspect
{
    [Template]
    public void Method<[CompileTime] TC, TR>(
        [CompileTime] TC a,
        [CompileTime] TC[] b,
        [CompileTime] TR c,
        [CompileTime] List<TR> d,
        [CompileTime] Target e )
    {
    }
}

// <target>
[MyAspect]
internal class Target
{
}