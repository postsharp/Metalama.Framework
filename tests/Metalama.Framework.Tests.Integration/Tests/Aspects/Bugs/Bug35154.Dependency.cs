using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug35154;

[Inheritable]
[AttributeUsage( AttributeTargets.Class )]
public class Aspect1 : TypeAspect { }

[Aspect1]
public class BaseClass { }