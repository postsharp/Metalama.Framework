<Query Kind="Expression">
  <NuGetReference Prerelease="true">Metalama.LinqPad</NuGetReference>
  <Namespace>Metalama.Framework.Workspaces</Namespace>
  <Namespace>Metalama.Framework.Code</Namespace>
</Query>

// For proper formatting of the dump output, add this to My Extensions as a top-level method:
// public static object ToDump(object obj) => Metalama.LinqPad.MetalamaDumper.ToDump(obj);

WorkspaceCollection.Default.Load(@"C:\src\metalama\Metalama.sln")
    .Diagnostics
	.Where(d => d.Severity >= Metalama.Framework.Diagnostics.Severity.Warning)
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