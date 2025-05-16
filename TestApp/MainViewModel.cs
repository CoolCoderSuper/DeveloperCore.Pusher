using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using DeveloperCore.Pusher;

namespace TestApp;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private static readonly string _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeveloperCore", "Pusher");
    private static readonly string _file = Path.Combine(_dir, "config.json");
    private Sender? _sender;
    private IListener? _listener;

    public MainViewModel()
    {
        if (!Directory.Exists(_dir))
            Directory.CreateDirectory(_dir);
        var config = File.Exists(_file) ? JsonSerializer.Deserialize<Config>(File.ReadAllText(_file))! : new Config();
        Host = config.Host;
        Port = config.Port;
        Key = config.Key;
        ChannelName = config.ChannelName;
        Event = config.Event;
        Data = config.Data;
    }
    
    public ObservableCollection<Notification> Notifications { get; set; } = [];

    public string? Host
    {
        get;
        set => SetField(ref field, value);
    }
    
    public int Port
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Key
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ChannelName
    {
        get;
        set => SetField(ref field, value);
    }
    
    public string? Event
    {
        get;
        set => SetField(ref field, value);
    }
    
    public string? Data
    {
        get;
        set => SetField(ref field, value);
    }

    public string ConnectText => _listener is { Connected: true } ? "Disconnect" : "Connect";
    
    public bool Connected => _listener is { Connected: true };
    
    public async void Connect()
    {
        if (Connected && _listener != null)
        {
            await _listener.DisconnectAsync(CancellationToken.None);
            //OnConnectedStateChanged();
        }
        else
        {
            var builder = new UriBuilder("http", Host, Port);
            _sender = new Sender(builder.Uri, Key);
            //builder.Scheme = "ws";
            //_listener = new WebSocketListener(builder.Uri, _key, data => Notifications.Insert(0, data), _ => OnConnectedStateChanged(), e => throw e.Exception);
            builder.Scheme = "http";
            _listener = new SSEListener(builder.Uri, Key, data => Notifications.Insert(0, data), _ => OnConnectedStateChanged(), e => throw e.Exception);
            await _listener.ConnectAsync(CancellationToken.None);
            //OnConnectedStateChanged();
        }
    }

    private void OnConnectedStateChanged()
    {
        OnPropertyChanged(nameof(Connected));
        OnPropertyChanged(nameof(ConnectText));
    }

    public async void Send()
    {
        if (_sender == null) return;
        await _sender.SendAsync(ChannelName, Event, JsonSerializer.Deserialize<ExpandoObject>(Data ?? ""));
    }
    
    public void Clear()
    {
        Notifications.Clear();
    }

    public void Close()
    {
        var config = new Config
        {
            Host = Host,
            Port = Port,
            Key = Key,
            ChannelName = ChannelName,
            Event = Event,
            Data = Data
        };
        File.WriteAllText(_file, JsonSerializer.Serialize(config));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}
