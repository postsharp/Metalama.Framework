using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Caravela.AspectWorkbench.CodeEditor;
using Caravela.AspectWorkbench.ViewModels;
using Microsoft.Win32;
using PostSharp;
using RoslynPad.Editor;
using TextRange = System.Windows.Documents.TextRange;

namespace Caravela.AspectWorkbench.Views
{
    public partial class MainWindow
    {
        private const string _testsProjectPath = @"c:\src\Caravela\Tests\Caravela.Templating.UnitTests\";
        private const string _fileDialogueExt = ".cs";
        private const string _fileDialogueFilter = "C# Files (*.cs)|*.cs";

        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            this.InitializeRoslynEditors();

            var newViewModel = new MainViewModel();
            this._viewModel = newViewModel;
            this.DataContext = newViewModel;
            Post.Cast<MainViewModel, INotifyPropertyChanged>( newViewModel ).PropertyChanged += this.ViewModel_PropertyChanged;
        }

        private void InitializeRoslynEditors()
        {
            var roslynHost = CustomRoslynHost.Create();
            var highlightColors = new ClassificationHighlightColors();
            var workingDirectory = Directory.GetCurrentDirectory();

            this.sourceTextBox.Initialize( roslynHost, highlightColors, workingDirectory, "" );
        }

        private void ViewModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            // TODO RichTextBox doesn't support data binding out of the box. RoslynPad doesn't support binding to text either.
            switch ( e.PropertyName )
            {
                case nameof( MainViewModel.TestText ):
                    this.sourceTextBox.Text = this._viewModel.TestText;
                    break;
                case nameof( MainViewModel.ColoredTemplateDocument ):
                    this.highlightedSourceRichBox.Document = this._viewModel.ColoredTemplateDocument ?? new FlowDocument();
                    break;
                case nameof( MainViewModel.CompiledTemplateDocument ):
                    this.compiledTemplateRichBox.Document = this._viewModel.CompiledTemplateDocument ?? new FlowDocument();
                    break;
                case nameof( MainViewModel.TransformedTargetDocument ):
                    this.transformedCodeRichBox.Document = this._viewModel.TransformedTargetDocument ?? new FlowDocument();
                    break;
            }
        }

        private void UpdateViewModel()
        {
            this._viewModel.TestText = this.sourceTextBox.Text;

            // Alternatively set the UpdateSourceTrigger property of the TextBox binding to PropertyChanged.
            this.expectedOutputTextBox.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
        }

        private void NewButton_Click( object sender, RoutedEventArgs e )
        {
            this._viewModel.NewTest();
        }

        private async void OpenButton_Click( object sender, RoutedEventArgs e )
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = _fileDialogueExt;
            dlg.Filter = _fileDialogueFilter;
            dlg.InitialDirectory = _testsProjectPath;

            if ( dlg.ShowDialog() == true )
            {
                await this._viewModel.LoadTestAsync( dlg.FileName );
            }
        }

        private async void SaveButton_Click( object sender, RoutedEventArgs e )
        {
            if ( this._viewModel.IsNewTest )
            {
                this.SaveAsButton_Click( sender, e );
                return;
            }

            this.UpdateViewModel();
            await this._viewModel.SaveTestAsync( null );
        }

        private async void SaveAsButton_Click( object sender, RoutedEventArgs e )
        {
            this.UpdateViewModel();

            var dlg = new SaveFileDialog();
            dlg.DefaultExt = _fileDialogueExt;
            dlg.Filter = _fileDialogueFilter;
            dlg.InitialDirectory = _testsProjectPath;

            if ( dlg.ShowDialog() == false )
            {
                return;
            }

            await this._viewModel.SaveTestAsync( dlg.FileName );
        }

        private async void RunButton_Click( object sender, RoutedEventArgs e )
        {
            this.UpdateViewModel();
            await this._viewModel.RunTestAsync();
        }

        private void MakeExpectedButton_Click( object sender, RoutedEventArgs e )
        {
            if ( this._viewModel.TransformedTargetDocument == null )
            {
                this._viewModel.ExpectedOutputText = "";
            }

            this._viewModel.ExpectedOutputText = new TextRange( 
                this._viewModel.TransformedTargetDocument.ContentStart, 
                this._viewModel.TransformedTargetDocument.ContentEnd ).Text;
        }
    }
}
