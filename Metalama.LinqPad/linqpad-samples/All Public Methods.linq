<Query Kind="Expression">
  <NuGetReference>Caravela.LinqPad</NuGetReference>
  <Namespace>Caravela.Framework.Workspaces</Namespace>
  <Namespace>Caravela.Framework.Code</Namespace>
</Query>

// For proper formatting of the dump output, add this to My Extensions as a top-level method:
// public static object ToDump(object obj) => Caravela.LinqPad.CaravelaDumper.ToDump(obj);

WorkspaceCollection.Default.Load(@"C:\src\caravela\Caravela.sln")
	.Methods
	.Where( m => m.Accessibility ==  Caravela.Framework.Code.Accessibility.Public && m.DeclaringType.Accessibility == Caravela.Framework.Code.Accessibility.Public )
	.GroupBy( m => m.DeclaringType.FullName )
	.OrderBy( g => g.Key )
	
	

