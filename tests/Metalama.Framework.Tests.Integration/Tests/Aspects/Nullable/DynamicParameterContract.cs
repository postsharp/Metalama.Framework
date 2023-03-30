#if TEST_OPTIONS
// @ClearIgnoredDiagnostics to verify nullability warnings
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicParameterContract;

internal class Aspect : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        value?.ToString();
        value!.ToString();
    }
}

// <target>
class TargetCode
{
    class Nullable
    {
        [Aspect]
        public string? Field = null;
        
        [Aspect]
        public string? Property { get; set; }

        [Aspect]
        public string? this[int i] => null;

        [return: Aspect]
        string? Method([Aspect] string? arg) => arg;
    }

    class NotNullable
    {
        [Aspect]
        public string Field = null!;

        [Aspect]
        public string Property { get; set; } = null!;

        [Aspect]
        public string this[int i] => null!;

        [return: Aspect]
        string Method([Aspect] string arg) => arg;
    }

#nullable disable

    class Oblivious
    {
        [Aspect]
        public string Field = null;

        [Aspect]
        public string Property { get; set; }

        [Aspect]
        public string this[int i] => null;

        [return: Aspect]
        string Method([Aspect] string arg) => arg;
    }
}