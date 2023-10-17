using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.Roslyn;

[CompileTime]
internal static class DeclarationExtensions
{
    public static string? GetDocumentationCommentId( this IDeclaration metalamaDeclaration )
    {
        var roslynSymbol = metalamaDeclaration.GetSymbol();

        return roslynSymbol?.GetDocumentationCommentId();
    }
}

internal class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var methodName = meta.Target.Method.Name;
        var methodDocumentationCommentId = meta.Target.Method.GetDocumentationCommentId();
        Console.WriteLine( $"Starting {methodName}, doc ID {methodDocumentationCommentId}." );

        return meta.Proceed();
    }
}

// <target>
public class TargetClass
{
    [Log]
    public static void TargetMethod() { }
}