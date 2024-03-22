namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.PartialMethodSymbolPart;

// <target>
internal partial class TargetCode
{
    partial void M();

    partial void M()
    {
    }
}