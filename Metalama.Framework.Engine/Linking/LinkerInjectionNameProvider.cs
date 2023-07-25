// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class LinkerInjectionNameProvider : InjectionNameProvider
    {
        private readonly LinkerInjectionHelperProvider _injectionHelperProvider;
        private readonly ConcurrentDictionary<INamedType, ConcurrentSet<string>> _injectedMemberNames;
        private readonly ConcurrentDictionary<(Type AspectType, IMember OverriddenMember), int> _overriddenByCounters;
        private readonly OurSyntaxGenerator _syntaxGenerator;

        public LinkerInjectionNameProvider(
            CompilationModel finalCompilationModel,
            LinkerInjectionHelperProvider injectionHelperProvider,
            OurSyntaxGenerator syntaxGenerator )
        {
            this._injectionHelperProvider = injectionHelperProvider;
            this._injectedMemberNames = new ConcurrentDictionary<INamedType, ConcurrentSet<string>>( finalCompilationModel.Comparers.Default );
            this._overriddenByCounters = new ConcurrentDictionary<(Type AspectType, IMember OverriddenMember), int>();
            this._syntaxGenerator = syntaxGenerator;
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

            return this._injectionHelperProvider.GetOverriddenByType( this._syntaxGenerator, aspect.AspectClass, ordinal );
        }

        private string FindUniqueName( INamedType containingType, string hint )
        {
            int? hintIndex = null;

            while ( true )
            {
                if ( hintIndex == null )
                {
                    if ( CheckAndAddName( hint ) )
                    {
                        return hint;
                    }
                    else
                    {
                        hintIndex = 1;
                    }
                }
                else
                {
                    var candidate = hint + hintIndex.Value;

                    if ( CheckAndAddName( candidate ) )
                    {
                        return candidate;
                    }
                    else
                    {
                        hintIndex++;
                    }
                }
            }

            bool CheckAndAddName( string name )
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

                var injectedNames = this._injectedMemberNames.GetOrAddNew( containingType );

                return injectedNames.Add( name );
            }
        }
    }
}