using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Bugs.TemplateInStaticClass;

[CompileTime]
 static class MethodBuilder
{
    [Template]
    static void DoSomething() { }
}

// <target>
class TargetCode { }