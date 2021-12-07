<Query Kind="Expression">
  <NuGetReference Prerelease="true">Caravela.LinqPad</NuGetReference>
  <Namespace>Caravela.Framework.Workspaces</Namespace>
  <Namespace>Caravela.Framework.Code</Namespace>
</Query>

// For proper formatting of the dump output, add this to My Extensions as a top-level method:
// public static object ToDump(object obj) => Caravela.LinqPad.CaravelaDumper.ToDump(obj);

WorkspaceCollection.Default.Load(@"C:\src\caravela\Caravela.sln")
    .Diagnostics
	.Where(d => d.Severity >= Caravela.Framework.Diagnostics.Severity.Warning)
	.GroupBy(g => (g.Declaration switch
	{
		IMemberOrNamedType memberOrNamedType => memberOrNamedType.DeclaringType ?? memberOrNamedType,
		_ => null
	})?.ToString() ?? "<null>" )
	.Where( g => g.Key != null )
	.Select(x => new { Declaration = x.Key, Count = x.Count() })
	.OrderBy(g => g.Count)
	.Chart(x => x.Declaration.ToString(), x => x.Count)
	.Dump();