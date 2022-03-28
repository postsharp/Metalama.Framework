﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerIntroductionNameProvider : IntroductionNameProvider
    {
        private readonly Dictionary<INamedType, HashSet<string>> _introducedMemberNames;

        public LinkerIntroductionNameProvider()
        {
            this._introducedMemberNames = new Dictionary<INamedType, HashSet<string>>();
        }

        internal override string GetOverrideName( INamedType targetType, AspectLayerId aspectLayer, IMember overriddenMember )
        {
            var shortAspectName = aspectLayer.AspectShortName;
            var shortLayerName = aspectLayer.LayerName;

            string nameHint;

            if ( overriddenMember.IsExplicitInterfaceImplementation )
            {
                var interfaceMember = overriddenMember.GetExplicitInterfaceImplementation();
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
                        ? $"{overriddenMember.Name}_{shortAspectName}_{shortLayerName}"
                        : $"{overriddenMember.Name}_{shortAspectName}";
            }

            return this.FindUniqueName( targetType, nameHint );
        }

        internal override string GetInitializerName( INamedType targetType, AspectLayerId aspectLayer, IMember initializedMember )
        {
            var shortAspectName = aspectLayer.AspectShortName;
            var shortLayerName = aspectLayer.LayerName;

            var nameHint =
                shortLayerName != null
                    ? $"Initialize_{shortAspectName}_{shortLayerName}_{initializedMember.Name}"
                    : $"Initialize_{shortAspectName}_{initializedMember.Name}";

            return this.FindUniqueName( targetType, nameHint );
        }

        internal override string GetInitializationName(
            INamedType targetType,
            AspectLayerId aspectLayer,
            IDeclaration targetDeclaration,
            InitializationReason reason )
        {
            var shortAspectName = aspectLayer.AspectShortName;
            var shortLayerName = aspectLayer.LayerName;

            var targetName = targetDeclaration switch
            {
                INamedType => null,
                IMember member => member.Name,
                _ => throw new AssertionFailedException()
            };

            // TODO: Not optimal.
            var reasonName = reason.ToString().Replace( ", ", "_" );

            var nameHint =
                shortLayerName != null
                    ? targetName != null
                        ? $"{reasonName}_{shortAspectName}_{shortLayerName}_{targetName}"
                        : $"{reasonName}_{shortAspectName}_{shortLayerName}"
                    : targetName != null
                        ? $"{reasonName}_{shortAspectName}_{targetName}"
                        : $"{reasonName}_{shortAspectName}";

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
                if ( !this._introducedMemberNames.TryGetValue( containingType, out var names ) )
                {
                    this._introducedMemberNames[containingType] = names = new HashSet<string>();
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

                if ( this._introducedMemberNames.TryGetValue( containingType, out var names )
                     && names.Where( x => StringComparer.Ordinal.Equals( x, name ) ).Any() )
                {
                    return false;
                }

                return true;
            }
        }
    }
}