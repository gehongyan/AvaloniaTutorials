using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaReactive.Models;
using DynamicData;
using DynamicData.Binding;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaReactive.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Binding to IObservable

    public IObservable<long> SecondsFromStartup =>
        Observable.Interval(TimeSpan.FromSeconds(1))
            .StartWith(0);

    #endregion

    #region Binding via ObservableAsPropertyHelper

    private readonly ObservableAsPropertyHelper<long> _secondsFromHelper;

    public long SecondsFromHelper => _secondsFromHelper.Value;

    public MainWindowViewModel()
    {
        _secondsFromHelper =
            Observable.Interval(TimeSpan.FromSeconds(1))
                .StartWith(0)
                .ToProperty(this, x => x.SecondsFromHelper);
    }

    #endregion

    #region IObservable Basic

    [Reactive]
    public string? SearchQuery { get; set; }

    public record PackageSearchMetadataViewData(IPackageSearchMetadata Metadata)
    {
        public ICommand OpenPage => ReactiveCommand.Create(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Metadata.ProjectUrl.ToString()
            });
        });

        public Task<IImage> IconImage
        {
            get
            {
                return Task.Run(async () =>
                {
                    await Task.Yield();
                    HttpClient httpClient = new();
                    Stream dataStream = await httpClient.GetStreamAsync(Metadata.IconUrl);
                    MemoryStream memStream = new();
                    await dataStream.CopyToAsync(memStream);
                    memStream.Position = 0;
                    IImage newBitmap = new Bitmap(memStream);
                    return newBitmap;
                });
            }
        }
    }

    public IObservable<IEnumerable<PackageSearchMetadataViewData>> SearchResults => this
        .WhenAnyValue(x => x.SearchQuery)
        .Throttle(TimeSpan.FromSeconds(1))
        .Select(term => term?.Trim())
        .DistinctUntilChanged()
        .Where(term => !string.IsNullOrWhiteSpace(term))
        .SelectMany(async x => await SearchNuGetPackagesAsync(x, CancellationToken.None))
        .Select(x => x.Take(4).Select(y => new PackageSearchMetadataViewData(y)));

    private async Task<IEnumerable<IPackageSearchMetadata>> SearchNuGetPackagesAsync(
        string? term, CancellationToken token)
    {
        List<Lazy<INuGetResourceProvider>> providers = new();
        providers.AddRange(Repository.Provider.GetCoreV3());
        PackageSource package = new("https://api.nuget.org/v3/index.json");
        SourceRepository source = new(package, providers);

        SearchFilter filter = new(false);
        PackageSearchResource? resource = await source.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
        return await resource.SearchAsync(term, filter, 0, 10, new NuGet.Common.NullLogger(), token).ConfigureAwait(false);
    }

    #endregion

    #region WhenAnyValue

    [Reactive]
    public string? InputText { get; set; }

    public IObservable<int> InputTextLength => this
        .WhenAnyValue(x => x.InputText)
        .Select(x => x?.Length ?? 0)
        .StartWith(InputText?.Length ?? 0);

    #endregion

    #region Command canExecute

    [Reactive]
    public bool Agree { get; set; }

    [Reactive]
    public string? Username { get; set; }

    [Reactive]
    public string? Password { get; set; }

    public ICommand RegisterCommand => ReactiveCommand.Create(() => { },
        canExecute: this.WhenAnyValue(
                x => x.Agree,
                x => x.Username,
                x => x.Password)
            .Select(x =>
                x.Item1
                && !string.IsNullOrWhiteSpace(x.Item2)
                && x.Item3 is { Length: >= 6 and <= 32 }));

    #endregion

    #region WhenAnyPropertyChanged

    public IObservable<int> ChangedPropertyTriggeredCount => this.WhenAnyPropertyChanged(
            nameof(Agree),
            nameof(Username),
            nameof(Password))
        .Scan(0, (count, _) => count + 1)
        .StartWith(0);

    #endregion

    #region WhenAnyObservable

    public IObservable<string> ObservableValuesSummary => this.WhenAnyObservable(
        x => x.SecondsFromStartup,
        x => x.InputTextLength,
        x => x.ChangedPropertyTriggeredCount,
        (seconds, length, count) => $"Current value: {seconds}s, {length} chars, {count} times changes");

    #endregion

    #region ObservableForProperty

    public IObservable<string> ObservableForPropertyBefore => this.ObservableForProperty(x => x.Password, true)
        .Select(x => x.Value ?? string.Empty)
        .StartWith(Password ?? string.Empty);

    public IObservable<string> ObservableForPropertyAfter => this.ObservableForProperty(x => x.Password)
        .Select(x => x.Value ?? string.Empty)
        .StartWith(Password ?? string.Empty);

    #endregion

    #region ObservableCollection

    public ObservableCollection<Student> Students { get; } = new(Student.GetMany(5));

    public ICommand AppendStudentCommand => ReactiveCommand.Create(() => Students.Add(Student.GetOne(Students.Count)));

    public ICommand RemoveStudentCommand => ReactiveCommand.Create(() => Students.RemoveAt(Students.Count - 1));

    public IObservable<int> StudentsCount => this.WhenAnyValue(x => x.Students.Count);

    public IObservable<int> StudentsOlderThen20 => Students
        .ToObservableChangeSet()
        .Filter(x => x.Age > 20)
        .ToCollection()
        .Select(x => x.Count);

    #endregion
}
