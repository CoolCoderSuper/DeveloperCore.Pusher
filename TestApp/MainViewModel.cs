using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DeveloperCore.Pusher;

namespace TestApp;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly Sender _s = new("http://localhost:7166/", "test");
    private readonly Receiver _r = new("ws://localhost:7166/");
    private bool _bound;
    private string _data = "";
    private Channel _channel;

    public string Data
    {
        get => _data;
        set => SetField(ref _data, value);
    }

    public string ConnectText => _r.Connected ? "Disconnect" : "Connect";
    public string BindText => _bound ? "Unbind" : "Bind";
    
    public async Task Connect()
    {
        if (_r.Connected)
        {
            _r.Unsubscribe(_channel);
            await _r.DisconnectAsync();
            OnPropertyChanged(nameof(ConnectText));
        }
        else
        {
            await _r.ConnectAsync();
            _channel = _r.Subscribe("test");
            OnPropertyChanged(nameof(ConnectText));
        }
    }
    
    public void Bind()
    {
        Action<Notification> action = (data) => Data += $"{data.Data}{Environment.NewLine}";
        if (_bound)
        {
            _channel.Unbind("Nice", action);
            _bound = false;
            OnPropertyChanged(nameof(BindText));
        }else
        {
            _channel.Bind("Nice", action);            
            _bound = true;
            OnPropertyChanged(nameof(BindText));
        }
    }
    
    public async Task Send()
    {
        await _s.SendAsync("Nice", new { name = "hello", age = 9 });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}