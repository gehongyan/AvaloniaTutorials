using System;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace AvaloniaPrism.ViewModels;

public class EventsViewModel : BindableBase, IDisposable
{
    private string _receivedMessage;
    private readonly SubscriptionToken _subscriptionToken;

    public EventsViewModel(IEventAggregator eventAggregator)
    {
        PublishEventCommand = new DelegateCommand(() =>
            eventAggregator.GetEvent<PubSubEvent<string>>()
                .Publish($"Hello from Avalonia! It's {DateTime.Now} now."));
        _subscriptionToken = eventAggregator.GetEvent<PubSubEvent<string>>()
            .Subscribe(message => ReceivedMessage = $"Received message: {message}", ThreadOption.UIThread);
    }

    public DelegateCommand PublishEventCommand { get; private set; }

    public string ReceivedMessage
    {
        get => _receivedMessage;
        private set => SetProperty(ref _receivedMessage, value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _subscriptionToken.Dispose();
    }
}
