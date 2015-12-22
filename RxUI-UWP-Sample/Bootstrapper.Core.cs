namespace RxUI_UWP_Sample
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using ReactiveUI;

    // no good place for this in RxUI yet...
    public interface ICanHandleUndo
    {
        bool HandleUndo();
    }

    public abstract partial class BootstrapApplication : Application, IScreen
    {
        const string DefaultTileId = "App";

        public new static BootstrapApplication Current { get; private set; }

        public RoutingState Router { get; private set; }

        public Frame RootFrame { get; private set; }
        public CommandBar CommandBar
        {
            get { return ((Window.Current.Content as Frame).Content as Page).BottomAppBar as CommandBar; }
            set { ((Window.Current.Content as Frame).Content as Page).BottomAppBar = value; }
        }

        readonly Subject<WindowCreatedEventArgs> _windowCreated = new Subject<WindowCreatedEventArgs>();
        public IObservable<WindowCreatedEventArgs> WindowCreated => _windowCreated.AsObservable();

        protected BootstrapApplication()
        {
            Current = this;

            Observable.FromEventPattern<object>(h => Resuming += h, h => Resuming -= h)
                .Subscribe(async _ => await OnResumingAsync());

            Observable.FromEventPattern<SuspendingEventHandler, SuspendingEventArgs>(h => Suspending += h, h => Suspending -= h)
                .Select(args => args.EventArgs)
                .Subscribe(async eventArgs =>
                {
                    var deferral = eventArgs.SuspendingOperation.GetDeferral();

                    try
                    {
                        await PersistRoutingStateAsync();
                        await OnSuspendingAsync(eventArgs);
                    }
                    finally
                    {
                        deferral.Complete();
                    }
                });
        }

        protected sealed override async void OnActivated(IActivatedEventArgs args) { await Activated(args); }
        protected sealed override async void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs args) { await Activated(args); } 
        protected sealed override async void OnFileActivated(FileActivatedEventArgs args) { await Activated(args); } 
        protected sealed override async void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs args) { await Activated(args); } 
        protected sealed override async void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs args) { await Activated(args); } 
        protected sealed override async void OnSearchActivated(SearchActivatedEventArgs args) { await Activated(args); } 
        protected sealed override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args) { await Activated(args); } 

        protected sealed override async void OnLaunched(LaunchActivatedEventArgs args) { await Launched(args); }

        async Task Activated(IActivatedEventArgs args)
        {
            if (Window.Current.Content == null)
            {
                InitializeFrame();
            }

            await OnStartAsync(BootstrapApplication.StartKind.Launch, args);

            Window.Current.Activate();
        }

        async Task Launched(IActivatedEventArgs args)
        {
            if (Window.Current.Content == null)
            {
                InitializeFrame();
            }

            var stateRestored = args.PreviousExecutionState == ApplicationExecutionState.Running ||
                                args.PreviousExecutionState == ApplicationExecutionState.Suspended;

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                // restore state if necessary/possible
                // only the primary tile should restore (this includes toast with no data payload)
                var e = args as ILaunchActivatedEventArgs;
                if (e?.TileId == DefaultTileId && string.IsNullOrWhiteSpace(e.Arguments))
                {
                    stateRestored = await RestoreRoutingStateAsync();
                }
            }

            if (!stateRestored)
            {
                Router = new RoutingState();
                await OnInitializeAsync(args);
            }

            Router.NavigationStack.BeforeItemsAdded
                .Where(_ => Router.NavigationStack.Count > 0)
                .Select(_ => SystemNavigationManager.GetForCurrentView())
                .Subscribe(navManager => navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible);

            Router.NavigationStack.ItemsRemoved
                .Where(_ => Router.NavigationStack.Count <= 1)
                .Select(_ => SystemNavigationManager.GetForCurrentView())
                .Subscribe(navManager => navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed);

            await OnStartAsync(BootstrapApplication.StartKind.Launch, args);

            Window.Current.Activate();
        }

        void InitializeFrame()
        {
            if (RootFrame == null)
            {
                RootFrame = new Frame
                {
                    Language = Windows.Globalization.ApplicationLanguages.Languages[0],
                    CacheSize = 1
                };
            }

            Window.Current.Content = RootFrame;

            var navManager = SystemNavigationManager.GetForCurrentView();
            Observable.FromEventPattern<BackRequestedEventArgs>(
                h => navManager.BackRequested += h,
                h => navManager.BackRequested -= h)
                .Select(x => x.EventArgs)
                .Subscribe(
                    args =>
                    {
                        if (Router.NavigationStack.Count > 1)
                        {
                            var canUndo = Router.NavigationStack.First() as ICanHandleUndo;
                            if (canUndo == null || !canUndo.HandleUndo())
                            {
                                var noAwait = Router.NavigateBack.ExecuteAsyncTask();
                            }

                            args.Handled = true;
                        }
                    });
        }

        protected sealed override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            _windowCreated.OnNext(args);
            base.OnWindowCreated(args);
        }

        Task PersistRoutingStateAsync()
        {
            // TODO: !!! this needs work - JsonSerializer can't serialize IRoutableViewModel interface by itself
            // likely, my IRoutableViewModels need to have the [DataContract] attribute applied
            // investigate later
            // for now, no suspended state will be saved

            //using (var ms = new MemoryStream())
            //using (var writer = new StreamWriter(ms, Encoding.UTF8))
            //using (var jsonWriter = new JsonTextWriter(writer))
            //{
            //    var serializer = new JsonSerializer();
            //    serializer.Serialize(jsonWriter, Router);

            //    await writer.FlushAsync();

            //    var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync(
            //        "routingState.json",
            //        CreationCollisionOption.ReplaceExisting);

            //    await FileIO.WriteBytesAsync(file, ms.ToArray());
            //}

            return Task.CompletedTask;
        }

        Task<bool> RestoreRoutingStateAsync()
        {
            // TODO: !!! this needs work - JsonSerializer can't serialize IRoutableViewModel interface by itself
            // likely, my IRoutableViewModels need to have the [DataContract] attribute applied
            // investigate later
            // for now, no suspended state will be loaded

            //try
            //{
            //    var file = await ApplicationData.Current.RoamingFolder.GetFileAsync("routingState.json");

            //    using (var rawStream = await file.OpenReadAsync())
            //    using (var stream = rawStream.AsStreamForRead())
            //    using (var reader = new StreamReader(stream))
            //    using (var jsonReader = new JsonTextReader(reader))
            //    {
            //        var deserializer = new JsonSerializer();
            //        Router = deserializer.Deserialize<RoutingState>(jsonReader);
            //    }

            //    return true;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}

            return Task.FromResult(false);
        }
    }
}
