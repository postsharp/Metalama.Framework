// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;
using DeclarationKind = Metalama.Framework.Code.DeclarationKind;
using EnumerableExtensions = Metalama.Framework.Engine.Collections.EnumerableExtensions;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

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
                _ => throw new ArgumentException( nameof(symbol), $"Unexpected symbol: {symbol.GetType().Name}." )
            };

        /// <summary>
        /// Gets a value indicating whether a symbol is exposed to the user code model.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsVisible( this ISymbol m )
            => !m.IsImplicitlyDeclared || (m.Kind == SymbolKind.Method && string.Equals( m.MetadataName, ".ctor", StringComparison.Ordinal ));

        /// <summary>
        /// Select all declarations recursively contained in a given declaration (i.e. all children of the tree).
        /// </summary>
        /// <param name="declaration"></param>
        /// <returns></returns>
        public static IEnumerable<IDeclaration> GetContainedDeclarations( this IDeclaration declaration )
            => declaration.SelectManyRecursive(
                child => child switch
                {
                    ICompilation compilation => compilation.Types,
                    INamedType namedType => EnumerableExtensions.Concat<IDeclaration>(
                            namedType.NestedTypes,
                            namedType.Methods,
                            namedType.Constructors,
                            namedType.Fields,
                            namedType.Properties,
                            namedType.Indexers,
                            namedType.Events,
                            namedType.TypeParameters )
                        .ConcatNotNull( namedType.StaticConstructor ),
                    IMethod method => method.Parameters
                        .Concat<IDeclaration>( method.TypeParameters )
                        .ConcatNotNull( method.ReturnParameter ),
                    IMemberWithAccessors member => member.Accessors,
                    _ => null
                } );

        /// <summary>
        /// Select all declarations recursively contained in a given declaration (i.e. all children of the tree).
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static IEnumerable<ISymbol> GetContainedSymbols( this ISymbol symbol )
            => symbol switch
            {
                IAssemblySymbol compilation => compilation.GetTypes(),
                INamedTypeSymbol namedType => namedType.GetMembers().Where( IsVisible ).Concat( namedType.TypeParameters ),
                IMethodSymbol method => method.Parameters.Concat<ISymbol>( method.TypeParameters ),
                IPropertySymbol property => property.Parameters,
                _ => Array.Empty<ISymbol>()
            };

        internal static IEnumerable<AttributeRef> ToAttributeLinks(
            this IEnumerable<AttributeData> attributes,
            ISymbol declaringSymbol,
            Compilation compilation )
            => attributes.Select( a => new AttributeRef( a, Ref.FromSymbol( declaringSymbol, compilation ) ) );

        internal static IEnumerable<AttributeRef> GetAllAttributes( this ISymbol symbol, Compilation compilation )
            => symbol switch
            {
                IMethodSymbol method => method
                    .GetAttributes()
                    .ToAttributeLinks( method, compilation )
                    .Concat(
                        method.GetReturnTypeAttributes()
                            .Select( a => new AttributeRef( a, Ref.ReturnParameter( method, compilation ) ) ) ),
                _ => symbol.GetAttributes().ToAttributeLinks( symbol, compilation )
            };

        internal static Ref<IDeclaration> ToTypedRef( this ISymbol symbol, Compilation compilation ) => Ref.FromSymbol( symbol, compilation );

        internal static Ref<T> ToTypedRef<T>( this T declaration )
            where T : class, IDeclaration
            => ((IDeclarationImpl) declaration).ToRef().As<T>();

        public static ISymbol? GetSymbol( this IDeclaration declaration, Compilation compilation )
            => declaration.GetSymbol().Translate( declaration.GetCompilationModel().RoslynCompilation, compilation );

        internal static MemberRef<T> ToMemberRef<T>( this T member )
            where T : class, IMemberOrNamedType
            => new( ((IDeclarationImpl) member).ToRef() );

        public static Location? GetDiagnosticLocation( this IDeclaration declaration )
            => declaration switch
            {
                IDiagnosticLocationImpl hasLocation => hasLocation.DiagnosticLocation,
                _ => null
            };

        internal static void CheckArguments( this IDeclaration declaration, IReadOnlyList<IParameter> parameters, RunTimeTemplateExpression[]? arguments )
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
            RunTimeTemplateExpression[]? args )
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
                        argument = SyntaxFactory.Argument( arg.ToTypedExpression( parameter.Type ) );
                    }
                }

                arguments.Add( argument );
            }

            return arguments.ToArray();
        }

        internal static ExpressionSyntax GetReceiverSyntax<T>(
            this T declaration,
            RunTimeTemplateExpression instance,
            SyntaxGenerationContext generationContext )
            where T : IMember
        {
            if ( declaration.IsStatic )
            {
                return generationContext.SyntaxGenerator.Type( declaration.DeclaringType.GetSymbol() );
            }

            if ( instance.Syntax.Kind() == SyntaxKind.NullLiteralExpression )
            {
                throw GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember.CreateException( declaration );
            }

            return instance.ToTypedExpression( declaration.DeclaringType, true );
        }

        internal static RefKind ToOurRefKind( this Microsoft.CodeAnalysis.RefKind roslynRefKind )
            => roslynRefKind switch
            {
                Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
                Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
                Microsoft.CodeAnalysis.RefKind.RefReadOnly => RefKind.RefReadOnly,
                _ => throw new InvalidOperationException( $"Roslyn RefKind {roslynRefKind} not recognized here." )
            };

        internal static Microsoft.CodeAnalysis.RefKind ToRoslynRefKind( this RefKind ourRefKind )
            => ourRefKind switch
            {
                RefKind.None => Microsoft.CodeAnalysis.RefKind.None,
                RefKind.Ref => Microsoft.CodeAnalysis.RefKind.Ref,
                RefKind.RefReadOnly => Microsoft.CodeAnalysis.RefKind.RefReadOnly,
                _ => throw new InvalidOperationException( $"RefKind {ourRefKind} not recognized." )
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

        internal static Microsoft.CodeAnalysis.Accessibility ToRoslynAccessibility( this Accessibility accessibility )
            => accessibility switch
            {
                Accessibility.Private => Microsoft.CodeAnalysis.Accessibility.Private,
                Accessibility.PrivateProtected => Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal,
                Accessibility.Protected => Microsoft.CodeAnalysis.Accessibility.Protected,
                Accessibility.Internal => Microsoft.CodeAnalysis.Accessibility.Internal,
                Accessibility.ProtectedInternal => Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal,
                Accessibility.Public => Microsoft.CodeAnalysis.Accessibility.Public,
                _ => throw new ArgumentOutOfRangeException()
            };

        internal static DeclarationModifiers ToDeclarationModifiers( this IMember member )
        {
            var modifiers = DeclarationModifiers.None;

            if ( member.IsAbstract )
            {
                modifiers |= DeclarationModifiers.Abstract;
            }

            if ( member.IsSealed )
            {
                modifiers |= DeclarationModifiers.Sealed;
            }

            if ( member is IField field && field.Writeability == Writeability.ConstructorOnly )
            {
                modifiers |= DeclarationModifiers.ReadOnly;
            }

            if ( member.IsStatic )
            {
                modifiers |= DeclarationModifiers.Static;
            }

            if ( member.IsVirtual )
            {
                modifiers |= DeclarationModifiers.Virtual;
            }

            if ( member.IsOverride )
            {
                modifiers |= DeclarationModifiers.Override;
            }

            if ( member.IsNew )
            {
                modifiers |= DeclarationModifiers.New;
            }

            if ( member.IsAsync )
            {
                modifiers |= DeclarationModifiers.Async;
            }

            return modifiers;
        }

        internal static string ToDisplayString( this DeclarationKind kind )
            => kind switch
            {
                DeclarationKind.TypeParameter => "generic parameter",
                _ => kind.ToString().ToLowerInvariant()
            };

        public static SyntaxToken ToOperatorKeyword( this Code.OperatorKind operatorKind )
            => operatorKind switch
            {
                Code.OperatorKind.ImplicitConversion => SyntaxFactory.Token( SyntaxKind.ImplicitKeyword ),
                Code.OperatorKind.ExplicitConversion => SyntaxFactory.Token( SyntaxKind.ExplicitKeyword ),
                Code.OperatorKind.Addition => SyntaxFactory.Token( SyntaxKind.PlusToken ),
                Code.OperatorKind.BitwiseAnd => SyntaxFactory.Token( SyntaxKind.AmpersandToken ),
                Code.OperatorKind.BitwiseOr => SyntaxFactory.Token( SyntaxKind.BarToken ),
                Code.OperatorKind.Decrement => SyntaxFactory.Token( SyntaxKind.MinusMinusToken ),
                Code.OperatorKind.Division => SyntaxFactory.Token( SyntaxKind.SlashToken ),
                Code.OperatorKind.Equality => SyntaxFactory.Token( SyntaxKind.EqualsEqualsToken ),
                Code.OperatorKind.ExclusiveOr => SyntaxFactory.Token( SyntaxKind.CaretToken ),
                Code.OperatorKind.False => SyntaxFactory.Token( SyntaxKind.FalseKeyword ),
                Code.OperatorKind.GreaterThan => SyntaxFactory.Token( SyntaxKind.GreaterThanToken ),
                Code.OperatorKind.GreaterThanOrEqual => SyntaxFactory.Token( SyntaxKind.GreaterThanEqualsToken ),
                Code.OperatorKind.Increment => SyntaxFactory.Token( SyntaxKind.PlusPlusToken ),
                Code.OperatorKind.Inequality => SyntaxFactory.Token( SyntaxKind.ExclamationEqualsToken ),
                Code.OperatorKind.LeftShift => SyntaxFactory.Token( SyntaxKind.LessThanLessThanToken ),
                Code.OperatorKind.LessThan => SyntaxFactory.Token( SyntaxKind.LessThanToken ),
                Code.OperatorKind.LessThanOrEqual => SyntaxFactory.Token( SyntaxKind.LessThanEqualsToken ),
                Code.OperatorKind.LogicalNot => SyntaxFactory.Token( SyntaxKind.ExclamationToken ),
                Code.OperatorKind.Modulus => SyntaxFactory.Token( SyntaxKind.PercentToken ),
                Code.OperatorKind.Multiply => SyntaxFactory.Token( SyntaxKind.AsteriskToken ),
                Code.OperatorKind.OnesComplement => SyntaxFactory.Token( SyntaxKind.TildeToken ),
                Code.OperatorKind.RightShift => SyntaxFactory.Token( SyntaxKind.GreaterThanGreaterThanToken ),
                Code.OperatorKind.Subtraction => SyntaxFactory.Token( SyntaxKind.MinusToken ),
                Code.OperatorKind.True => SyntaxFactory.Token( SyntaxKind.TrueKeyword ),
                Code.OperatorKind.UnaryNegation => SyntaxFactory.Token( SyntaxKind.UnaryMinusExpression ),
                Code.OperatorKind.UnaryPlus => SyntaxFactory.Token( SyntaxKind.UnaryPlusExpression ),
                _ => throw new AssertionFailedException(),
            };

        public static string ToOperatorMethodName( this Code.OperatorKind operatorKind )
            => operatorKind switch
            {
                Code.OperatorKind.ImplicitConversion => WellKnownMemberNames.ImplicitConversionName,
                Code.OperatorKind.ExplicitConversion => WellKnownMemberNames.ExplicitConversionName,
                Code.OperatorKind.Addition => WellKnownMemberNames.AdditionOperatorName,
                Code.OperatorKind.BitwiseAnd => WellKnownMemberNames.BitwiseAndOperatorName,
                Code.OperatorKind.BitwiseOr => WellKnownMemberNames.BitwiseOrOperatorName,
                Code.OperatorKind.Decrement => WellKnownMemberNames.DecrementOperatorName,
                Code.OperatorKind.Division => WellKnownMemberNames.DivisionOperatorName,
                Code.OperatorKind.Equality => WellKnownMemberNames.EqualityOperatorName,
                Code.OperatorKind.ExclusiveOr => WellKnownMemberNames.ExclusiveOrOperatorName,
                Code.OperatorKind.False => WellKnownMemberNames.FalseOperatorName,
                Code.OperatorKind.GreaterThan => WellKnownMemberNames.GreaterThanOperatorName,
                Code.OperatorKind.GreaterThanOrEqual => WellKnownMemberNames.GreaterThanOrEqualOperatorName,
                Code.OperatorKind.Increment => WellKnownMemberNames.IncrementOperatorName,
                Code.OperatorKind.Inequality => WellKnownMemberNames.InequalityOperatorName,
                Code.OperatorKind.LeftShift => WellKnownMemberNames.LeftShiftOperatorName,
                Code.OperatorKind.LessThan => WellKnownMemberNames.LessThanOperatorName,
                Code.OperatorKind.LessThanOrEqual => WellKnownMemberNames.LessThanOrEqualOperatorName,
                Code.OperatorKind.LogicalNot => WellKnownMemberNames.LogicalNotOperatorName,
                Code.OperatorKind.Modulus => WellKnownMemberNames.ModulusOperatorName,
                Code.OperatorKind.Multiply => WellKnownMemberNames.MultiplyOperatorName,
                Code.OperatorKind.OnesComplement => WellKnownMemberNames.OnesComplementOperatorName,
                Code.OperatorKind.RightShift => WellKnownMemberNames.RightShiftOperatorName,
                Code.OperatorKind.Subtraction => WellKnownMemberNames.SubtractionOperatorName,
                Code.OperatorKind.True => WellKnownMemberNames.TrueOperatorName,
                Code.OperatorKind.UnaryNegation => WellKnownMemberNames.UnaryNegationOperatorName,
                Code.OperatorKind.UnaryPlus => WellKnownMemberNames.UnaryPlusOperatorName,
                _ => throw new AssertionFailedException(),
            };

        public static DeclarationKind ToDeclarationKind( this Code.OperatorKind operatorKind )
            => operatorKind.IsConversionOperator()
            ? DeclarationKind.ConversionOperator
            : DeclarationKind.UserDefinedOperator;

        public static bool IsBinaryOperator( this Code.OperatorKind operatorKind )
            => operatorKind switch
            {
                Code.OperatorKind.ImplicitConversion => false,
                Code.OperatorKind.ExplicitConversion => false,
                Code.OperatorKind.Addition => true,
                Code.OperatorKind.BitwiseAnd => true,
                Code.OperatorKind.BitwiseOr => true,
                Code.OperatorKind.Decrement => false,
                Code.OperatorKind.Division => true,
                Code.OperatorKind.Equality => true,
                Code.OperatorKind.ExclusiveOr => true,
                Code.OperatorKind.False => false,
                Code.OperatorKind.GreaterThan => true,
                Code.OperatorKind.GreaterThanOrEqual => true,
                Code.OperatorKind.Increment => false,
                Code.OperatorKind.Inequality => true,
                Code.OperatorKind.LeftShift => true,
                Code.OperatorKind.LessThan => true,
                Code.OperatorKind.LessThanOrEqual => true,
                Code.OperatorKind.LogicalNot => false,
                Code.OperatorKind.Modulus => true,
                Code.OperatorKind.Multiply => true,
                Code.OperatorKind.OnesComplement => false,
                Code.OperatorKind.RightShift => true,
                Code.OperatorKind.Subtraction => true,
                Code.OperatorKind.True => false,
                Code.OperatorKind.UnaryNegation => false,
                Code.OperatorKind.UnaryPlus => false,
                _ => throw new AssertionFailedException(),
            };

        public static bool IsUnaryOperator( this Code.OperatorKind operatorKind )
            => operatorKind switch
            {
                Code.OperatorKind.ImplicitConversion => false,
                Code.OperatorKind.ExplicitConversion => false,
                Code.OperatorKind.Addition => false,
                Code.OperatorKind.BitwiseAnd => false,
                Code.OperatorKind.BitwiseOr => false,
                Code.OperatorKind.Decrement => true,
                Code.OperatorKind.Division => false,
                Code.OperatorKind.Equality => false,
                Code.OperatorKind.ExclusiveOr => false,
                Code.OperatorKind.False => true,
                Code.OperatorKind.GreaterThan => false,
                Code.OperatorKind.GreaterThanOrEqual => false,
                Code.OperatorKind.Increment => true,
                Code.OperatorKind.Inequality => false,
                Code.OperatorKind.LeftShift => false,
                Code.OperatorKind.LessThan => false,
                Code.OperatorKind.LessThanOrEqual => false,
                Code.OperatorKind.LogicalNot => true,
                Code.OperatorKind.Modulus => false,
                Code.OperatorKind.Multiply => false,
                Code.OperatorKind.OnesComplement => true,
                Code.OperatorKind.RightShift => false,
                Code.OperatorKind.Subtraction => false,
                Code.OperatorKind.True => true,
                Code.OperatorKind.UnaryNegation => true,
                Code.OperatorKind.UnaryPlus => true,
                _ => throw new AssertionFailedException(),
            };

        public static bool IsConversionOperator( this Code.OperatorKind operatorKind )
            => operatorKind switch
            {
                Code.OperatorKind.ImplicitConversion => true,
                Code.OperatorKind.ExplicitConversion => true,
                Code.OperatorKind.Addition => false,
                Code.OperatorKind.BitwiseAnd => false,
                Code.OperatorKind.BitwiseOr => false,
                Code.OperatorKind.Decrement => false,
                Code.OperatorKind.Division => false,
                Code.OperatorKind.Equality => false,
                Code.OperatorKind.ExclusiveOr => false,
                Code.OperatorKind.False => false,
                Code.OperatorKind.GreaterThan => false,
                Code.OperatorKind.GreaterThanOrEqual => false,
                Code.OperatorKind.Increment => false,
                Code.OperatorKind.Inequality => false,
                Code.OperatorKind.LeftShift => false,
                Code.OperatorKind.LessThan => false,
                Code.OperatorKind.LessThanOrEqual => false,
                Code.OperatorKind.LogicalNot => false,
                Code.OperatorKind.Modulus => false,
                Code.OperatorKind.Multiply => false,
                Code.OperatorKind.OnesComplement => false,
                Code.OperatorKind.RightShift => false,
                Code.OperatorKind.Subtraction => false,
                Code.OperatorKind.True => false,
                Code.OperatorKind.UnaryNegation => false,
                Code.OperatorKind.UnaryPlus => false,
                _ => throw new AssertionFailedException(),
            };

        internal static bool IsAutoProperty( this IPropertySymbol symbol )
            => symbol switch
            {
                { IsAbstract: true } => false,
                { DeclaringSyntaxReferences: { Length: > 0 } syntaxReferences } =>
                    syntaxReferences.All(
                        sr =>
                            sr.GetSyntax() is BasePropertyDeclarationSyntax propertyDecl
                            && propertyDecl.AccessorList != null
                            && propertyDecl.AccessorList.Accessors.All( a => a.Body == null && a.ExpressionBody == null ) ),
                { GetMethod: { } getMethod, SetMethod: { } setMethod } => getMethod.IsCompilerGenerated() && setMethod.IsCompilerGenerated(),
                _ => false
            };

        internal static bool IsEventField( this IEventSymbol symbol )
            => !symbol.IsAbstract
               && symbol.DeclaringSyntaxReferences.All( sr => sr.GetSyntax() is VariableDeclaratorSyntax );

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
                    throw new AssertionFailedException();
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
        public static IMethod? FindClosestVisibleMethod( this INamedType namedType, IMethod signatureTemplate )
            =>
            signatureTemplate switch
            {
                { MethodKind: Code.MethodKind.UserDefinedOperator or Code.MethodKind.ConversionOperator } => namedType.AllOperators.OfExactSignature( signatureTemplate, matchIsStatic: false ),
                _ => namedType.AllMethods.OfExactSignature( signatureTemplate, matchIsStatic: false ),
            };

        /// <summary>
        /// Finds a parameterless member in the given type and parent type, taking into account member hiding.
        /// </summary>
        /// <param name="namedType">Type.</param>
        /// <param name="name">Member name.</param>
        /// <returns>A property of the given signature that is visible from the given type or <c>null</c> if no such property exists.</returns>
        public static IMember? FindClosestUniquelyNamedMember(
            this INamedType namedType,
            string name )
            => namedType.AllProperties.OfName( name ).FirstOrDefault() ??
               (IMember?) namedType.AllFields.OfName( name ).FirstOrDefault() ??
               namedType.AllEvents.OfName( name ).FirstOrDefault();
    }
}