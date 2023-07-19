
using System;
using Castle.DynamicProxy.Generators;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33467;

public class B { }

file static class BExtensions
{
    public static B Bar(this B b) => b;
}