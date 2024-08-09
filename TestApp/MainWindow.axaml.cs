using Avalonia.Controls;
using Avalonia.Interactivity;
using DeveloperCore.Pusher;
using System;

namespace TestApp;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        var view = this.FindControl<MainView>("MainView");
        view?.Close();
    }
}