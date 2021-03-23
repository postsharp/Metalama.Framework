// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.Advices
{
    internal static class AdviceAttributeFactory
    {
        public static IAdvice CreateAdvice<T>( this IAttribute attribute, AspectInstance aspect, T declaration, ICodeElement templateMethod )
            where T : ICodeElement
        {
            var namedArguments = attribute.NamedArguments.ToDictionary( p => p.Key, p => p.Value );

            bool TryGetNamedArgument<TArg>( string name, [NotNullWhen( true )] out TArg? value )
            {
                if ( namedArguments.TryGetValue( name, out var objectValue ) && objectValue.Value != null )
                {
                    value = (TArg) objectValue.Value;
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }

            switch ( attribute.Type.Name )
            {
                case nameof( OverrideMethodAttribute ):
                    return new OverrideMethodAdvice( aspect, (IMethod) declaration, (IMethod) templateMethod );
                case nameof( IntroduceMethodAttribute ):
                    {
                        var advice = new IntroduceMethodAdvice( aspect, (INamedType) declaration, (IMethod) templateMethod );

                        if ( TryGetNamedArgument<string>( nameof( IntroduceMethodAttribute.Name ), out var name ) )
                        {
                            advice.Builder.Name = name;
                        }

                        if ( TryGetNamedArgument<IntroductionScope>( nameof( IntroduceMethodAttribute.Scope ), out _ ) )
                        {
                            // TODO: handle scope.
                        }

                        if ( TryGetNamedArgument<bool>( nameof( IntroduceMethodAttribute.IsStatic ), out var isStatic ) )
                        {
                            advice.Builder.IsStatic = isStatic;
                        }

                        if ( TryGetNamedArgument<bool>( nameof( IntroduceMethodAttribute.IsVirtual ), out var isVirtual ) )
                        {
                            advice.Builder.IsVirtual = isVirtual;
                        }

                        if ( TryGetNamedArgument<bool>( nameof( IntroduceMethodAttribute.IsSealed ), out var isSealed ) )
                        {
                            advice.Builder.IsSealed = isSealed;
                        }

                        if ( TryGetNamedArgument<Accessibility>( nameof( IntroduceMethodAttribute.Visibility ), out var visibility ) )
                        {
                            advice.Builder.Accessibility = visibility;
                        }

                        return advice;
                    }
            }

            throw new NotImplementedException( $"No implementation for advice attribute {attribute.Constructor.DeclaringType}." );
        }
    }
}