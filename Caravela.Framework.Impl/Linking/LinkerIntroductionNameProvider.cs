// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionNameProvider : IntroductionNameProvider
    {
        internal override string GetOverrideName( AspectLayerId aspectLayer, IMethod overriddenDeclaration )
        {
            // TODO: Obviously these replace methods are not very efficient.
            var cleanAspectName = aspectLayer.AspectName.Replace( "_", "__" ).Replace( ".", "_" );
            
            // ReSharper disable once ConstantConditionalAccessQualifier
            var cleanLayerName = aspectLayer.LayerName?.Replace( "_", "__" )?.Replace( ".", "_" );

            return
                cleanLayerName != null
                    ? $"__{overriddenDeclaration.Name}__{cleanAspectName}__{cleanLayerName}"
                    : $"__{overriddenDeclaration.Name}__{cleanAspectName}";
        }
    }
}