<Query Kind="Statements">
  <NuGetReference Prerelease="true">Caravela.Framework.Workspaces</NuGetReference>
  <Namespace>Caravela.Framework.Workspaces</Namespace>
  <Namespace>Caravela.Framework.Code</Namespace>
</Query>

var project = Project.Load(@"C:\src\Caravela\Caravela.Framework\Caravela.Framework.csproj");

project.Compilation.Dump();