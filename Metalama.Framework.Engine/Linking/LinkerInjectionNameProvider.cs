// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerInjectionNameProvider : InjectionNameProvider
    {
        private readonly Type[] _digitOrdinalTypes;
        private readonly ConcurrentDictionary<INamedType, ConcurrentSet<string>> _injectedMemberNames;
        private readonly ConcurrentDictionary<(Type AspectType, IMember OverriddenMember), int> _overriddenByCounters;

        public LinkerInjectionNameProvider( CompilationModel finalCompilationModel )
        {
            this._injectedMemberNames = new ConcurrentDictionary<INamedType, ConcurrentSet<string>>( finalCompilationModel.Comparers.Default );
            this._overriddenByCounters = new ConcurrentDictionary<(Type AspectType, IMember OverriddenMember), int>();

            this._digitOrdinalTypes = new[]
            {
                typeof(OverrideOrdinal._0),
                typeof(OverrideOrdinal._1),
                typeof(OverrideOrdinal._2),
                typeof(OverrideOrdinal._3),
                typeof(OverrideOrdinal._4),
                typeof(OverrideOrdinal._5),
                typeof(OverrideOrdinal._6),
                typeof(OverrideOrdinal._7),
                typeof(OverrideOrdinal._8),
                typeof(OverrideOrdinal._9)
            };
        }

        internal override string GetOverrideName( INamedType targetType, AspectLayerId aspectLayer, IMember overriddenMember )
        {
            var shortAspectName = aspectLayer.AspectShortName;
            var shortLayerName = aspectLayer.LayerName;

            string nameHint;

            if ( overriddenMember.IsExplicitInterfaceImplementation )
            {
                var interfaceMember = overriddenMember.GetExplicitInterfaceImplementation();
                var cleanInterfaceName = interfaceMember.DeclaringType.Name.ReplaceOrdinal( "_", "__" ).ReplaceOrdinal( ".", "_" );

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
            InitializerKind reason )
        {
            var shortAspectName = aspectLayer.AspectShortName;
            var shortLayerName = aspectLayer.LayerName;

            var targetName = targetDeclaration switch
            {
                INamedType => null,
                IMember member => member.Name,
                _ => throw new AssertionFailedException( $"Unexpected declaration: '{targetDeclaration}'." )
            };

            // TODO: Not optimal.
            var reasonName = reason.ToString().ReplaceOrdinal( ", ", "_" );

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

        internal override TypeSyntax GetOverriddenByType( IAspectInstanceInternal aspect, IMember overriddenMember )
        {
            var ordinal = this._overriddenByCounters.AddOrUpdate( (aspect.AspectClass.Type, overriddenMember), 0, ( _, v ) => v + 1 );

            switch ( ordinal )
            {
                case 0:
                    return OurSyntaxGenerator.Default.Type(
                        ((CompilationModel) overriddenMember.Compilation).Factory.GetTypeByReflectionType(
                            typeof(OverriddenBy<>).MakeGenericType( aspect.AspectClass.Type ) )
                        .GetSymbol() );

                case < 10:
                    return OurSyntaxGenerator.Default.Type(
                        ((CompilationModel) overriddenMember.Compilation).Factory.GetTypeByReflectionType(
                            typeof(OverriddenBy<,>).MakeGenericType(
                                aspect.AspectClass.Type,
                                this._digitOrdinalTypes[ordinal] ) )
                        .GetSymbol() );

                case < 100:
                    return OurSyntaxGenerator.Default.Type(
                        ((CompilationModel) overriddenMember.Compilation).Factory.GetTypeByReflectionType(
                            typeof(OverriddenBy<,>).MakeGenericType(
                                aspect.AspectClass.Type,
                                typeof(OverrideOrdinal.C<,>).MakeGenericType(
                                    this._digitOrdinalTypes[ordinal / 10],
                                    this._digitOrdinalTypes[ordinal % 10] ) ) )
                        .GetSymbol() );

                default:
                    // NOTE: Lets have a beer when someone really hits this limit (without having a bug in the aspect).
                    throw new AssertionFailedException( $"More than 100 overrides of {overriddenMember} by aspect {aspect.AspectClass.ShortName}." );
            }
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
                var names = this._injectedMemberNames.GetOrAddNew( containingType );

                if ( !names.Add( name ) )
                {
                    throw new AssertionFailedException( $"The name '{name}' is not unique." );
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

                if ( this._injectedMemberNames.TryGetValue( containingType, out var names )
                     && names.Contains( name ) )
                {
                    return false;
                }

                return true;
            }
        }
    }
}