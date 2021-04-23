// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal static class AdviceAttributeFactory
    {
        public static IAdvice CreateAdvice<T>(
            this IAttribute attribute,
            AspectInstance aspect,
            IDiagnosticAdder diagnosticAdder,
            T declaration,
            ICodeElement templateMethod )
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

                value = default;

                return false;
            }

            var aspectLinkerOptionsAttribute = templateMethod.Attributes.FirstOrDefault(
                x => x.Type == x.Compilation.TypeFactory.GetTypeByReflectionType( typeof(AspectLinkerOptionsAttribute) ) );

            AspectLinkerOptions? aspectLinkerOptions = null;

            if ( aspectLinkerOptionsAttribute != null )
            {
                var linkerOptionsArguments = attribute.NamedArguments.ToDictionary( p => p.Key, p => p.Value );

                var forceNotInlineable = false;

                if ( linkerOptionsArguments.TryGetValue( nameof(AspectLinkerOptionsAttribute.ForceNotInlineable), out var forceNotInlineableValue ) )
                {
                    forceNotInlineable = (bool) forceNotInlineableValue.Value.AssertNotNull();
                }

                aspectLinkerOptions = AspectLinkerOptions.Create( forceNotInlineable );
            }

            switch ( attribute.Type.Name )
            {
                case nameof(IntroduceMethodAttribute):
                    {
                        TryGetNamedArgument<IntroductionScope>( nameof(IntroduceMethodAttribute.Scope), out var scope );
                        TryGetNamedArgument<ConflictBehavior>( nameof(IntroduceMethodAttribute.ConflictBehavior), out var conflictBehavior );

                        var advice = new IntroduceMethodAdvice(
                            aspect,
                            (INamedType) declaration,
                            (IMethod) templateMethod,
                            scope,
                            conflictBehavior,
                            aspectLinkerOptions );

                        advice.Initialize( diagnosticAdder );

                        if ( TryGetNamedArgument<string>( nameof(IntroduceMethodAttribute.Name), out var name ) )
                        {
                            advice.Builder.Name = name;
                        }

                        if ( TryGetNamedArgument<bool>( nameof(IntroduceMethodAttribute.IsVirtual), out var isVirtual ) )
                        {
                            advice.Builder.IsVirtual = isVirtual;
                        }

                        if ( TryGetNamedArgument<bool>( nameof(IntroduceMethodAttribute.IsSealed), out var isSealed ) )
                        {
                            advice.Builder.IsSealed = isSealed;
                        }

                        if ( TryGetNamedArgument<Accessibility>( nameof(IntroduceMethodAttribute.Accessibility), out var accessibility ) )
                        {
                            advice.Builder.Accessibility = accessibility;
                        }

                        advice.Builder.ReturnType = ((IMethod) templateMethod).ReturnType;

                        return advice;
                    }
            }

            throw new NotImplementedException( $"No implementation for advice attribute {attribute.Constructor.DeclaringType}." );
        }
    }
}