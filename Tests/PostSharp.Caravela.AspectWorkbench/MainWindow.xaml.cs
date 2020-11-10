using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.AspectWorkbench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        

        public MainWindow()
        {
            this.InitializeComponent();

            this.textBox.Text = @"  
// Don't rename classes, methods, neither remove namespaces. Many things are hardcoded.  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Templating.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;
using static Caravela.Framework.Aspects.TemplateContext;

class Aspect
{
  [Template]
  dynamic Template()
  {
    var parameters = new object[target.Method.Parameters.Count];
    var stringBuilder = new StringBuilder();
    buildTime( stringBuilder );
    stringBuilder.Append(target.Method.Name);
    stringBuilder.Append('(');
    int i = 0;
    foreach ( var p in target.Parameters )
    {
       string comma = i > 0 ? "", "" : """";

        if ( p.IsOut )
        {
            stringBuilder.Append( $""{comma}{p.Name} = <out>"" );
        }
        else
        {
            stringBuilder.Append( $""{comma}{p.Name} = {{{i}}}"" );
            parameters[i] = p.Value;
        }

        i++;
    }
    stringBuilder.Append(')');

    Console.WriteLine( stringBuilder.ToString(), parameters );

    try
    {
        dynamic result = proceed();
        Console.WriteLine( stringBuilder + "" returned "" + result, parameters );
        return result;
    }
    catch ( Exception _e )
    {
        Console.WriteLine( stringBuilder + "" failed: "" + _e, parameters );
        throw;
    }
  }
}

class TargetCode
{
    int Method(int a, int b )
    {
        return a + b;
    }
}

";
        }

        private static readonly Dictionary<string, Color> classificationToColor = new Dictionary<string, Color>
        {
            {ClassificationTypeNames.Comment, Colors.Green},
            {ClassificationTypeNames.ExcludedCode, Colors.Black},
            {ClassificationTypeNames.Identifier, Colors.Black},
            {ClassificationTypeNames.Keyword, Colors.Red},
            {ClassificationTypeNames.ControlKeyword, Colors.Black},
            {ClassificationTypeNames.NumericLiteral, Colors.Black},
            {ClassificationTypeNames.Operator, Colors.Black},
            {ClassificationTypeNames.OperatorOverloaded, Colors.Black},
            {ClassificationTypeNames.PreprocessorKeyword, Colors.Black},
            {ClassificationTypeNames.StringLiteral, Colors.Blue},
            {ClassificationTypeNames.WhiteSpace, Colors.Black},
            {ClassificationTypeNames.Text, Colors.Black},
            {ClassificationTypeNames.StaticSymbol, Colors.Black},
            {ClassificationTypeNames.PreprocessorText, Colors.Gray},
            {ClassificationTypeNames.Punctuation, Colors.Black},
            {ClassificationTypeNames.VerbatimStringLiteral, Colors.Blue},
            {ClassificationTypeNames.StringEscapeCharacter, Colors.LightSeaGreen},
            {ClassificationTypeNames.ClassName, Colors.Purple},
            {ClassificationTypeNames.DelegateName, Colors.Purple},
            {ClassificationTypeNames.EnumName, Colors.Purple},
            {ClassificationTypeNames.InterfaceName, Colors.Purple},
            {ClassificationTypeNames.ModuleName, Colors.Black},
            {ClassificationTypeNames.StructName, Colors.Purple},
            {ClassificationTypeNames.TypeParameterName, Colors.HotPink},
            {ClassificationTypeNames.FieldName, Colors.Teal},
            {ClassificationTypeNames.EnumMemberName, Colors.Teal},
            {ClassificationTypeNames.ConstantName, Colors.Teal},
            {ClassificationTypeNames.LocalName, Colors.Brown},
            {ClassificationTypeNames.ParameterName, Colors.Brown},
            {ClassificationTypeNames.MethodName, Colors.Teal},
            {ClassificationTypeNames.ExtensionMethodName, Colors.Chartreuse},
            {ClassificationTypeNames.PropertyName, Colors.Teal},
            {ClassificationTypeNames.EventName, Colors.Teal},
            {ClassificationTypeNames.NamespaceName, Colors.Black},
            {ClassificationTypeNames.LabelName, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentAttributeName, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentAttributeQuotes, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentAttributeValue, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentCDataSection, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentComment, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentDelimiter, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentEntityReference, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentName, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentProcessingInstruction, Colors.Black},
            {ClassificationTypeNames.XmlDocCommentText, Colors.Black},
            {ClassificationTypeNames.XmlLiteralAttributeName, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralAttributeQuotes, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralAttributeValue, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralCDataSection, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralComment, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralDelimiter, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralEmbeddedExpression, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralEntityReference, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralName, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralProcessingInstruction, Colors.Gray},
            {ClassificationTypeNames.XmlLiteralText, Colors.Gray},
            {ClassificationTypeNames.RegexComment, Colors.Indigo},
            {ClassificationTypeNames.RegexCharacterClass, Colors.Indigo},
            {ClassificationTypeNames.RegexAnchor, Colors.Indigo},
            {ClassificationTypeNames.RegexQuantifier, Colors.Indigo},
            {ClassificationTypeNames.RegexGrouping, Colors.Indigo},
            {ClassificationTypeNames.RegexAlternation, Colors.Indigo},
            {ClassificationTypeNames.RegexText, Colors.Indigo},
            {ClassificationTypeNames.RegexSelfEscapedCharacter, Colors.Indigo},
            {ClassificationTypeNames.RegexOtherEscape, Colors.Indigo},
        };


        private async void OnClick(object sender, RoutedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            this.errorsTextBlock.Text = "";
            
            // Step 1: source
            var project = CreateProject();
            var document1 = project.AddDocument("name.cs", this.textBox.Text);
            

            var compilationForInitialDiagnostics = CSharpCompilation.Create("assemblyName", new[] {await document1.GetSyntaxTreeAsync()},
                project.MetadataReferences, (CSharpCompilationOptions?) project.CompilationOptions);
            var diagnostics = compilationForInitialDiagnostics.GetDiagnostics();
            this.ReportDiagnostics(diagnostics);

            if (diagnostics.Any(d=>d.Severity == DiagnosticSeverity.Error))
                return;

            var syntaxRoot1 = await document1.GetSyntaxRootAsync();
            var semanticModel1 = await  document1.GetSemanticModelAsync();

            

            var templateCompiler = new WorkbenchTemplateCompiler( semanticModel1);
            bool success = templateCompiler.TryCompile( syntaxRoot1,  out var annotatedSyntaxRoot, out var transformedSyntaxRoot );
            
            this.ReportDiagnostics( templateCompiler.Diagnostics );

            // If we have an annotated syntax tree, display it.
            if ( annotatedSyntaxRoot != null )
            {
                var document2 = document1.WithSyntaxRoot( annotatedSyntaxRoot );
                var text2 = await document2.GetTextAsync();

                var marker = new CompileTimeTextSpanMarker( text2 );
                marker.Visit( await document2.GetSyntaxRootAsync() );
                var metaSpans = marker.GetMarkedSpans();


                await WriteSyntaxColoring( text2, metaSpans, this.richBox1 );

            }

            if (!success )
            {
                return;
            }
           
            
            // Render the transformed tree.
            var project3 = CreateProject();
            var document3 = project3.AddDocument( "name.cs", transformedSyntaxRoot );
            var optionSet = (await document3.GetOptionsAsync()).WithChangedOption( FormattingOptions.IndentationSize, 4 );
            var syntaxNode3 = await document3.GetSyntaxRootAsync();

            var formattedTransformedSyntaxRoot = Formatter.Format( transformedSyntaxRoot, project3.Solution.Workspace, optionSet );
            var text4 = formattedTransformedSyntaxRoot.GetText( Encoding.UTF8 );
            var spanMarker = new CompileTimeTextSpanMarker( text4 );
            spanMarker.Visit( formattedTransformedSyntaxRoot );
            await WriteSyntaxColoring( text4, spanMarker.GetMarkedSpans(), this.richBox2 );


            // Write the template to disk for debugging.
            var outputFileName = Path.GetFullPath( "Template.cs" );
            using ( var outputFile = File.CreateText( outputFileName ) )
            {
                text4.Write( outputFile );
            }


            // Compile the template. This would eventually need to be done by Caravela itself and not this test program.

            var finalCompilation = CSharpCompilation.Create("assemblyName", new[] {formattedTransformedSyntaxRoot.SyntaxTree.WithChangedText(text4).WithFilePath(outputFileName)},
                project.MetadataReferences, (CSharpCompilationOptions?) project.CompilationOptions);
            
            var buildTimeAssemblyStream = new MemoryStream();
            var buildTimeDebugStream = new MemoryStream();
            var emitResult = finalCompilation.Emit(buildTimeAssemblyStream, buildTimeDebugStream);
            if (!emitResult.Success)
            {
                this.ReportDiagnostics(emitResult.Diagnostics);
                return;
            }

            var assembly = AppDomain.CurrentDomain.Load(buildTimeAssemblyStream.GetBuffer(), buildTimeDebugStream.GetBuffer());

            
            try
            {
                var aspectType = assembly.GetType("Aspect");
                var aspectInstance = Activator.CreateInstance(aspectType);
                var templateMethod =
                    aspectType.GetMethod("Template_Template", BindingFlags.Instance | BindingFlags.Public);
                
                var driver = new TemplateDriver( templateMethod );

                var caravelaCompilation = new SourceCompilation( compilationForInitialDiagnostics );
                var targetType = caravelaCompilation.GetTypeByReflectionName( "TargetCode" );
                var targetMethod = targetType.Methods.GetValue().SingleOrDefault( m => m.Name == "Method" );

                var output = driver.ExpandDeclaration( aspectInstance, targetMethod, caravelaCompilation );
                var formattedOutput = Formatter.Format( output, project3.Solution.Workspace);

                
                
                await WriteSyntaxColoring( formattedOutput.GetText(), null, this.richBox3);

            }
            catch ( Exception exception )
            {
                this.errorsTextBlock.Text += Environment.NewLine + exception.ToString();

            }

            this.errorsTextBlock.Text += Environment.NewLine + $"It took {stopwatch.Elapsed.TotalSeconds:f1} s.";

        }

        private void ReportDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
        {
            diagnostics = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            
            if (diagnostics.Count > 0)
            {
                if (this.errorsTextBlock.Text.Length > 0)
                {
                    this.errorsTextBlock.Text += Environment.NewLine;
                }

                this.errorsTextBlock.Text += string.Join(Environment.NewLine,
                    diagnostics.Select(d => d.Location + ":" + d.Id + " " + d.GetMessage()));
            }
        }

        private static Project CreateProject()
        {

            var netStandardDirectory =
                Environment.ExpandEnvironmentVariables(
                    @"%USERPROFILE%\.nuget\packages\netstandard.library\2.0.0\build\netstandard2.0\ref");
            
            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;
            var project = solution.AddProject(guid.ToString(), guid.ToString(), LanguageNames.CSharp)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddMetadataReferences(Directory.GetFiles(netStandardDirectory, "*.dll")
                    .Where(f=>!Path.GetFileNameWithoutExtension(f).EndsWith("Native", StringComparison.OrdinalIgnoreCase))
                    .Select(f => MetadataReference.CreateFromFile(f)))
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( CompileTimeAttribute ).Assembly.Location ) )
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(SyntaxNode).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(SyntaxFactory).Assembly.Location))
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TemplateHelper ).Assembly.Location ) )
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location))
                ;
            return project;
        }

        private static async Task WriteSyntaxColoring(SourceText text, ImmutableList<TextSpan> metaSpans, RichTextBox richTextBox)
        {
            
            var project = CreateProject();
            var document = project.AddDocument("name.cs", text.ToString());

            
            var classifiedSpans =
                await Classifier.GetClassifiedSpansAsync(document, TextSpan.FromBounds(0, text.Length));

            var ranges = classifiedSpans.Select(classifiedSpan =>
                new TextRange(classifiedSpan, text.GetSubText(classifiedSpan.TextSpan).ToString()));

            ranges = TextRange.FillGaps(text, ranges);

            var paragraph = new Paragraph();

            foreach (var range in ranges)
            {
                Color color;
               
               
                if (range.ClassificationType == null || !classificationToColor.TryGetValue(range.ClassificationType, out color))
                {
                    color = Colors.Black;
                }
                

                var isConstant = metaSpans != null && metaSpans.Any(s => s.Contains(range.TextSpan));

                var run = new Run(range.Text)
                {
                    Foreground = new SolidColorBrush(color),
                    Background = isConstant ? Brushes.Yellow : Brushes.White,
                    ToolTip = range.ClassificationType
                };
                ToolTipService.SetShowOnDisabled(run, true);
                paragraph.Inlines.Add(run);
            }


            richTextBox.Document = new FlowDocument( paragraph ) { PageWidth = 2000 };

            // This is how to enable wrapping on the document:
            // richTextBox.Document.SetBinding( FlowDocument.PageWidthProperty, 
            //    new Binding( "ActualWidth" ) { Source = richTextBox } );
            
        }
    }
    
    public class TextRange
    {
        public ClassifiedSpan ClassifiedSpan { get; }
        public string Text { get; }

        public TextRange(string classification, TextSpan span, SourceText text) :
            this(classification, span, text.GetSubText(span).ToString())
        {
        }

        public TextRange(string classification, TextSpan span, string text) :
            this(new ClassifiedSpan(classification, span), text)
        {
        }

        public TextRange(ClassifiedSpan classifiedSpan, string text)
        {
            this.ClassifiedSpan = classifiedSpan;
            this.Text = text;
        }

        public string ClassificationType => this.ClassifiedSpan.ClassificationType;

        public TextSpan TextSpan => this.ClassifiedSpan.TextSpan;

        public override string ToString() => this.ClassificationType ?? "null" + ":" + this.Text;
        
        public static IEnumerable<TextRange> FillGaps(SourceText text, IEnumerable<TextRange> ranges)
        {
            const string WhitespaceClassification = null;
            var current = 0;
            TextRange previous = null;

            foreach (var range in ranges)
            {
                var start = range.TextSpan.Start;
                if (start > current)
                {
                    yield return new TextRange(WhitespaceClassification, TextSpan.FromBounds(current, start), text);
                }

                if (previous == null || range.TextSpan != previous.TextSpan)
                {
                    yield return range;
                }

                previous = range;
                current = range.TextSpan.End;
            }

            if (current < text.Length)
            {
                yield return new TextRange(WhitespaceClassification, TextSpan.FromBounds(current, text.Length), text);
            }
        }

    }
}