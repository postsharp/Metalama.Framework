﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating;

internal static partial class SyntaxFactoryEx
{
    private static readonly Dictionary<Type, MethodInfo> _syntaxFactoryMethods = typeof(SyntaxFactory).GetMethods( BindingFlags.Static | BindingFlags.Public )
        .Where( m => m.Name == "Literal" && m.GetParameters().Length == 4 )
        .ToDictionary( x => x.GetParameters()[2].ParameterType, x => x );

    private class LiteralFormatter<T>
    {
        public static readonly LiteralFormatter<T> Instance = new();

        private readonly Func<T, int, SyntaxToken> _func;

        private LiteralFormatter()
        {
            var objectDisplayType = typeof(CSharpSyntaxNode).Assembly.GetType( "Microsoft.CodeAnalysis.CSharp.ObjectDisplay" ).AssertNotNull();
            var objectDisplayOptionsType = typeof(SyntaxNode).Assembly.GetType( "Microsoft.CodeAnalysis.ObjectDisplayOptions" ).AssertNotNull();
            var objectDisplayTypeMethods = objectDisplayType.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
            var formatPrimitiveMethod = objectDisplayTypeMethods.Single( m => m.Name == "FormatLiteral" && m.GetParameters()[0].ParameterType == typeof(T) );

            var literalMethod = _syntaxFactoryMethods[typeof(T)];

            var valueParameter = Expression.Parameter( typeof(T) );
            var optionsParameter = Expression.Parameter( typeof(int) );
            var callFormatPrimitiveArguments = new List<Expression>( 3 ) { valueParameter, Expression.Convert( optionsParameter, objectDisplayOptionsType ) };

            if ( formatPrimitiveMethod.GetParameters().Length == 3 )
            {
                callFormatPrimitiveArguments.Add( Expression.Default( typeof(CultureInfo) ) );
            }

            var callFormatPrimitive =
                Expression.Call(
                    formatPrimitiveMethod,
                    callFormatPrimitiveArguments );

            var callLiteralMethod = Expression.Call(
                literalMethod,
                Expression.Default( typeof(SyntaxTriviaList) ),
                callFormatPrimitive,
                valueParameter,
                Expression.Default( typeof(SyntaxTriviaList) ) );

            this._func = Expression.Lambda<Func<T, int, SyntaxToken>>( callLiteralMethod, valueParameter, optionsParameter ).Compile();
        }

        public SyntaxToken Format( T value, ObjectDisplayOptions options ) => this._func( value, (int) options );
    }
}