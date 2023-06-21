using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Aspects.Eligibility.TypeOf_CrossAssembly;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
class TypeAttribute : Attribute
{
    public TypeAttribute(string type) { }
}

// <target>
public partial class TargetClass
{
    [TestAspect]
    [Type(nameof(RunTimeClass))]
    [Type(nameof(RunTimeOrCompileTimeClass))]
    [Type(nameof(CompileTimeClass))]
    void M1() { }

    [TestAspect]
    [Type(nameof(RunTimeClass))]
    [Type(nameof(RunTimeOrCompileTimeClass))]
    void M2() { }

    [TestAspect]
    [Type(nameof(RunTimeClass))]
    void M3() { }

    [TestAspect]
    void M4() { }
}