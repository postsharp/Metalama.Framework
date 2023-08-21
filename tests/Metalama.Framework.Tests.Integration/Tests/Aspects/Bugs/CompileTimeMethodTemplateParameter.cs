using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CompileTimeMethodTemplateParameter;

class AspectAttribute : TypeAspect
{
    [Template]
    private void Template1(int a) => IntMethod(a);

    [CompileTime]
    int IntMethod(int i) => i;

    [Template]
    private void Template2(int b) => VoidMethod(b);

    [CompileTime]
    void VoidMethod(int j) { }
}