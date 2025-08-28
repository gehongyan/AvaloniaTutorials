using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Sandbox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        _player.CommandManager.IsEnabled = false;
        _smtc = _player.SystemMediaTransportControls;
        _smtc.IsEnabled = true;
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsNextEnabled = true;
        _smtc.IsPreviousEnabled = true;
    }

    private readonly Windows.Media.Playback.MediaPlayer _player = new();
    private readonly Windows.Media.SystemMediaTransportControls _smtc;

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var updater = _smtc.DisplayUpdater;
        updater.AppMediaId = "AvaloniaMediaPlayer";
        updater.Type = Windows.Media.MediaPlaybackType.Music;
        updater.MusicProperties.Title = "Title from Avalonia";
        updater.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(
            new Uri("https://static.wikia.nocookie.net/projectsekai/images/1/13/Marasy_pfp.jpg/revision/latest?cb=20221019185149"));
        updater.Update();
        _smtc.PlaybackStatus = Windows.Media.MediaPlaybackStatus.Playing;
    }
}
