using System;
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

namespace Caravela.AspectWorkbench.Views
{
    public partial class MainWindow
    {
        private const string TestsProjectPath = @"c:\src\Caravela\Tests\Caravela.Templating.UnitTests\";
        private const string FileDialogueExt = ".cs";
        private const string FileDialogueFilter = "C# Files (*.cs)|*.cs";

        private readonly MainViewModel viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            this.InitializeRoslynEditors();

            var newViewModel = new MainViewModel();
            this.viewModel = newViewModel;
            this.DataContext = newViewModel;
            Post.Cast<MainViewModel, INotifyPropertyChanged>( newViewModel ).PropertyChanged += this.ViewModel_PropertyChanged;
        }

        private void InitializeRoslynEditors()
        {
            var roslynHost = CustomRoslynHost.Create();
            var highlightColors = new ClassificationHighlightColors();
            string workingDirectory = Directory.GetCurrentDirectory();

            this.sourceTextBox.Initialize( roslynHost, highlightColors, workingDirectory, "" );
            this.targetSourceTextBox.Initialize( roslynHost, highlightColors, workingDirectory, "" );
        }

        private void ViewModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            // TODO RichTextBox doesn't support data binding out of the box. RoslynPad doesn't support binding to text either.
            switch ( e.PropertyName )
            {
                case nameof( MainViewModel.TemplateText ):
                    this.sourceTextBox.Text = this.viewModel.TemplateText;
                    break;
                case nameof( MainViewModel.TargetText ):
                    this.targetSourceTextBox.Text = this.viewModel.TargetText;
                    break;
                case nameof( MainViewModel.ColoredTemplateDocument ):
                    this.highlightedSourceRichBox.Document = this.viewModel.ColoredTemplateDocument ?? new FlowDocument();
                    break;
                case nameof( MainViewModel.CompiledTemplateDocument ):
                    this.compiledTemplateRichBox.Document = this.viewModel.CompiledTemplateDocument ?? new FlowDocument();
                    break;
                case nameof( MainViewModel.TransformedTargetDocument ):
                    this.transformedCodeRichBox.Document = this.viewModel.TransformedTargetDocument ?? new FlowDocument();
                    break;
            }
        }

        private void UpdateViewModel()
        {
            this.viewModel.TemplateText = this.sourceTextBox.Text;
            this.viewModel.TargetText = this.targetSourceTextBox.Text;
            // Alternatively set the UpdateSourceTrigger property of the TextBox binding to PropertyChanged.
            this.expectedOutputTextBox.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
        }

        private void NewButton_Click( object sender, RoutedEventArgs e )
        {
            this.viewModel.NewTest();
        }

        private async void OpenButton_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = FileDialogueExt;
            dlg.Filter = FileDialogueFilter;
            dlg.InitialDirectory = TestsProjectPath;

            if ( dlg.ShowDialog() == true )
            {
                await this.viewModel.LoadTestAsync( dlg.FileName );
            }
        }

        private async void SaveButton_Click( object sender, RoutedEventArgs e )
        {
            if ( this.viewModel.IsNewTest )
            {
                this.SaveAsButton_Click( sender, e );
                return;
            }

            this.UpdateViewModel();
            await this.viewModel.SaveTestAsync( null );
        }

        private async void SaveAsButton_Click( object sender, RoutedEventArgs e )
        {
            this.UpdateViewModel();

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = FileDialogueExt;
            dlg.Filter = FileDialogueFilter;
            dlg.InitialDirectory = TestsProjectPath;

            if ( dlg.ShowDialog() == false )
            {
                return;
            }
            await this.viewModel.SaveTestAsync( dlg.FileName );
        }

        private async void RunButton_Click( object sender, RoutedEventArgs e )
        {
            this.UpdateViewModel();
            await this.viewModel.RunTestAsync();
        }
    }
}
