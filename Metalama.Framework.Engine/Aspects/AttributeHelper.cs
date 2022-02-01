// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Aspects;

public static class AttributeHelper
{
    [return: NotNullIfNotNull( "name" )]
    public static string? GetShortName( string? name )
    {
        if ( name == null )
        {
            return null;
        }
        
        Parse( name, out _, out _, out var shortName );

        return shortName;
    }

    public static void Parse( string fullName, out string ns, out string typeName, out string shortName )
    {
        var lastDot = fullName.LastIndexOf( '.' );
        
        if ( lastDot >= 0 )
        {
            ns = fullName.Substring( 0, lastDot );
            typeName = fullName.Substring( lastDot + 1 );
        }
        else
        {
            ns = "";
            typeName = fullName;
        }

        shortName = typeName.TrimEnd( "Attribute" );
    }
}