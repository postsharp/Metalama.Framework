// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;
using DeclarationKind = Metalama.Framework.Code.DeclarationKind;
using EnumerableExtensions = Metalama.Framework.Engine.Collections.EnumerableExtensions;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using OperatorKind = Metalama.Framework.Code.OperatorKind;
using RefKind = Metalama.Framework.Code.RefKind;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    public static class DeclarationExtensions
    {
        public static DeclarationKind GetDeclarationKind( this ISymbol symbol )
            => symbol switch
            {
                INamespaceSymbol => DeclarationKind.Namespace,
                INamedTypeSymbol => DeclarationKind.NamedType,
                IMethodSymbol method =>
                    method.MethodKind switch
                    {
                        MethodKind.Constructor or MethodKind.StaticConstructor => DeclarationKind.Constructor,
                        MethodKind.Destructor => DeclarationKind.Finalizer,
                        _ => DeclarationKind.Method
                    },
                IPropertySymbol => DeclarationKind.Property,
                IFieldSymbol => DeclarationKind.Field,
                ITypeParameterSymbol => DeclarationKind.TypeParameter,
                IAssemblySymbol => DeclarationKind.Compilation,
                IParameterSymbol => DeclarationKind.Parameter,
                IEventSymbol => DeclarationKind.Event,
                ITypeSymbol => DeclarationKind.None,
                IModuleSymbol => DeclarationKind.Compilation,
                _ => throw new ArgumentException( $"Unexpected symbol: {symbol.GetType().Name}.", nameof(symbol) )
            };

        /// <summary>
        /// Select all declarations recursively contained in a given declaration (i.e. all descendants of the tree).
        /// </summary>
        internal static IEnumerable<IDeclaration> GetContainedDeclarations( this IDeclaration declaration )
            => declaration.SelectManyRecursive(
                child => child switch
                {
                    ICompilation compilation => new[] { compilation.GlobalNamespace },
                    INamespace ns => EnumerableExtensions.Concat<IDeclaration>( ns.Namespaces, ns.Types ),
                    INamedType namedType => EnumerableExtensions.Concat<IDeclaration>(
                            namedType.NestedTypes,
                            namedType.Methods,
                            namedType.Constructors,
                            namedType.Fields,
                            namedType.Properties,
                            namedType.Indexers,
                            namedType.Events,
                            namedType.TypeParameters )
                        .ConcatNotNull( namedType.StaticConstructor )
                        .ConcatNotNull( namedType.Finalizer ),
                    IMethod method => method.Parameters
                        .Concat<IDeclaration>( method.TypeParameters )
                        .ConcatNotNull( method.ReturnParameter ),
                    IIndexer indexer => indexer.Parameters.Concat<IDeclaration>( indexer.Accessors ),
                    IConstructor constructor => constructor.Parameters,
                    IHasAccessors member => member.Accessors,
                    _ => Enumerable.Empty<IDeclaration>()
                } );

        internal static Ref<IDeclaration> ToTypedRef( this ISymbol symbol, CompilationContext compilationContext )
            => Ref.FromSymbol( symbol, compilationContext );

        internal static Ref<T> ToTypedRef<T>( this T declaration )
            where T : class, IDeclaration
            => ((IDeclarationImpl) declaration).ToRef().As<T>();

        internal static ISymbol? GetSymbol( this IDeclaration declaration, CompilationContext compilationContext )
            => compilationContext.SymbolTranslator.Translate( declaration.GetSymbol().AssertNotNull(), declaration.GetCompilationModel().RoslynCompilation );

        internal static MemberRef<T> ToMemberRef<T>( this T member )
            where T : class, IMemberOrNamedType
            => new( ((IDeclarationImpl) member).ToRef() );

        internal static Location? GetDiagnosticLocation( this IDeclaration declaration )
            => declaration switch
            {
                IDiagnosticLocationImpl hasLocation => hasLocation.DiagnosticLocation,
                _ => null
            };

        private static void CheckArguments( this IDeclaration declaration, IReadOnlyList<IParameter> parameters, TypedExpressionSyntaxImpl[]? arguments )
        {
            // TODO: somehow provide locations for the diagnostics?
            var argumentsLength = arguments?.Length ?? 0;

            if ( parameters.LastOrDefault()?.IsParams == true )
            {
                // all non-params arguments have to be set + any number of params arguments
                var requiredArguments = parameters.Count - 1;

                if ( argumentsLength < requiredArguments )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresAtLeastNArguments.CreateException( (declaration, requiredArguments, argumentsLength) );
                }
            }
            else
            {
                if ( argumentsLength != parameters.Count )
                {
                    throw GeneralDiagnosticDescriptors.MemberRequiresNArguments.CreateException( (declaration, parameters.Count, argumentsLength) );
                }
            }
        }

        internal static ArgumentSyntax[] GetArguments(
            this IDeclaration declaration,
            IReadOnlyList<IParameter> parameters,
            TypedExpressionSyntaxImpl[]? args,
            SyntaxGenerationContext syntaxGenerationContext )
        {
            CheckArguments( declaration, parameters, args );

            if ( args == null || args.Length == 0 )
            {
                return Array.Empty<ArgumentSyntax>();
            }

            var arguments = new List<ArgumentSyntax>( args.Length );

            for ( var i = 0; i < args.Length; i++ )
            {
                var arg = args[i];

                ArgumentSyntax argument;
                var parameter = parameters[i];

                if ( i >= parameters.Count || parameter.IsParams )
                {
                    // params methods can be called as params or directly with an array
                    // so it's probably best to not do any type-checking for them

                    argument = SyntaxFactory.Argument( arg.Syntax );
                }
                else
                {
                    if ( parameter.RefKind is RefKind.Out or RefKind.Ref )
                    {
                        // With out and ref parameters, we unconditionally add the out or ref modifier, and "hope" the code will later compile.
                        // We also intentionally omit to cast the value since it would be illegal.

                        if ( !arg.IsReferenceable )
                        {
                            throw new DiagnosticException(
                                GeneralDiagnosticDescriptors.CannotPassExpressionToByRefParameter.CreateRoslynDiagnostic(
                                    null,
                                    (arg.Syntax.ToString(), parameter.Name, parameter.DeclaringMember) ) );
                        }

                        var syntax = parameter.RefKind is RefKind.Ref ? SyntaxKind.RefKeyword : SyntaxKind.OutKeyword;

                        argument = SyntaxFactory.Argument( null, SyntaxFactory.Token( syntax ), arg.Syntax );
                    }
                    else
                    {
                        argument = SyntaxFactory.Argument( arg.Convert( parameter.Type, syntaxGenerationContext ).Syntax.RemoveParenthesis() );
                    }
                }

                arguments.Add( argument );
            }

            return arguments.ToArray();
        }

        internal static ExpressionSyntax GetReceiverSyntax<T>(
            this T declaration,
            TypedExpressionSyntaxImpl instance,
            SyntaxGenerationContext generationContext )
            where T : IMember
        {
            if ( declaration.IsStatic )
            {
                return generationContext.SyntaxGenerator.Type( declaration.DeclaringType.GetSymbol() );
            }

            var definition = declaration.GetOriginalDefinition();

            if ( definition.IsExplicitInterfaceImplementation )
            {
                return
                    SyntaxFactory.ParenthesizedExpression(
                        SyntaxFactory.CastExpression(
                            generationContext.SyntaxGenerator.Type( definition.GetExplicitInterfaceImplementation().DeclaringType.GetSymbol() ),
                            instance.Syntax ) );
            }

            return instance.Convert( declaration.DeclaringType, generationContext ).Syntax;
        }

        internal static RefKind ToOurRefKind( this Microsoft.CodeAnalysis.RefKind roslynRefKind )
            => roslynRefKind switch
            {
                Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
                Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
                Microsoft.CodeAnalysis.RefKind.RefReadOnly => RefKind.RefReadOnly,
                _ => throw new InvalidOperationException( $"Roslyn RefKind {roslynRefKind} not recognized here." )
            };

        internal static Accessibility ToOurVisibility( this Microsoft.CodeAnalysis.Accessibility accessibility )
            => accessibility switch
            {
                Microsoft.CodeAnalysis.Accessibility.NotApplicable => Accessibility.Private,
                Microsoft.CodeAnalysis.Accessibility.Private => Accessibility.Private,
                Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => Accessibility.PrivateProtected,
                Microsoft.CodeAnalysis.Accessibility.Protected => Accessibility.Protected,
                Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
                Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => Accessibility.ProtectedInternal,
                Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
                _ => throw new ArgumentOutOfRangeException()
            };

        internal static string ToDisplayString( this DeclarationKind kind )
            => kind switch
            {
                DeclarationKind.TypeParameter => "generic parameter",
                _ => kind.ToString().ToLowerInvariant()
            };

        internal static SyntaxToken ToOperatorKeyword( this OperatorKind operatorKind )
            => operatorKind switch
            {
                OperatorKind.ImplicitConversion => SyntaxFactory.Token( SyntaxKind.ImplicitKeyword ),
                OperatorKind.ExplicitConversion => SyntaxFactory.Token( SyntaxKind.ExplicitKeyword ),
                OperatorKind.Addition => SyntaxFactory.Token( SyntaxKind.PlusToken ),
                OperatorKind.BitwiseAnd => SyntaxFactory.Token( SyntaxKind.AmpersandToken ),
                OperatorKind.BitwiseOr => SyntaxFactory.Token( SyntaxKind.BarToken ),
                OperatorKind.Decrement => SyntaxFactory.Token( SyntaxKind.MinusMinusToken ),
                OperatorKind.Division => SyntaxFactory.Token( SyntaxKind.SlashToken ),
                OperatorKind.Equality => SyntaxFactory.Token( SyntaxKind.EqualsEqualsToken ),
                OperatorKind.ExclusiveOr => SyntaxFactory.Token( SyntaxKind.CaretToken ),
                OperatorKind.False => SyntaxFactory.Token( SyntaxKind.FalseKeyword ),
                OperatorKind.GreaterThan => SyntaxFactory.Token( SyntaxKind.GreaterThanToken ),
                OperatorKind.GreaterThanOrEqual => SyntaxFactory.Token( SyntaxKind.GreaterThanEqualsToken ),
                OperatorKind.Increment => SyntaxFactory.Token( SyntaxKind.PlusPlusToken ),
                OperatorKind.Inequality => SyntaxFactory.Token( SyntaxKind.ExclamationEqualsToken ),
                OperatorKind.LeftShift => SyntaxFactory.Token( SyntaxKind.LessThanLessThanToken ),
                OperatorKind.LessThan => SyntaxFactory.Token( SyntaxKind.LessThanToken ),
                OperatorKind.LessThanOrEqual => SyntaxFactory.Token( SyntaxKind.LessThanEqualsToken ),
                OperatorKind.LogicalNot => SyntaxFactory.Token( SyntaxKind.ExclamationToken ),
                OperatorKind.Modulus => SyntaxFactory.Token( SyntaxKind.PercentToken ),
                OperatorKind.Multiply => SyntaxFactory.Token( SyntaxKind.AsteriskToken ),
                OperatorKind.OnesComplement => SyntaxFactory.Token( SyntaxKind.TildeToken ),
                OperatorKind.RightShift => SyntaxFactory.Token( SyntaxKind.GreaterThanGreaterThanToken ),
                OperatorKind.Subtraction => SyntaxFactory.Token( SyntaxKind.MinusToken ),
                OperatorKind.True => SyntaxFactory.Token( SyntaxKind.TrueKeyword ),
                OperatorKind.UnaryNegation => SyntaxFactory.Token( SyntaxKind.MinusToken ),
                OperatorKind.UnaryPlus => SyntaxFactory.Token( SyntaxKind.PlusToken ),
                _ => throw new AssertionFailedException( $"Unexpected OperatorKind: {operatorKind}." )
            };

        internal static string ToOperatorMethodName( this OperatorKind operatorKind )
            => operatorKind switch
            {
                OperatorKind.ImplicitConversion => WellKnownMemberNames.ImplicitConversionName,
                OperatorKind.ExplicitConversion => WellKnownMemberNames.ExplicitConversionName,
                OperatorKind.Addition => WellKnownMemberNames.AdditionOperatorName,
                OperatorKind.BitwiseAnd => WellKnownMemberNames.BitwiseAndOperatorName,
                OperatorKind.BitwiseOr => WellKnownMemberNames.BitwiseOrOperatorName,
                OperatorKind.Decrement => WellKnownMemberNames.DecrementOperatorName,
                OperatorKind.Division => WellKnownMemberNames.DivisionOperatorName,
                OperatorKind.Equality => WellKnownMemberNames.EqualityOperatorName,
                OperatorKind.ExclusiveOr => WellKnownMemberNames.ExclusiveOrOperatorName,
                OperatorKind.False => WellKnownMemberNames.FalseOperatorName,
                OperatorKind.GreaterThan => WellKnownMemberNames.GreaterThanOperatorName,
                OperatorKind.GreaterThanOrEqual => WellKnownMemberNames.GreaterThanOrEqualOperatorName,
                OperatorKind.Increment => WellKnownMemberNames.IncrementOperatorName,
                OperatorKind.Inequality => WellKnownMemberNames.InequalityOperatorName,
                OperatorKind.LeftShift => WellKnownMemberNames.LeftShiftOperatorName,
                OperatorKind.LessThan => WellKnownMemberNames.LessThanOperatorName,
                OperatorKind.LessThanOrEqual => WellKnownMemberNames.LessThanOrEqualOperatorName,
                OperatorKind.LogicalNot => WellKnownMemberNames.LogicalNotOperatorName,
                OperatorKind.Modulus => WellKnownMemberNames.ModulusOperatorName,
                OperatorKind.Multiply => WellKnownMemberNames.MultiplyOperatorName,
                OperatorKind.OnesComplement => WellKnownMemberNames.OnesComplementOperatorName,
                OperatorKind.RightShift => WellKnownMemberNames.RightShiftOperatorName,
                OperatorKind.Subtraction => WellKnownMemberNames.SubtractionOperatorName,
                OperatorKind.True => WellKnownMemberNames.TrueOperatorName,
                OperatorKind.UnaryNegation => WellKnownMemberNames.UnaryNegationOperatorName,
                OperatorKind.UnaryPlus => WellKnownMemberNames.UnaryPlusOperatorName,
                _ => throw new AssertionFailedException( $"Unexpected OperatorKind: {operatorKind}." )
            };

        internal static bool? IsAutoProperty( this IPropertySymbol symbol )
            => symbol switch
            {
                { IsAbstract: true } => false,
                { DeclaringSyntaxReferences: { Length: > 0 } syntaxReferences } =>
                    syntaxReferences.All(
                        sr =>
                            sr.GetSyntax() is BasePropertyDeclarationSyntax { AccessorList: { } } propertyDecl
                            && propertyDecl.AccessorList.Accessors.All( a => a.Body == null && a.ExpressionBody == null ) ),
                { GetMethod: { } getMethod } => getMethod.IsCompilerGenerated(),
                { SetMethod: { } setMethod } => setMethod.IsCompilerGenerated(),
                _ => null
            };

        internal static bool IsAutoAccessor( this IMethodSymbol symbol )
            => symbol switch
            {
                { IsAbstract: true } => false,
                { DeclaringSyntaxReferences: { Length: > 0 } syntaxReferences } =>
                    syntaxReferences.All(
                        sr =>
                            sr.GetSyntax() is AccessorDeclarationSyntax { Body: null, ExpressionBody: null } ),
                _ => symbol.IsCompilerGenerated()
            };

        internal static bool? IsEventField( this IEventSymbol symbol )
            => symbol switch
            {
                // TODO: partial events.
                { IsAbstract: true } => false,
                { DeclaringSyntaxReferences.Length: > 0 } =>
                    symbol.DeclaringSyntaxReferences.All( sr => sr.GetSyntax() is VariableDeclaratorSyntax ),
                { AddMethod: { } getMethod, RemoveMethod: { } setMethod } => getMethod.IsCompilerGenerated() && setMethod.IsCompilerGenerated(),
                _ => null
            };

        internal static bool? HasInitializer( this IPropertySymbol symbol )
            => symbol switch
            {
                { DeclaringSyntaxReferences.Length: > 0 } =>
                    symbol.DeclaringSyntaxReferences.Any( p => p.GetSyntax().AssertCast<PropertyDeclarationSyntax>().Initializer != null ),
                _ => null
            };

        internal static bool? HasInitializer( this IEventSymbol symbol )
            => symbol switch
            {
                { DeclaringSyntaxReferences.Length: > 0 } =>
                    symbol.DeclaringSyntaxReferences.Any( v => v.GetSyntax().AssertCast<VariableDeclaratorSyntax>().Initializer != null ),
                _ => null
            };

        internal static IMember GetExplicitInterfaceImplementation( this IMember member )
        {
            switch ( member )
            {
                case IMethod method:
                    return method.ExplicitInterfaceImplementations.Single();

                case IProperty property:
                    return property.ExplicitInterfaceImplementations.Single();

                case IEvent @event:
                    return @event.ExplicitInterfaceImplementations.Single();

                default:
                    throw new AssertionFailedException( $"Unexpected member type: {member.GetType()}." );
            }
        }

        public static ImmutableArray<SyntaxReference> GetDeclaringSyntaxReferences( this IDeclaration declaration )
            => ((IDeclarationImpl) declaration).DeclaringSyntaxReferences;

        /// <summary>
        /// Finds a method of given signature that is visible in the specified type, taking into account methods being hidden by other methods.
        /// </summary>
        /// <param name="namedType">Type.</param>
        /// <param name="signatureTemplate">Method that acts as a template for the signature.</param>
        /// <returns>A method of the given signature that is visible from the given type or <c>null</c> if no such method exists.</returns>
        internal static IMethod? FindClosestVisibleMethod( this INamedType namedType, IMethod signatureTemplate )
            => namedType.AllMethods.OfExactSignature( signatureTemplate, false );

        /// <summary>
        /// Finds a method of given signature that is visible in the specified type, taking into account methods being hidden by other methods.
        /// </summary>
        /// <param name="namedType">Type.</param>
        /// <param name="signatureTemplate">Method that acts as a template for the signature.</param>
        /// <returns>A method of the given signature that is visible from the given type or <c>null</c> if no such method exists.</returns>
        internal static IIndexer? FindClosestVisibleIndexer( this INamedType namedType, IIndexer signatureTemplate )
            => namedType.AllIndexers.OfExactSignature( signatureTemplate );

        /// <summary>
        /// Finds a parameterless member in the given type and parent type, taking into account member hiding.
        /// </summary>
        /// <param name="namedType">Type.</param>
        /// <param name="name">Member name.</param>
        /// <returns>A property of the given signature that is visible from the given type or <c>null</c> if no such property exists.</returns>
        internal static IMember? FindClosestUniquelyNamedMember(
            this INamedType namedType,
            string name )
            => namedType.AllProperties.OfName( name ).FirstOrDefault() ??
               (IMember?) namedType.AllFields.OfName( name ).FirstOrDefault() ??
               namedType.AllEvents.OfName( name ).FirstOrDefault();

        internal static bool? IsEventField( this IEvent @event )
        {
            switch ( @event )
            {
                case Event codeEvent:
                    var eventSymbol = codeEvent.GetSymbol().AssertNotNull();

                    return eventSymbol.IsEventField();

                case BuiltEvent builtEvent:
                    return builtEvent.EventBuilder.IsEventField;

                case EventBuilder eventBuilder:
                    return eventBuilder.IsEventField;

                default:
                    throw new AssertionFailedException( $"{@event} is not supported" );
            }
        }

        internal static bool IsFullyBound( this INamedType type )
        {
            return DoesNotContainGenericParameters( type );

            static bool DoesNotContainGenericParameters( IType type )
            {
                switch ( type )
                {
                    case INamedType namedType:
                        return namedType.TypeArguments.All( DoesNotContainGenericParameters );

                    case IArrayType array:
                        return DoesNotContainGenericParameters( array.ElementType );

                    case ITypeParameter:
                        return false;

                    default:
                        return true;
                }
            }
        }

        internal static bool TryGetHiddenDeclaration( this IMemberOrNamedType declaration, [NotNullWhen( true )] out IMemberOrNamedType? hiddenDeclaration )
        {
            if ( declaration is IMember { IsOverride: true } )
            {
                // Override symbol never hides anything.
                hiddenDeclaration = null;

                return false;
            }

            var currentType = declaration.DeclaringType?.BaseType;

            while ( currentType != null )
            {
                switch ( declaration )
                {
                    case IFieldOrProperty or IEvent or INamedType:
                        // Field/properties/events are matched by name. When a base method is hidden, we ignore it (as it may be still accessible).
                        var candidateMember =
                            currentType.Fields.OfName( declaration.Name ).FirstOrDefault()
                            ?? currentType.Properties.OfName( declaration.Name ).FirstOrDefault()
                            ?? currentType.Events.OfName( declaration.Name ).FirstOrDefault()
                            ?? currentType.NestedTypes.OfName( declaration.Name ).FirstOrDefault()
                            ?? (IMemberOrNamedType?) currentType.Methods.OfName( declaration.Name ).FirstOrDefault();

                        if ( candidateMember != null )
                        {
                            hiddenDeclaration = candidateMember;

                            return true;
                        }

                        break;

                    case IIndexer indexer:
                        // Indexers are matched by signature.
                        var candidateIndexer = currentType.Indexers.OfExactSignature( indexer );

                        if ( candidateIndexer != null )
                        {
                            hiddenDeclaration = candidateIndexer;

                            return true;
                        }

                        // No need to look for other declaration types as indexers cannot hide them.

                        break;

                    case IMethod method:
                        // Methods are matched by signature.
                        var candidateMethod = currentType.Methods.OfExactSignature( method );

                        if ( candidateMethod != null )
                        {
                            hiddenDeclaration = candidateMethod;

                            return true;
                        }

                        var candidateNonMethod =
                            currentType.Fields.OfName( declaration.Name ).FirstOrDefault()
                            ?? currentType.Properties.OfName( declaration.Name ).FirstOrDefault()
                            ?? currentType.Events.OfName( declaration.Name ).FirstOrDefault()
                            ?? (IMemberOrNamedType?) currentType.NestedTypes.OfName( declaration.Name ).FirstOrDefault();

                        if ( candidateNonMethod != null )
                        {
                            hiddenDeclaration = candidateNonMethod;

                            return true;
                        }

                        break;

                    default:
                        throw new AssertionFailedException( $"Unsupported declaration: {declaration}" );
                }

                currentType = currentType.BaseType;
            }

            hiddenDeclaration = null;

            return false;
        }

        internal static bool IsImplicitInstanceConstructor( this IConstructor ctor )
        {
            return !ctor.IsStatic && ctor is { IsImplicitlyDeclared: true, DeclaringType.TypeKind: TypeKind.Class or TypeKind.Struct };
        }

        internal static int GetDepthImpl( this IDeclaration declaration ) => declaration.GetCompilationModel().GetDepth( declaration );

        internal static T Translate<T>( this T declaration, ICompilation newCompilation )
            where T : IDeclaration
            => declaration.Compilation == newCompilation ? declaration : (T) ((CompilationModel) newCompilation).Factory.Translate( declaration );
    }
}