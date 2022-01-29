// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Aspects;

public static class AttributeHelper
{
    [return: NotNullIfNotNull( "name" )]
    public static string? GetShortName( string? name ) => name?.TrimEnd( "Attribute" );
}