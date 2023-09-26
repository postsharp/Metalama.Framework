// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal static class SymbolHelpers
{
    public static OperatorKind GetOperatorKindFromName( string methodName )
        => methodName switch
        {
            WellKnownMemberNames.ImplicitConversionName => OperatorKind.ImplicitConversion,
            WellKnownMemberNames.ExplicitConversionName => OperatorKind.ExplicitConversion,
            WellKnownMemberNames.AdditionOperatorName => OperatorKind.Addition,
            WellKnownMemberNames.BitwiseAndOperatorName => OperatorKind.BitwiseAnd,
            WellKnownMemberNames.BitwiseOrOperatorName => OperatorKind.BitwiseOr,
            WellKnownMemberNames.DecrementOperatorName => OperatorKind.Decrement,
            WellKnownMemberNames.DivisionOperatorName => OperatorKind.Division,
            WellKnownMemberNames.EqualityOperatorName => OperatorKind.Equality,
            WellKnownMemberNames.ExclusiveOrOperatorName => OperatorKind.ExclusiveOr,
            WellKnownMemberNames.FalseOperatorName => OperatorKind.False,
            WellKnownMemberNames.GreaterThanOperatorName => OperatorKind.GreaterThan,
            WellKnownMemberNames.GreaterThanOrEqualOperatorName => OperatorKind.GreaterThanOrEqual,
            WellKnownMemberNames.IncrementOperatorName => OperatorKind.Increment,
            WellKnownMemberNames.InequalityOperatorName => OperatorKind.Inequality,
            WellKnownMemberNames.LeftShiftOperatorName => OperatorKind.LeftShift,
            WellKnownMemberNames.LessThanOperatorName => OperatorKind.LessThan,
            WellKnownMemberNames.LessThanOrEqualOperatorName => OperatorKind.LessThanOrEqual,
            WellKnownMemberNames.LogicalNotOperatorName => OperatorKind.LogicalNot,
            WellKnownMemberNames.ModulusOperatorName => OperatorKind.Modulus,
            WellKnownMemberNames.MultiplyOperatorName => OperatorKind.Multiply,
            WellKnownMemberNames.OnesComplementOperatorName => OperatorKind.OnesComplement,
            WellKnownMemberNames.RightShiftOperatorName => OperatorKind.RightShift,
            WellKnownMemberNames.SubtractionOperatorName => OperatorKind.Subtraction,
            WellKnownMemberNames.TrueOperatorName => OperatorKind.True,
            WellKnownMemberNames.UnaryNegationOperatorName => OperatorKind.UnaryNegation,
            WellKnownMemberNames.UnaryPlusOperatorName => OperatorKind.UnaryPlus,
            _ => OperatorKind.None
        };

    internal static bool? BelongsToCompilation( this ISymbol symbol, CompilationContext compilationContext )
    {
        var assembly = symbol.ContainingAssembly;

        if ( assembly == null )
        {
            return null;
        }

        if ( !compilationContext.Assemblies.TryGetValue( assembly.Identity, out var thisCompilationAssembly ) )
        {
            // If we cannot find the assembly, we cannot make any decision whether this is or not a legit symbol.
            // It can happen that a referenced assembly has symbols to another referenced assembly that is not directly
            // referenced by our compilation.

            return null;
        }

        return assembly.Equals( thisCompilationAssembly );
    }

    [Conditional( "DEBUG" )]
    internal static void ThrowIfBelongsToDifferentCompilationThan( this ISymbol? symbol, CompilationContext compilationContext )
    {
        if ( symbol == null )
        {
            return;
        }

        if ( symbol.BelongsToCompilation( compilationContext ) == false )
        {
            throw new AssertionFailedException( $"The symbol '{symbol}' does not belong to the expected compilation." );
        }
    }

    [Conditional( "DEBUG" )]
    internal static void ThrowIfBelongsToDifferentCompilationThan( this ISymbol? symbol, ISymbol? otherSymbol )
    {
        if ( symbol?.ContainingAssembly == null || otherSymbol?.ContainingAssembly == null )
        {
            return;
        }

        if ( symbol.ContainingAssembly.Identity.Equals( otherSymbol.ContainingAssembly.Identity )
             && !symbol.ContainingAssembly.Equals( otherSymbol.ContainingAssembly ) )
        {
            throw new AssertionFailedException( $"The symbols '{symbol}' and '{otherSymbol}' do not belong to the same compilation." );
        }
    }
}