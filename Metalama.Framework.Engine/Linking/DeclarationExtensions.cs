// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking;

internal static class DeclarationExtensions
{
    public static bool IsEventFieldIntroduction( this IEventSymbol @event )
    {
        if ( @event.IsEventField() == true )
        {
            return true;
        }

        if ( @event.GetPrimaryDeclaration() is { } primaryDeclaration
             && primaryDeclaration.GetLinkerDeclarationFlags().HasFlagFast( AspectLinkerDeclarationFlags.EventField ) )
        {
            return true;
        }

        return false;
    }
}