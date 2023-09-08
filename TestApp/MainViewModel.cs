using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using DeveloperCore.Pusher;

namespace TestApp;

public class MainViewModel : INotifyPropertyChanged
{
    private Sender? _sender;
    private Listener? _listener;
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

    public string ConnectText => _listener is { Connected: true } ? "Disconnect" : "Connect";
    
    public bool Connected => _listener is { Connected: true };
    
    public async void Connect()
    {
        if (Connected)
        {
            await _listener?.DisconnectAsync(CancellationToken.None);
            OnPropertyChanged(nameof(Connected));
            OnPropertyChanged(nameof(ConnectText));
        }
        else
        {
            _sender = new Sender($"http://{Host}/", "key");
            _listener = new Listener($"ws://{Host}/", "key", (data) => Notifications.Insert(0, data));
            await _listener.ConnectAsync(CancellationToken.None);
            OnPropertyChanged(nameof(Connected));
            OnPropertyChanged(nameof(ConnectText));
        }
    }

    public async void Send()
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