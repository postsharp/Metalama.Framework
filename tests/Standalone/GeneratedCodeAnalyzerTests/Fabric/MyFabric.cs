using Metalama.Framework.Fabrics;

class MyFabric : ProjectFabric
{
    public override void AmendProject(IProjectAmender amender)
    {
        amender.SelectTypes()
            .Where(t => t.FullName == "Fabric.Components.Pages.Counter")
            .SelectMany(t => t.Methods)
            .AddAspectIfEligible<LogAttribute>();
    }
}