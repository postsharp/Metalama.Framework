﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.ExistingConflict_Ignore
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceUnaryOperator(
                builder.Target,
                nameof(UnaryOperatorTemplate),
                builder.Target,
                builder.Target,
                OperatorKind.UnaryNegation,
                whenExists: OverrideStrategy.Ignore );

            builder.Advice.IntroduceBinaryOperator(
                builder.Target,
                nameof(BinaryOperatorTemplate),
                builder.Target,
                builder.Target,
                builder.Target,
                OperatorKind.Addition,
                whenExists: OverrideStrategy.Ignore );

            builder.Advice.IntroduceConversionOperator(
                builder.Target,
                nameof(ConversionOperatorTemplate),
                TypeFactory.GetType( typeof(int) ),
                builder.Target,
                whenExists: OverrideStrategy.Ignore );
        }

        [Template]
        public dynamic? UnaryOperatorTemplate( dynamic? x )
        {
            Console.WriteLine( $"Unary operator {meta.Target.Method.OperatorKind}({x})" );

            return meta.Proceed();
        }

        [Template]
        public dynamic? BinaryOperatorTemplate( dynamic? x, dynamic? y )
        {
            Console.WriteLine( $"Binary operator {meta.Target.Method.OperatorKind}({x}, {y})" );

            return meta.Proceed();
        }

        [Template]
        public dynamic? ConversionOperatorTemplate( dynamic? x )
        {
            Console.WriteLine( $"Conversion operator {meta.Target.Method.OperatorKind}({x})" );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public static TargetClass operator -( TargetClass a )
        {
            Console.WriteLine( "This is the original operator." );

            return new TargetClass();
        }

        public static TargetClass operator +( TargetClass a, TargetClass b )
        {
            Console.WriteLine( "This is the original operator." );

            return new TargetClass();
        }

        public static explicit operator TargetClass( int a )
        {
            Console.WriteLine( "This is the original operator." );

            return new TargetClass();
        }
    }
}