<Query Kind="Statements">
  <NuGetReference Prerelease="true">Caravela.LinqPad</NuGetReference>
  <Namespace>Caravela.Framework.Workspaces</Namespace>
  <Namespace>Caravela.Framework.Code</Namespace>
</Query>

var projects = Workspace.Default.Load(@"C:\src\caravela\Caravela.sln");

projects.Diagnostics
	.Where(d => d.Severity >= Caravela.Framework.Diagnostics.Severity.Warning)
	.GroupBy(g => g.Id)
	.Select(x => new { Id = x.Key, Count = x.Count() })
	.OrderByDescending(g => g.Count)
	.Chart(x => x.Id, x => x.Count)
	.Dump();