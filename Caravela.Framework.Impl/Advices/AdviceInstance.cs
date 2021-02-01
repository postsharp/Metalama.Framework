using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl.Advices
{
    static class AdviceAttributeFactory
    {
        public static T CreateAdvice<T>( this IAdviceAttribute<T> attribute, IAspect aspect, ICodeElement declaration, ICodeElement target )
            where T : class, IAdvice
        {
            switch ( attribute )
            {
                case OverrideMethodAttribute overrideMethodAttr:
                    return (T) (object) new OverrideMethodAdvice( aspect, ( IMethod) target, (IMethod) declaration );
                case IntroduceMethodAttribute introduceMethodAttr:
                    return (T) (object) new IntroduceMethodAdvice( aspect, (INamedType) target, (IMethod) declaration )
                    {
                        Name = introduceMethodAttr.Name,
                        Scope = introduceMethodAttr.Scope,
                        IsStatic = introduceMethodAttr.IsStatic,
                        IsVirtual = introduceMethodAttr.IsVirtual,
                        Visibility = introduceMethodAttr.Visibility
                    };
            }

            throw new InvalidOperationException( "Unknown advice." );
        }
    }
}