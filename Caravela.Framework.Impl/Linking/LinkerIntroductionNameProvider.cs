// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionNameProvider : IntroductionNameProvider
    {
        private readonly Dictionary<INamedType, HashSet<string>> _overrideNames;

        public LinkerIntroductionNameProvider()
        {
            this._overrideNames = new Dictionary<INamedType, HashSet<string>>();
        }

        internal override string GetOverrideName( INamedType targetType, AspectLayerId aspectLayer, IMember overriddenDeclaration )
        {
            var shortAspectName = aspectLayer.AspectShortName;
            var shortLayerName = aspectLayer.LayerName;

            string nameHint;

            if ( overriddenDeclaration.IsExplicitInterfaceImplementation )
            {
                var interfaceMember = overriddenDeclaration.GetExplicitInterfaceImplementation();
                var cleanInterfaceName = interfaceMember.DeclaringType.Name.Replace( "_", "__" ).Replace( ".", "_" );

                nameHint =
                    shortLayerName != null
                        ? $"{cleanInterfaceName}_{interfaceMember.Name}_{shortAspectName}_{shortLayerName}"
                        : $"{cleanInterfaceName}_{interfaceMember.Name}_{shortAspectName}";
            }
            else
            {
                // TODO: Obviously these replace methods are not very efficient.

                nameHint =
                    shortLayerName != null
                        ? $"{overriddenDeclaration.Name}_{shortAspectName}_{shortLayerName}"
                        : $"{overriddenDeclaration.Name}_{shortAspectName}";
            }

            return this.FindUniqueName( targetType, nameHint );
        }

        private string FindUniqueName( INamedType containingType, string hint )
        {
            if ( CheckName( hint ) )
            {
                AddName( hint );

                return hint;
            }
            else
            {
                for ( var i = 1; /* Nothing */; i++ )
                {
                    var candidate = hint + i;

                    if ( CheckName( candidate ) )
                    {
                        AddName( candidate );

                        return candidate;
                    }
                }
            }

            void AddName( string name )
            {
                if ( !this._overrideNames.TryGetValue( containingType, out var names ) )
                {
                    this._overrideNames[containingType] = names = new HashSet<string>();
                }

                if ( !names.Add( name ) )
                {
                    throw new AssertionFailedException();
                }
            }

            bool CheckName( string name )
            {
                if ( containingType.FieldsAndProperties.OfName( name ).Any() )
                {
                    return false;
                }

                if ( containingType.Methods.OfName( name ).Any() )
                {
                    return false;
                }

                if ( containingType.Events.OfName( name ).Any() )
                {
                    return false;
                }

                if ( containingType.NestedTypes.OfName( name ).Any() )
                {
                    return false;
                }

                if ( this._overrideNames.TryGetValue( containingType, out var names )
                     && names.Where( x => StringComparer.Ordinal.Equals( x, name ) ).Any() )
                {
                    return false;
                }

                return true;
            }
        }
    }
}