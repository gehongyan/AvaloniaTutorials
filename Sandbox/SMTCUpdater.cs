using System;
using Windows.Media;
using Windows.Storage.Streams;

namespace MediaTest;

public enum SmtcMediaStatus
{
    Playing,
    Paused,
    Stopped
}

public class SmtcUpdater
{
    private readonly SystemMediaTransportControlsDisplayUpdater _updater;

    public SmtcUpdater(SystemMediaTransportControlsDisplayUpdater updater, string appMediaId)
    {
        _updater = updater;
        _updater.AppMediaId = appMediaId;
        _updater.Type = MediaPlaybackType.Music;
    }

    public SmtcUpdater SetTitle(string title)
    {
        _updater.MusicProperties.Title = title;
        return this;
    }

    public SmtcUpdater SetAlbumTitle(string albumTitle)
    {
        _updater.MusicProperties.AlbumTitle = albumTitle;
        return this;
    }

    public SmtcUpdater SetArtist(string artist)
    {
        _updater.MusicProperties.Artist = artist;
        return this;
    }

    public SmtcUpdater SetThumbnail(string imgUrl)
    {
        _updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(imgUrl));
        return this;
    }

    public void Update() => _updater.Update();
}

public class SmtcCreator : IDisposable
{
    private readonly Windows.Media.Playback.MediaPlayer _player = new();
    private readonly SystemMediaTransportControls _smtc;
    private readonly SmtcUpdater _updater;

    public SmtcCreator(string mediaId)
    {
        //先禁用系统播放器的命令
        _player.CommandManager.IsEnabled = false;
        //直接创建SystemMediaTransportControls对象被平台限制，神奇的是MediaPlayer对象可以创建该NativeObject
        _smtc = _player.SystemMediaTransportControls;
        _updater = new SmtcUpdater(_smtc.DisplayUpdater, mediaId);

        //启用状态
        _smtc.IsEnabled = true;
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsNextEnabled = true;
        _smtc.IsPreviousEnabled = true;
        //响应系统播放器的命令
        _smtc.ButtonPressed += _smtc_ButtonPressed;
    }

    public void Dispose()
    {
        _smtc.IsEnabled = false;
        _player.Dispose();
    }

    public SmtcUpdater Info
    {
        get => _updater;
    }

    public event EventHandler PlayOrPause, Previous, Next;

    public void SetMediaStatus(SmtcMediaStatus status)
    {
        _smtc.PlaybackStatus = status switch
        {
            SmtcMediaStatus.Playing => MediaPlaybackStatus.Playing,
            SmtcMediaStatus.Paused => MediaPlaybackStatus.Paused,
            SmtcMediaStatus.Stopped => MediaPlaybackStatus.Stopped,
            _ => throw new NotImplementedException(),
        };
    }


    private void _smtc_ButtonPressed(SystemMediaTransportControls sender,
        SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        switch (args.Button)
        {
            case SystemMediaTransportControlsButton.Play:
            case SystemMediaTransportControlsButton.Pause:
                PlayOrPause?.Invoke(this, EventArgs.Empty);
                break;
            case SystemMediaTransportControlsButton.Next:
                Next?.Invoke(this, EventArgs.Empty);
                break;
            case SystemMediaTransportControlsButton.Previous:
                Previous?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}