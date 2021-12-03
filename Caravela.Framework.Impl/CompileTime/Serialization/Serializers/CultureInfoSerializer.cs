// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using Caravela.Framework.Serialization;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    /// <exclude/>
    // This needs to be public because the type is instantiated from an activator in client assemblies.
    public class CultureInfoSerializer : ReferenceTypeMetaSerializer
    {
        /// <exclude/>
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
#if CULTURE_INFO_USER_OVERRIDE
            if (constructorArguments.TryGetValue( "useUserOverride", out bool useUserOverride ))
                return new CultureInfo( constructorArguments.GetValue<string>( "identifier" ), useUserOverride );
#endif
            // This is returned if we're running .NETStandard1.3 or useUserOverride was not set.
            return new CultureInfo( constructorArguments.GetValue<string>( "identifier" ) );
        }

        /// <exclude/>
        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var info = (CultureInfo) obj;
            constructorArguments.SetValue( "identifier", info.Name );
            constructorArguments.SetValue( "useUserOverride", info.UseUserOverride );
        }

        /// <exclude/>
        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
        }
    }
}