#if TEST_OPTIONS
// @KeepDisabledCode
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31218
{
    public sealed class NotNullCheckAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            foreach (var method in builder.Target.Methods.Where(
                         m => !m.IsImplicitlyDeclared && (
                             m.Accessibility == Accessibility.Public || m.Accessibility == Accessibility.Protected
                                                                     || m.IsExplicitInterfaceImplementation ) ))
            {
                foreach (var parameter in method.Parameters.Where(
                             p => p.RefKind is RefKind.None or RefKind.In
                                  && !p.Type.IsNullable.GetValueOrDefault()
                                  && p.Type.IsReferenceType.GetValueOrDefault() ))
                {
                    builder.Advice.AddContract(
                        parameter,
                        nameof(ValidateParameter),
                        args: new { parameterName = parameter.Name } );
                }

                if (method.ReturnType.IsReferenceType.GetValueOrDefault()
                    && !method.ReturnType.IsNullable.GetValueOrDefault())
                {
                    builder.Advice.AddContract(
                        method.ReturnParameter,
                        nameof(ValidateMethodResult),
                        args: new { methodName = method.Name } );
                }
            }

            foreach (var constructor in builder.Target.Constructors.Where(
                         c => !c.IsImplicitlyDeclared && (
                             c.Accessibility == Accessibility.Public || c.Accessibility == Accessibility.Protected ) ))
            {
                foreach (var parameter in constructor.Parameters.Where(
                             p => p.RefKind is RefKind.None or RefKind.In
                                  && !p.Type.IsNullable.GetValueOrDefault()
                                  && p.Type.IsReferenceType.GetValueOrDefault() ))
                {
                    builder.Advice.AddContract(
                        parameter,
                        nameof(ValidateParameter),
                        args: new { parameterName = parameter.Name } );
                }
            }

            foreach (var property in builder.Target.Properties.Where(
                         p => !p.IsImplicitlyDeclared && (
                             ( p.Accessibility == Accessibility.Public || p.Accessibility == Accessibility.Protected
                                                                       || p.IsExplicitInterfaceImplementation )
                             && !p.Type.IsNullable.GetValueOrDefault()
                             && p.Type.IsReferenceType.GetValueOrDefault() ) )
                    )
            {
                builder.Advice.AddContract(
                    property,
                    nameof(ValidatePropertyGetter),
                    ContractDirection.Output,
                    args: new { propertyName = property.Name } );

                builder.Advice.AddContract(
                    property,
                    nameof(ValidatePropertySetter),
                    ContractDirection.Input,
                    args: new { propertyName = property.Name } );
            }
        }

        [Template]
        private void ValidateParameter( dynamic? value, [CompileTime] string parameterName )
        {
            Console.WriteLine( "Aspect" );
        }

        [Template]
        private void ValidateMethodResult( dynamic? value, [CompileTime] string methodName )
        {
            Console.WriteLine( "Aspect" );
        }

        [Template]
        private void ValidatePropertySetter( dynamic? value, [CompileTime] string propertyName )
        {
            Console.WriteLine( "Aspect" );
        }

        [Template]
        private void ValidatePropertyGetter( dynamic? value, [CompileTime] string propertyName )
        {
            Console.WriteLine( "Aspect" );
        }
    }

    // <target>
    [NotNullCheck]
    public record SomeRecord( int X, int Y, string Z );
}