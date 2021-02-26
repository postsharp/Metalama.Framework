// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Runtime.CompilerServices;

#if DEBUG
// Support for Castle dynamic proxies used by FakeItEasy.
[assembly: InternalsVisibleTo( "DynamicProxyGenAssembly2" )]
#endif