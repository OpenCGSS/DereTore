using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls;
using DereTore.Applications.StarlightDirector.UI.Converters;
using Fluent;
using ComboBox = System.Windows.Controls.ComboBox;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
            InitializeCommandBindings();
        }

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(MainWindow),
            new PropertyMetadata(null, OnProjectChanged));

        private static void OnProjectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var window = obj as MainWindow;
            var newProject = e.NewValue as Project;
            var oldProject = e.OldValue as Project;
            Debug.Assert(window != null);
            window.Editor.Project = newProject;
            window.Editor.Score = newProject?.GetScore(newProject.Difficulty);

            if (oldProject != null) {
                oldProject.DifficultyChanged -= window.Project_DifficultyChanged;
                //BindingOperations.ClearBinding(window.DifficultySelector, ComboBox.SelectedIndexProperty);
            }
            if (newProject != null) {
                newProject.DifficultyChanged += window.Project_DifficultyChanged;
                //var binding = new Binding();
                //binding.Converter = new DifficultyToIndexConverter();
                //binding.Path = new PropertyPath($"{nameof(Project)}.{nameof(Entities.Project.Difficulty)}");
                //binding.Mode = BindingMode.TwoWay;
                //binding.Source = window;
                //BindingOperations.SetBinding(window.DifficultySelector, ComboBox.SelectedIndexProperty, binding);
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            Project = Project.Current;
        }

        private void Project_DifficultyChanged(object sender, EventArgs e) {
            var project = Project;
            Editor.Score = project?.GetScore(project.Difficulty);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            if (Project == null || (!Project.IsChanged && Project.IsSaved)) {
                return;
            }
            var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ExitPrompt), Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
            switch (result) {
                case MessageBoxResult.Yes:
                    if (CmdFileSaveProject.CanExecute(null)) {
                        CmdFileSaveProject.Execute(null);
                    }
                    if (!Project.IsSaved) {
                        e.Cancel = true;
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

    }
}
