using Avalonia.Controls;
using Avalonia.Interactivity;
using DeveloperCore.Pusher;
using System;

namespace TestApp;

public partial class MainWindow : Window
{
    readonly Sender s = new(@"https://localhost:7166/", "test");
    readonly Receiver r = new(@"wss://localhost:7166/", "test");

    public MainWindow()
    {
        InitializeComponent();
        btnSend.Click += BtnSend_Click;
        btnConnect.Click += BtnConnect_Click;
        btnBind.Click += BtnBind_Click;
    }

    bool _bound = false;
    private void BtnBind_Click(object? sender, RoutedEventArgs e)
    {
        Action<Notification> action = (data) => txtData.Text += $"{data.Data}{Environment.NewLine}";
        if (_bound)
        {
            r.Unbind("Nice", action);
            _bound = false;
            btnBind.Content = "Bind";
        }else
        {
            r.Bind("Nice", action);            
            _bound = true;
            btnBind.Content = "Unbind";
        }
    }

    private async void BtnConnect_Click(object? sender, RoutedEventArgs e)
    {
        if (r.Connected)
        {
            await r.DisconnectAsync();
            btnConnect.Content = "Connect";
        }
        else
        {
            await r.ConnectAsync();
            btnConnect.Content = "Disconnect";
        }
    }

    private async void BtnSend_Click(object? sender, RoutedEventArgs e)
    {
        await s.Send("Nice", new { name = "hello", age = 9 });
    }
}