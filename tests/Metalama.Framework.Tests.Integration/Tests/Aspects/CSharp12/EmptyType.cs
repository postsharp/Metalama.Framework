#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.EmptyType;

class TheAspect : TypeAspect
{
    [Introduce]
    void M() { }
}

// <target>
[TheAspect]
class C;

// <target>
[TheAspect]
struct S;

// Not new in C# 12, included for completeness.
// <target>
[TheAspect]
record R;

// <target>
[TheAspect]
interface I;

#endif