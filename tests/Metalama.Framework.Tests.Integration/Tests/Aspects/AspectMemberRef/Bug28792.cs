using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.Win32;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AspectMembersRef.Bug28792
{
    internal class RegistryStorageAttribute : TypeAspect
    {
        public string Key { get; }

        public RegistryStorageAttribute( string key )
        {
            Key = "HKEY_CURRENT_USER\\SOFTWARE\\Company\\Product\\" + key;
        }

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var property in builder.Target.FieldsAndProperties.Where( p => p.IsAutoPropertyOrField == true && !p.IsImplicitlyDeclared ))
            {
                builder.Advice.Override( property, nameof(OverrideProperty) );
            }
        }

        [Template]
        private dynamic? OverrideProperty
        {
            get
            {
                var value = Registry.GetValue( Key, meta.Target.FieldOrProperty.Name, null );

                if (value != null)
                {
                    return Convert.ChangeType( value, meta.Target.FieldOrProperty.Type.ToType() );
                }
                else
                {
                    return meta.Target.FieldOrProperty.Type.DefaultValue();
                }
            }
        }
    }

    // <target>
    [RegistryStorage( "Animals" )]
    internal class Animals
    {
        public int Turtles { get; set; }
    }
}