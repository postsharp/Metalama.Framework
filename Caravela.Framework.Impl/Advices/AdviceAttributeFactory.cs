using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal static class AdviceAttributeFactory
    {
        public static IAdvice CreateAdvice<T>( this IAttribute attribute, IAspect aspect, T declaration, ICodeElement templateMethod )
            where T : ICodeElement
        {
            switch ( attribute.Type.Name )
            {
                case nameof(OverrideMethodAttribute):
                    return new OverrideMethodAdvice( aspect, (IMethod) declaration, (IMethod) templateMethod );
                case nameof( IntroduceMethodAttribute ):
                    return new IntroduceMethodAdvice( aspect, (INamedType) declaration, (IMethod) templateMethod )
                    {
                        Name = attribute.NamedArguments.ContainsKey( nameof( IntroduceMethodAdvice.Name ) ) ? (string?)attribute.NamedArguments[nameof(IntroduceMethodAdvice.Name)] : null,
                        Scope = attribute.NamedArguments.ContainsKey( nameof( IntroduceMethodAdvice.Scope ) ) ? (IntroductionScope?) attribute.NamedArguments[nameof( IntroduceMethodAdvice.Scope )] : null,
                        IsStatic = attribute.NamedArguments.ContainsKey( nameof( IntroduceMethodAdvice.IsStatic ) ) ? (bool?) attribute.NamedArguments[nameof( IntroduceMethodAdvice.IsStatic )] : null,
                        IsVirtual = attribute.NamedArguments.ContainsKey( nameof( IntroduceMethodAdvice.IsVirtual ) ) ? (bool?) attribute.NamedArguments[nameof( IntroduceMethodAdvice.IsVirtual )] : null,
                        Visibility = attribute.NamedArguments.ContainsKey( nameof( IntroduceMethodAdvice.Visibility ) ) ? (Visibility?) attribute.NamedArguments[nameof( IntroduceMethodAdvice.Visibility )] : null,
                    };
            }

            throw new NotImplementedException( $"No implementation for advice attribute {attribute.Constructor.DeclaringType}." );
        }
    }
}