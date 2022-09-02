// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
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
    }
}