// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

#pragma warning disable CS0649

namespace Caravela.Framework.DesignTime.Vsix.Classifier
{

    internal static class FormatDefinitions
    {
        public const string CompileTimeName = "Caravela/CompileTime";
        public const string DynamicName = "Caravela/Dynamic";
        public const string TemplateKeywordName = "Caravela/TemplateKeyword";
        public const string CompileTimeVariableName = "Caravela/CompileTimeVariable";
        private const double _backgroundOpacity = 0.3;
        private static readonly Color _background = Colors.LightSteelBlue;
        /*
#pragma warning disable SA1401 // Fields should be private

        [Export( typeof( ClassificationTypeDefinition ) )]
        [Name( CompileTimeName )]
        internal static ClassificationTypeDefinition? CompileTime;

        [Export( typeof( ClassificationTypeDefinition ) )]
        [Name( CompileTimeVariableName )]
        internal static ClassificationTypeDefinition? CompileTimeVariable;

        [Export( typeof( ClassificationTypeDefinition ) )]
        [Name( DynamicName )]
        internal static ClassificationTypeDefinition? Dynamic;

        [Export( typeof( ClassificationTypeDefinition ) )]
        [Name( TemplateKeywordName )]
        internal static ClassificationTypeDefinition? TemplateKeyword;

#pragma warning restore SA1401 // Fields should be private
        */

        [Export( typeof( EditorFormatDefinition ) )]
        [Name( CompileTimeName )]
        [UserVisible( true )]
        [ClassificationType( ClassificationTypeNames = CompileTimeName )]
        [Order( Before = Priority.High )]
        private sealed class CompileTimeFormatDefinition : FormatDefinition
        {
            public CompileTimeFormatDefinition()
                : base( $"Compile-Time Code", background: _background, backgroundOpacity: _backgroundOpacity )
            {
            }
        }

        [Export( typeof( EditorFormatDefinition ) )]
        [Name( CompileTimeVariableName )]
        [UserVisible( true )]
        [ClassificationType( ClassificationTypeNames = CompileTimeVariableName )]
        [Order( Before = Priority.High )]
        private sealed class CompileTimeVariableFormatDefinition : FormatDefinition
        {
            public CompileTimeVariableFormatDefinition()
                : base( $"Compile-Time Variable", background: _background, backgroundOpacity: _backgroundOpacity, isItalic: true )
            {
            }
        }

        [Export( typeof( EditorFormatDefinition ) )]
        [Name( DynamicName )]
        [UserVisible( true )]
        [ClassificationType( ClassificationTypeNames = DynamicName )]
        [Order( Before = Priority.High )]
        private sealed class DynamicFormatDefinition : FormatDefinition
        {
            public DynamicFormatDefinition()
                : base( $"Dynamic", background: _background, backgroundOpacity: _backgroundOpacity, decorations: System.Windows.TextDecorations.Underline )
            {
            }
        }

        [Export( typeof( EditorFormatDefinition ) )]
        [Name( TemplateKeywordName )]
        [UserVisible( true )]
        [ClassificationType( ClassificationTypeNames = TemplateKeywordName )]
        [Order( Before = DefaultOrderings.Highest, After = DefaultOrderings.Highest )]
        private sealed class TemplateKeywordFormatDefinition : FormatDefinition
        {
            public TemplateKeywordFormatDefinition()
                : base( $"Template keyword", background: _background, backgroundOpacity: _backgroundOpacity, foreground: Colors.Fuchsia, isBold: true )
            {
            }
        }

        private abstract class FormatDefinition : ClassificationFormatDefinition
        {

            protected FormatDefinition( string displayName,  Color? background = null, double? backgroundOpacity = null, Color? foreground = null, bool? isItalic = false, TextDecorationCollection? decorations = null, bool? isBold = null ) : base()
            {
                // Foreground color and font weight is overwritten for method names and I didn't find an order/priority that would prevent that.
                // Specifically, color/font for static symbols is overwritten.

                if ( foreground != null )
                {
                    this.ForegroundColor = foreground;
                }

                this.IsItalic = isItalic;
                this.IsBold = isBold;
                this.TextDecorations = decorations;

                this.DisplayName = displayName;
                this.BackgroundColor = background;
                this.BackgroundOpacity = backgroundOpacity;
            }
        }
    }
}
