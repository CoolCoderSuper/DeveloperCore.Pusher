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

public class MainViewModel : INotifyPropertyChanged
{
    private static readonly string _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeveloperCore", "Pusher");
    private static readonly string _file = Path.Combine(_dir, "config.json");
    private Sender? _sender;
    private Listener? _listener;
    private string _host;
    private string _key;
    private string _channelName;
    private string _event;
    private string _data;
    
    public MainViewModel()
    {
        if (!Directory.Exists(_dir))
            Directory.CreateDirectory(_dir);
        var config = File.Exists(_file) ? JsonSerializer.Deserialize<Config>(File.ReadAllText(_file)) : new Config();
        Host = config.Host;
        Key = config.Key;
        ChannelName = config.ChannelName;
        Event = config.Event;
        Data = config.Data;
    }
    
    public ObservableCollection<Notification> Notifications { get; set; } = new();

    public string Host
    {
        get => _host;
        set => SetField(ref _host, value);
    }

    public string Key
    {
        get => _key;
        set => SetField(ref _key, value);
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
            _sender = new Sender($"http://{Host}/", _key);
            _listener = new Listener($"ws://{Host}/", _key, (data) => Notifications.Insert(0, data));
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

    public void Close()
    {
        var config = new Config
        {
            Host = Host,
            Key = Key,
            ChannelName = ChannelName,
            Event = Event,
            Data = Data
        };
        File.WriteAllText(_file, JsonSerializer.Serialize(config));
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