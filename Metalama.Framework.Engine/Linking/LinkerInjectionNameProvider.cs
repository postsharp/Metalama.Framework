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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class LinkerInjectionNameProvider : InjectionNameProvider
    {
        private readonly Regex _interfaceMemberReplaceRegex = new Regex("_|\\.", RegexOptions.Compiled);

        private readonly CompilationModel _finalCompilation;
        private readonly LinkerInjectionHelperProvider _injectionHelperProvider;
        private readonly ConcurrentDictionary<INamedType, ConcurrentSet<string>> _injectedMemberNames;
        private readonly ConcurrentDictionary<(Type AspectType, IMember OverriddenMember), int> _overriddenByCounters;
        private readonly ConcurrentDictionary<(INamedType Type, string Hint), StrongBox<int>> _nameCollisionCounters;
        private readonly OurSyntaxGenerator _syntaxGenerator;

        public LinkerInjectionNameProvider(
            CompilationModel finalCompilation,
            LinkerInjectionHelperProvider injectionHelperProvider,
            OurSyntaxGenerator syntaxGenerator )
        {
            this._finalCompilation = finalCompilation;
            this._injectionHelperProvider = injectionHelperProvider;
            this._injectedMemberNames = new ConcurrentDictionary<INamedType, ConcurrentSet<string>>( finalCompilation.Comparers.Default );
            this._overriddenByCounters = new ConcurrentDictionary<(Type AspectType, IMember OverriddenMember), int>();
            this._nameCollisionCounters = new ConcurrentDictionary<(INamedType Type, string Hint), StrongBox<int>>();
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
                var cleanInterfaceName = 
                    this._interfaceMemberReplaceRegex.Replace( 
                        interfaceMember.DeclaringType.Name, 
                        m => 
                            m.Value switch 
                            { 
                                "_" => "__", 
                                "." => "_", 
                                _ => throw new AssertionFailedException() 
                            } );

                nameHint =
                    shortLayerName != null
                        ? $"{cleanInterfaceName}_{interfaceMember.Name}_{shortAspectName}_{shortLayerName}"
                        : $"{cleanInterfaceName}_{interfaceMember.Name}_{shortAspectName}";
            }
            else
            {
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
            // TODO: COunter update can be optimized but having multiple indexer overrides is not likely.

            var ordinal = this._overriddenByCounters.AddOrUpdate( (aspect.AspectClass.Type, overriddenMember), 0, ( _, v ) => v + 1 );

            return this._injectionHelperProvider.GetOverriddenByType( this._syntaxGenerator, aspect.AspectClass, ordinal );
        }

        private string FindUniqueName( INamedType containingType, string hint )
        {
            // PERF: Do not add into cache until the second collision is encountered.
            //       We want to be optimistic for that most of time one of the following happens:
            //          1) Hint does not collide.
            //          2) Hint does collide, but appending "1" resolves the collision.
            //       In other cases we allocate the cache and next time we work with it.

            // THR:  This method may get called on multiple threads. If parallelization is using by-type grouping instead of by-syntax tree, 
            //       most of the synchronization (allocating and accessing the strong box) can be removed.

            if ( !this._nameCollisionCounters.TryGetValue( (containingType, hint), out var counter ) )
            {
                var finalContainingType = containingType.Translate( this._finalCompilation );
                var injectedMemberNames = this._injectedMemberNames.GetOrAdd( finalContainingType, _ => new ConcurrentSet<string>() );

                // Collision counter was not yet initialized, therefore final compilation members need to be checked.
                if ( CheckFinalMemberNames( finalContainingType, hint ) )
                {
                    if ( injectedMemberNames.Add( hint ) )
                    {
                        // There is no collision with compilation members and we have reserved the name.
                        this._nameCollisionCounters.AddOrUpdate(
                            (finalContainingType, hint),
                            new StrongBox<int>( 1 ),
                            ( k, v ) =>
                            {
                                // If the strong box is already present, it means that other thread ran through before us, we have to only assert.
                                Invariant.Assert( v.Value > 1 );
                                return v;
                            } );

                        return hint;
                    }
                    else
                    {
                        // There was collision with a name that was already added by this algorithm.
                        // Example: "foo", "foo1" declared in code, FindUniqueName executes 11 times for "foo".
                        //          When FindUniqueName executes for "foo1", it will try to start the chain with "foo11".
                        // Resolution: add a counter for "2" and proceed to the slow path.
                        counter = this._nameCollisionCounters.AddOrUpdate( (finalContainingType, hint), new StrongBox<int>( 2 ), ( k, v ) => v );

                        return FindAndUpdate( finalContainingType, injectedMemberNames, hint, counter );
                    }
                }
                else
                {
                    counter = this._nameCollisionCounters.AddOrUpdate( (finalContainingType, hint), new StrongBox<int>( 2 ), (k, v) => v );

                    return FindAndUpdate( finalContainingType, injectedMemberNames, hint, counter );
                }
            }
            else
            {
                var injectedMemberNames = this._injectedMemberNames.GetOrAdd( containingType, _ => new ConcurrentSet<string>() );

                return FindAndUpdate( containingType, injectedMemberNames, hint, counter );
            }

            static string FindAndUpdate( INamedType finalContainingType, ConcurrentSet<string> injectedMemberNames, string hint, StrongBox<int> counter )
            {
                lock ( counter )
                {
                    while (true)
                    {
                        var candidate = $"{hint}{counter.Value++}";

                        if ( CheckFinalMemberNames( finalContainingType, hint ) && injectedMemberNames.Add( candidate ) )
                        {
                            return candidate;
                        }
                    }
                }
            }

            static bool CheckFinalMemberNames( INamedType type, string name )
            {
                if ( type.FieldsAndProperties.OfName( name ).Any() )
                {
                    return false;
                }

                if ( type.Methods.OfName( name ).Any() )
                {
                    return false;
                }

                if ( type.Events.OfName( name ).Any() )
                {
                    return false;
                }

                if ( type.NestedTypes.OfName( name ).Any() )
                {
                    return false;
                }

                return true;
            }
        }
    }
}