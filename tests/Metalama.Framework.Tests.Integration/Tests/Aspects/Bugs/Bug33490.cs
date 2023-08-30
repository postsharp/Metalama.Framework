// @Skipped(#33490 - serialization of nullability and tuple element names)

using System;
using System.IO;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33490;

class TestAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceMethod(builder.Target, nameof(Template));
    }

    [Template]
    int Template()
    {
        var compileTimeTuple1 = meta.CompileTime((x: 1, y: 2));
        var compileTimeTuple2 = meta.CompileTime<(int x, int y)>((1, 2));
        var compileTimeTupleArray = meta.CompileTime(new[] { (x: 1, y: 2) });
        var compileTimeNullable1 = meta.CompileTime(new string[] { "A" });
        var compileTimeNullable2 = meta.CompileTime(new string?[] { "A" });
        var runTimeTuple1 = meta.RunTime(compileTimeTuple1);
        var runTimeTuple2 = meta.RunTime(compileTimeTuple2);
        var runTimeTupleArray = meta.RunTime(compileTimeTupleArray);
        var runTimeNullable1 = meta.RunTime(compileTimeNullable1);
        var runTimeNullable2 = meta.RunTime(compileTimeNullable2);
        return runTimeTuple1.x + runTimeTuple1.y + runTimeTuple2.x + runTimeTuple2.y + runTimeTupleArray[0].x + runTimeTupleArray[0].y;
    }
}

// <target>
[TestAspect]
class Target
{
}