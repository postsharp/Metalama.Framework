using System.Runtime.CompilerServices;

#if DEBUG
// Support for Castle dynamic proxies used by FakeItEasy.
[assembly: InternalsVisibleTo( "DynamicProxyGenAssembly2" )]
#endif