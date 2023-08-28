using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_Variants;

// Tests that none of these cases cause compile-time error.

[TemplateProvider]
abstract class B
{
    [Template]
    public abstract void M();

    [Template]
    public virtual void M2() { }
}

class C : B
{
    public override void M() { }

    public override sealed void M2() { }
}

sealed class D : B
{
    public override void M() { }
}

// <target>
class T { }