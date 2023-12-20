This test check that an app built using Metalama can be executed in an environment with strong name validation not bypassed.

https://learn.microsoft.com/en-us/dotnet/standard/assembly/disable-strong-name-bypass-feature

An example of such app is an ASP.NET app running in IIS.

We only test this for run time, as we ship build-time public signed assemblies that are not strong-name-signed, so the default strong name validation bypass is required in build time.