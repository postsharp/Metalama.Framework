// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Aspects;

public static class AttributeHelper
{
    [return: NotNullIfNotNull( "name" )]
    internal static string? GetShortName( string? name )
    {
        if ( name == null )
        {
            return null;
        }

        Parse( name, out _, out var shortName );

        return shortName;
    }

    public static void Parse( string fullName, out string ns, out string shortName )
    {
        string typeName;
        
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

        shortName = typeName.TrimSuffix( "Attribute" );
    }
}