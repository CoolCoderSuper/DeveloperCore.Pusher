using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using DeveloperCore.Pusher;

namespace TestApp;

public class MainViewModel : INotifyPropertyChanged
{
    private Sender? _sender;
    private Receiver? _receiver;
    private Channel? _channel;
    private string _host = "localhost:7166";
    private string _channelName = "test";
    private string _event = "Nice";
    private string _data = """
                           {
                            "name":"hello",
                            "age":9
                           }
                           """;
    
    public ObservableCollection<Notification> Notifications { get; set; } = new();

    public string Host
    {
        get => _host;
        set => SetField(ref _host, value);
    }

    public string ChannelName
    {
        get => _channelName;
        set => SetField(ref _channelName, value);
    }
    
    public string Event
    {
        get => _event;
        set => SetField(ref _event, value);
    }
    
    public string Data
    {
        get => _data;
        set => SetField(ref _data, value);
    }

    public string ConnectText => _receiver is { Connected: true } ? "Disconnect" : "Connect";
    
    public bool Connected => _receiver is { Connected: true };
    
    public async Task Connect()
    {
        Action<Notification> action = (data) => Notifications.Insert(0, data);
        if (Connected)
        {
            _channel?.Unbind("Nice", action);
            _receiver?.Unsubscribe(_channel);
            await _receiver?.DisconnectAsync();
            OnPropertyChanged(nameof(Connected));
            OnPropertyChanged(nameof(ConnectText));
        }
        else
        {
            _sender = new Sender($"http://{Host}/");
            _receiver = new Receiver($"ws://{Host}/");
            await _receiver.ConnectAsync();
            _channel = _receiver.Subscribe("test");
            _channel.Bind("Nice", action);            
            OnPropertyChanged(nameof(Connected));
            OnPropertyChanged(nameof(ConnectText));
        }
    }

    public async Task Send()
    {
        await _sender.SendAsync(_channelName, _event, JsonSerializer.Deserialize<ExpandoObject>(_data));
    }
    
    public void Clear()
    {
        Notifications.Clear();
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