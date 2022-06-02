﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class CSharpAttributeHelperTests : IDisposable
    {
        private readonly ITestOutputHelper _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly AdhocWorkspace _workspace = new();
        private Document? _testFileDocument;

        public CSharpAttributeHelperTests( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        public void Dispose() => this._workspace.Dispose();

        private void LogAndAssertContains<T>( IEnumerable<T> enumerable, T expectedItem, int expectedOccurrences = 1 )
        {
            this._logger.WriteLine( "Expected:" );
            this._logger.WriteLine( expectedItem!.ToString() );
            this._logger.WriteLine( "Actual:" );

            var actualOccurrences = 0;

            foreach ( var item in enumerable )
            {
                this._logger.WriteLine( item?.ToString() ?? "<null>" );

                if ( expectedItem.Equals( item ) )
                {
                    actualOccurrences++;
                }
            }

            Assert.Equal( expectedOccurrences, actualOccurrences );
        }

        private void LogAndAssertContains( IEnumerable<AttributeSyntax> attributes, string expectedAttributeName, int expectedOccurrences = 1 )
        {
            this.LogAndAssertContains( attributes.Select( a => a.Name.ToString() ), expectedAttributeName, expectedOccurrences );
        }

        [Fact]
        public async Task Can_AddAttributeToMethodAsync()
        {
            var syntax = @"
public class Class
{
    public void Method1() { }
}
";

            var originalMethodDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First();

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalMethodDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .AttributeLists
                .SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeWithArgumentsAndPropertiesAsync()
        {
            var syntax = @"
public class Class
{
    public void Method1() { }
}
";

            SyntaxNode originalMethodDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First();

            var attributeDescription = new AttributeDescription(
                name: "TestAttribute",
                constructorArguments: new[] { "ARG1", "ARG2" }.ToImmutableList(),
                namedArguments: new List<(string Name, string Value)> { ("Prop1", "111"), ("Prop2", "222") }.ToImmutableList() );

            var newRoot = await this.AddAttributeAsync( originalMethodDeclaration, attributeDescription );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .AttributeLists
                .Select( list => list.ToString() );

            this.LogAndAssertContains( resultAttributes, "[TestAttribute(ARG1, ARG2, Prop1 = 111, Prop2 = 222)]" );
        }

        [Fact]
        public async Task Can_AddAttributeToClassAsync()
        {
            var syntax = @"
public class Class
{
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .First()
                .AttributeLists
                .SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToInterfaceAsync()
        {
            var syntax = @"
public interface IInterface
{
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .First();

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<InterfaceDeclarationSyntax>()
                .First()
                .AttributeLists
                .SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToDelegateAsync()
        {
            var syntax = @"
public delegate void Delegate();
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<DelegateDeclarationSyntax>()
                .First();

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<DelegateDeclarationSyntax>()
                .First()
                .AttributeLists
                .SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToEnumAsync()
        {
            var syntax = @"
public enum Enum
{
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<EnumDeclarationSyntax>()
                .First();

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<EnumDeclarationSyntax>()
                .First()
                .AttributeLists
                .SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToPropertyAccessorAsync()
        {
            var syntax = @"
public class Class
{
    public int P1 { get; set; }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .First( x => x.Keyword.ValueText == "get" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<AccessorDeclarationSyntax>()
                .First( x => x.Keyword.ValueText == "get" )
                .AttributeLists.SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToEventAsync()
        {
            var syntax = @"
public class Class
{
    public event EventHandler { add { } remove { } }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<EventDeclarationSyntax>()
                .First();

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<EventDeclarationSyntax>()
                .First()
                .AttributeLists.SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToFieldAsync()
        {
            var syntax = @"
public class Class
{
    private List<int> field;
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .First( x => x.Declaration.Variables.First().Identifier.ToString() == "field" )
                .Declaration.Variables.First();

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<FieldDeclarationSyntax>()
                .First( x => x.Declaration.Variables.First().Identifier.ToString() == "field" )
                .AttributeLists.SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToVariableDeclaratorAsync()
        {
            var syntax = @"
public class Class
{
    private List<int> field = new List<int>();
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .First( x => x.Identifier.ToString() == "field" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<FieldDeclarationSyntax>()
                .First( x => x.Declaration.Variables.First().Identifier.ToString() == "field" )
                .AttributeLists.SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task Can_AddAttributeToParameterAsync()
        {
            var syntax = @"
public class Class
{
    public void Method(int param1, string param2) { }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ParameterSyntax>()
                .First( x => x.Identifier.ToString() == "param2" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var resultAttributes = newRoot.DescendantNodesAndSelf()
                .OfType<ParameterSyntax>()
                .First( x => x.Identifier.ToString() == "param2" )
                .AttributeLists.SelectMany( list => list.Attributes );

            this.LogAndAssertContains( resultAttributes, "TestAttribute" );
        }

        [Fact]
        public async Task When_AttributeAppliedToDeclarationWithComment_Then_AttributePreserveCommentAsync()
        {
            var syntax = @"
namespace Test
{
    public class Class1 { }

    
    /// <summary>
    /// This is summary that needs to be preserved.
    /// </summary>
    public class Class2 { }
}
";

            var expectedSyntax = @"
namespace Test
{
    public class Class1 { }


    /// <summary>
    /// This is summary that needs to be preserved.
    /// </summary>
    [TestAttribute]
    public class Class2 { }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First( x => x.Identifier.ToString() == "Class2" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            var newSyntax = newRoot.ToFullString();

            Assert.Equal( expectedSyntax, newSyntax );
        }

        [Fact]
        public async Task When_AttributeAppliedToIndentedDeclaration_Then_AttributePreserveIndentationAndExactlyOneNewlineAfterAttributeAsync()
        {
            var syntax = @"
namespace Test
{
    public class Class1 { }

    public class Class2 { }
}
";

            var expectedSyntax = @"
namespace Test
{
    public class Class1 { }

    [TestAttribute]
    public class Class2 { }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First( x => x.Identifier.ToString() == "Class2" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            Assert.Equal( expectedSyntax, newRoot.ToFullString() );
        }

        [Fact]
        public async Task When_AttributeAppliedToIndentedDeclarationWithAnAttribute_Then_AttributePreserveIndentationAndExactlyOneNewlineAfterAttributeAsync()
        {
            var syntax = @"
namespace Test
{
    public class Class1 { }

    [FirstAttribute]
    public class Class2 { }
}
";

            var expectedSyntax = @"
namespace Test
{
    public class Class1 { }

    [FirstAttribute]
    [TestAttribute]
    public class Class2 { }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First( x => x.Identifier.ToString() == "Class2" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            Assert.Equal( expectedSyntax, newRoot.ToFullString() );
        }

        [Fact]
        public async Task
            When_AttributeAppliedToIndentedDeclarationWithSeveralAttributes_Then_AttributePreserveIndentationAndExactlyOneNewlineAfterAttributeAsync()
        {
            var syntax = @"
namespace Test
{
    public class Class1 { }

    [FirstAttribute]
    [SecondAttribute]
    [ThirdAttribute]
    public class Class2 { }
}
";

            var expectedSyntax = @"
namespace Test
{
    public class Class1 { }

    [FirstAttribute]
    [SecondAttribute]
    [ThirdAttribute]
    [TestAttribute]
    public class Class2 { }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First( x => x.Identifier.ToString() == "Class2" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            Assert.Equal( expectedSyntax, newRoot.ToFullString() );
        }

        [Fact]
        public async Task When_AttributeAppliedToParameter_Then_NoIndentationAndNoNewlineAsync()
        {
            var syntax = @"
namespace Test
{
    public class Class1
    {
        public void Method(int param1, string param2) { }
    }
}
";

            var expectedSyntax = @"
namespace Test
{
    public class Class1
    {
        public void Method(int param1, [TestAttribute] string param2) { }
    }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ParameterSyntax>()
                .First( x => x.Identifier.ToString() == "param2" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            Assert.Equal( expectedSyntax, newRoot.ToFullString() );
        }

        [Fact( Skip = "TODO #29087" )]
        public Task When_AttributeAppliedToAssembly_Then_AttributeIsAddedOnNewLineAfterLeadingCommentAsync()
        {
            // string syntax = @"// leading comment";
            //
            //             string expectedSyntax = @"// leading comment
            // [assembly: TestAttribute]";
            //
            //             SyntaxNode originalRoot = await this.GetSyntaxRootAsync(syntax);
            //
            //             SyntaxNode newRoot = await CSharpAttributeHelper.AddAttributeAsync( originalRoot, originalRoot, new AttributeDescription( "TestAttribute" ), this.cancellationTokenSource.Token );
            //
            //             Assert.Equal(expectedSyntax, newRoot.ToFullString());

            return Task.CompletedTask;
        }

        [Fact( Skip = "TODO #29087" )]
        public Task When_AttributeAppliedToAssembly_Then_AttributeIsAddedOnNewLineAfterAlreadyExistingAttributesAsync()
        {
            // string syntax = @"// leading comment
            // // some comment
            // [assembly: TestAttribute]
            // // some other comment";
            //
            //             string expectedSyntax = @"// leading comment
            // // some comment
            // [assembly: TestAttribute]
            // // some other comment
            // [assembly: TestAttribute]";
            //
            //             SyntaxNode originalRoot = await this.GetSyntaxRootAsync(syntax);
            //
            //             SyntaxNode newRoot = await CSharpAttributeHelper.AddAttributeAsync( originalRoot, originalRoot, new AttributeDescription( "TestAttribute" ), this.cancellationTokenSource.Token );
            //
            //             Assert.Equal(expectedSyntax, newRoot.ToFullString());

            return Task.CompletedTask;
        }

        [Fact]
        public async Task When_AttributeAppliedToParameterWithAttribute_Then_NoIndentationAndNoNewlineAsync()
        {
            var syntax = @"
namespace Test
{
    public class Class1
    {
        public void Method(int param1, [FirstAttribute] string param2) { }
    }
}
";

            var expectedSyntax = @"
namespace Test
{
    public class Class1
    {
        public void Method(int param1, [FirstAttribute][TestAttribute] string param2) { }
    }
}
";

            SyntaxNode originalDeclaration = (await this.GetSyntaxRootAsync( syntax )).DescendantNodes()
                .OfType<ParameterSyntax>()
                .First( x => x.Identifier.ToString() == "param2" );

            var newRoot = await this.AddAttributeAsync( "TestAttribute", originalDeclaration );

            Assert.Equal( expectedSyntax, newRoot.ToFullString() );
        }

        private Task<SyntaxNode> AddAttributeAsync( string attributeName, SyntaxNode originalMethodDeclaration )
        {
            return this.AddAttributeAsync( originalMethodDeclaration, new AttributeDescription( attributeName ) );
        }

        private async Task<SyntaxNode> AddAttributeAsync( SyntaxNode syntaxNodeToBeDecorated, AttributeDescription attributeDescription )
        {
            var semanticModel = await this._testFileDocument!.GetSemanticModelAsync();
            var symbolToBeDecorated = semanticModel!.GetDeclaredSymbol( syntaxNodeToBeDecorated );

            var resultSolution = await CSharpAttributeHelper.AddAttributeAsync(
                this._testFileDocument,
                symbolToBeDecorated!,
                attributeDescription,
                this._cancellationTokenSource.Token );

            var resultDocument = resultSolution.GetDocument( this._testFileDocument.Id );
            var resultRoot = await resultDocument!.GetSyntaxRootAsync();

            return resultRoot!;
        }

        private async Task<SyntaxNode> GetSyntaxRootAsync( string syntax )
        {
            var project = ProjectInfo.Create( ProjectId.CreateNewId(), VersionStamp.Create(), "TestProject", "TestProject", LanguageNames.CSharp );
            this._workspace.AddProject( project );
            this._testFileDocument = this._workspace.AddDocument( project.Id, "TestFile.cs", SourceText.From( syntax ) );

            return (await this._testFileDocument.GetSyntaxRootAsync())!;
        }
    }
}