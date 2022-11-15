<Query Kind="Expression">
  <NuGetReference>Metalama.LinqPad</NuGetReference>
  <Namespace>Metalama.Framework.Workspaces</Namespace>
  <Namespace>Metalama.Framework.Code</Namespace>
</Query>

// For proper formatting of the dump output, add this to My Extensions as a top-level method:
// public static object ToDump(object obj) => Metalama.LinqPad.MetalamaDumper.ToDump(obj);

WorkspaceCollection.Default.Load(@"C:\src\metalama\Metalama.sln")
    .SourceCode
	.Methods
	.Where( m => m.Accessibility ==  Metalama.Framework.Code.Accessibility.Public && m.DeclaringType.Accessibility == Metalama.Framework.Code.Accessibility.Public )
	.GroupBy( m => m.DeclaringType.FullName )
	.OrderBy( g => g.Key )
	
	

