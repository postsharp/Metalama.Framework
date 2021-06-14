// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionNameProvider : IntroductionNameProvider
    {
        internal override string GetOverrideName( AspectLayerId aspectLayer, IMember overriddenDeclaration )
        {
            // TODO: Obviously these replace methods are not very efficient.
            var cleanAspectName = aspectLayer.AspectName.Replace( "_", "__" ).Replace( ".", "_" );
            var cleanLayerName = aspectLayer.LayerName?.Replace( "_", "__" ).Replace( ".", "_" );

            return
                cleanLayerName != null
                    ? $"__Override__{overriddenDeclaration.Name}__By__{cleanAspectName}__{cleanLayerName}"
                    : $"__Override__{overriddenDeclaration.Name}__By__{cleanAspectName}";
        }

        internal override string GetInterfaceProxyName( AspectLayerId aspectLayer, IMember interfaceMember )
        {
            var cleanAspectName = aspectLayer.AspectName.Replace( "_", "__" ).Replace( ".", "_" );
            var cleanLayerName = aspectLayer.LayerName?.Replace( "_", "__" ).Replace( ".", "_" );
            var cleanInterfaceName = interfaceMember.DeclaringType.FullName.Replace( "_", "__" ).Replace( ".", "_" );

            return
                cleanLayerName != null
                    ? $"__InterfaceImpl__{cleanInterfaceName}__{interfaceMember.Name}__By__{cleanAspectName}__{cleanLayerName}"
                    : $"__InterfaceImpl__{cleanInterfaceName}__{interfaceMember.Name}__By__{cleanAspectName}";
        }
    }
}