using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Issue32571;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Issue32571;

[Inheritable]
[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public class Aspect1 : TypeAspect { }

[Aspect1]
[Aspect1]
public class BaseClass { }