using Avalonia.Controls;
using JuliaCrypt.ViewModels;
using System;
using System.IO;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using JuliaCrypt.DataFlowManagers;

namespace JuliaCrypt
{
    public partial class MainWindow : Window
    {
            //ImagePanel.IsVisible = !ImagePanel.IsVisible;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputFileClickHandler(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => 
            App.MWvmInstance.InputFile = FilePicker.ChooseInputFile();

        public void InvalidInputFileSelected() => FlyoutBase.ShowAttachedFlyout(InputFileButton);

        private void OutputFileClickHandler(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            App.MWvmInstance.OutputFile = FilePicker.ChooseOutputFile(title: "Select Save File.");
    }
}