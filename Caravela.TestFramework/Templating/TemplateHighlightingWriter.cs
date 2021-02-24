using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.DesignTime;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework.Templating
{
    public class TemplateHighlightingWriter
    {
        private readonly string _filePath;
        private readonly SourceText _sourceText;
        private readonly SyntaxNode _root;
        private readonly string _styleSheetPath;

        public TemplateHighlightingWriter( string filePath, SourceText sourceText, SyntaxNode root, [CallerFilePath] string callingSourceFilePath = null )
        {
            this._filePath = filePath;
            this._sourceText = sourceText;
            this._root = root;
#pragma warning disable CS8604 // Possible null reference argument.
            this._styleSheetPath = Path.Combine( Path.GetDirectoryName( callingSourceFilePath ), "highlighting.css" );
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public void Write()
        {
            //TODO: don't use the obsolete constructor
            var classifier = new TextSpanClassifier( this._sourceText, true );
            classifier.Visit( this._root );

            using ( var textWriter = new StreamWriter( this._filePath, false, Encoding.UTF8 ) )
            {
                textWriter.WriteLine( "<html>" );
                textWriter.WriteLine( "<head>" );
                //TODO: should be relative to repo root
                textWriter.WriteLine( $"<link rel='stylesheet' href='{this._styleSheetPath}' />" );
                textWriter.WriteLine( "</head>" );
                textWriter.WriteLine( "<body><pre>" );

                //TODO: preserve leading whitespaces
                foreach ( var classifiedSpan in classifier.ClassifiedTextSpans /*classifiedTextSpans!*/ )
                {
                    textWriter.Write( $"<span class='{classifiedSpan.Classification}'>" + this._sourceText.GetSubText( classifiedSpan.Span ) + "</span>" );
                }

                textWriter.WriteLine();
                textWriter.WriteLine();
                textWriter.WriteLine( "Legend:" );

                foreach ( var classification in Enum.GetValues( typeof( TextSpanClassification ) ) )
                {
                    textWriter.WriteLine( $"<span class='{classification}'>{classification}</span>" );
                }

                textWriter.WriteLine( "</pre></body>" );
                textWriter.WriteLine( "</html>" );
            }
        }
    }
}
