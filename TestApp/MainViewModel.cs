﻿using System;
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
    private Listener? _listener;
    private string? _host;
    private int _port;
    private string? _key;
    private string? _channelName;
    private string? _event;
    private string? _data;
    
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
        get => _host;
        set => SetField(ref _host, value);
    }
    
    public int Port
    {
        get => _port;
        set => SetField(ref _port, value);
    }

    public string? Key
    {
        get => _key;
        set => SetField(ref _key, value);
    }

    public string? ChannelName
    {
        get => _channelName;
        set => SetField(ref _channelName, value);
    }
    
    public string? Event
    {
        get => _event;
        set => SetField(ref _event, value);
    }
    
    public string? Data
    {
        get => _data;
        set => SetField(ref _data, value);
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
            _sender = new Sender(builder.Uri, _key);
            builder.Scheme = "ws";
            _listener = new Listener(builder.Uri, _key, data => Notifications.Insert(0, data), _ => OnConnectedStateChanged(), e => throw e.Exception);
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
        await _sender.SendAsync(_channelName, _event, JsonSerializer.Deserialize<ExpandoObject>(_data ?? ""));
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

    public void Test()
    {
        
    }
}