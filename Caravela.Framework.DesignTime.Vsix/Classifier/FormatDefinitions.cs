using Microsoft.CodeAnalysis.Classification;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Caravela.Framework.DesignTime.Vsix.Classifier
{

    internal static class FormatDefinitions
    {
        public const string CompileTimeName = "Caravela/CompileTime";
        public const string SpecialName = "Caravela/Special";


        [Export( typeof( ClassificationTypeDefinition ) ), Name( CompileTimeName )]
        internal static ClassificationTypeDefinition? CompileTime;

        [Export( typeof( ClassificationTypeDefinition ) ), Name( SpecialName )]
        internal static ClassificationTypeDefinition? Special;

        [Export( typeof( EditorFormatDefinition ) ), Name( CompileTimeName ), UserVisible( true )]
        [ClassificationType( ClassificationTypeNames = CompileTimeName ), Order( Before = Priority.High )]
        private sealed class CompileTimeFormatDefinition : FormatDefinition
        {
            public CompileTimeFormatDefinition()
                : base( $"Compile-Time Code", background: Colors.LightSteelBlue, backgroundOpacity: 0.5 ) { }
        }

        [Export( typeof( EditorFormatDefinition ) ), Name( SpecialName ), UserVisible( true )]
        [ClassificationType( ClassificationTypeNames = SpecialName ), Order( Before = Priority.High )]
        private sealed class SpecialFormatDefinition : FormatDefinition
        {
            public SpecialFormatDefinition()
                : base( $"Magic Template Code", background: Colors.Yellow ) { }
        }
        
        private abstract class FormatDefinition : ClassificationFormatDefinition
        {

            protected FormatDefinition( string displayName,  Color? background = null, double? backgroundOpacity = null ) : base()
            {
                // Foreground color and font weight is overwritten and I didn't find an order/priority that would prevent that.
                // Specifically, color/font for static symbols is overwritten.

                this.ForegroundCustomizable = false;
                this.DisplayName = displayName;
                this.BackgroundColor = background;
                this.BackgroundOpacity = backgroundOpacity;

            }

        }
    }
}
