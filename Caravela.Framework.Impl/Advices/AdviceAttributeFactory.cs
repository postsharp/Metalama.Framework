// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.Advices
{
    internal static class AdviceAttributeFactory
    {
        public static Advice CreateAdvice<T>(
            this AttributeData attribute,
            AspectInstance aspect,
            IDiagnosticAdder diagnosticAdder,
            T aspectTargetDeclaration,
            IDeclaration templateDeclaration,
            string? layerName )
            where T : IDeclaration
        {
            var namedArguments = attribute.NamedArguments.ToDictionary( p => p.Key, p => p.Value );

            bool TryGetNamedArgument<TArg>( string name, [NotNullWhen( true )] out TArg? value )
            {
                if ( namedArguments.TryGetValue( name, out var objectValue ) && objectValue.Value != null )
                {
                    value = (TArg) objectValue.Value;

                    return true;
                }

                value = default;

                return false;
            }

            var aspectLinkerOptionsAttribute = templateDeclaration.Attributes.FirstOrDefault(
                x => x.Type == x.Compilation.TypeFactory.GetTypeByReflectionType( typeof(AspectLinkerOptionsAttribute) ) );

            var adviceOptions = AdviceOptions.Default;

            if ( aspectLinkerOptionsAttribute != null )
            {
                var linkerOptionsArguments = attribute.NamedArguments.ToDictionary( p => p.Key, p => p.Value );

                var forceNotInlineable = false;

                if ( linkerOptionsArguments.TryGetValue( nameof(AspectLinkerOptionsAttribute.ForceNotInlineable), out var forceNotInlineableValue ) )
                {
                    forceNotInlineable = (bool) forceNotInlineableValue.Value.AssertNotNull();
                }

                adviceOptions = adviceOptions.WithLinkerOptions( forceNotInlineable );
            }

            switch ( attribute.AttributeClass?.Name )
            {
                case nameof(IntroduceAttribute):
                    {
                        TryGetNamedArgument<IntroductionScope>( nameof(IntroduceAttribute.Scope), out var scope );
                        TryGetNamedArgument<ConflictBehavior>( nameof(IntroduceAttribute.ConflictBehavior), out var conflictBehavior );
                        IMemberBuilder builder;
                        Advice advice;

                        switch ( templateDeclaration )
                        {
                            case IMethod templateMethod:
                                var introduceMethodAdvice = new IntroduceMethodAdvice(
                                    aspect,
                                    (INamedType) aspectTargetDeclaration,
                                    templateMethod,
                                    scope,
                                    conflictBehavior,
                                    layerName,
                                    adviceOptions );

                                advice = introduceMethodAdvice;
                                builder = introduceMethodAdvice.Builder;

                                break;

                            case IProperty templateProperty:
                                var introducePropertyAdvice = new IntroducePropertyAdvice(
                                    aspect,
                                    (INamedType) aspectTargetDeclaration,
                                    templateProperty,
                                    null,
                                    null,
                                    null,
                                    scope,
                                    conflictBehavior,
                                    layerName,
                                    adviceOptions );

                                advice = introducePropertyAdvice;
                                builder = introducePropertyAdvice.Builder;

                                break;

                            case IEvent templateEvent:
                                var introduceEventAdvice = new IntroduceEventAdvice(
                                    aspect,
                                    (INamedType) aspectTargetDeclaration,
                                    templateEvent,
                                    null,
                                    null,
                                    null,
                                    scope,
                                    conflictBehavior,
                                    layerName,
                                    adviceOptions );

                                advice = introduceEventAdvice;
                                builder = introduceEventAdvice.Builder;

                                break;

                            default:
                                throw new AssertionFailedException();
                        }

                        advice.Initialize( null, diagnosticAdder );

                        if ( TryGetNamedArgument<string>( nameof(IntroduceAttribute.Name), out var name ) )
                        {
                            builder.Name = name;
                        }

                        if ( TryGetNamedArgument<bool>( nameof(IntroduceAttribute.IsVirtual), out var isVirtual ) )
                        {
                            builder.IsVirtual = isVirtual;
                        }

                        if ( TryGetNamedArgument<bool>( nameof(IntroduceAttribute.IsSealed), out var isSealed ) )
                        {
                            builder.IsSealed = isSealed;
                        }

                        if ( TryGetNamedArgument<Accessibility>( nameof(IntroduceAttribute.Accessibility), out var accessibility ) )
                        {
                            builder.Accessibility = accessibility;
                        }

                        return advice;
                    }
            }

            throw new NotImplementedException( $"No implementation for advice attribute {attribute.AttributeClass}." );
        }
    }
}