﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.ParameterMapping
{
    /*
     * Verifies that template parameters are correctly mapped by name.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceBinaryOperator(
                builder.Target,
                nameof(InvertedParameterNames),
                builder.Target,
                TypeFactory.GetType(typeof(int)),
                TypeFactory.GetType(typeof(int)),
                OperatorKind.Addition,
                buildOperator: b =>
                {
                    b.Parameters[0].Name = "y";
                    b.Parameters[1].Name = "x";
                } );

            builder.Advice.IntroduceUnaryOperator(
                builder.Target,
                nameof(ParameterName),
                builder.Target,
                TypeFactory.GetType(typeof(int)),
                OperatorKind.UnaryNegation,
                buildOperator: b =>
                {
                    b.Parameters[0].Name = "y";
                });

            builder.Advice.IntroduceConversionOperator(
                builder.Target,
                nameof(ParameterName),
                builder.Target,
                TypeFactory.GetType(typeof(int)),
                true,
                buildOperator: b =>
                {
                    b.Parameters[0].Name = "y";
                });
        }

        [Template]
        public int InvertedParameterNames(dynamic? x, int y)
        {
            return x!.ToString().Length + y;
        }

        [Template]
        public int ParameterName(dynamic? x)
        {
            return x!.ToString().Length;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}