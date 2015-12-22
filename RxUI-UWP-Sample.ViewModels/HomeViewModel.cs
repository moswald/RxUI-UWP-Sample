namespace RxUI_UWP_Sample.ViewModels
{
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using ReactiveUI;
    using Splat;

    public class HomeViewModel : ReactiveObject, IRoutableViewModel
    {
        public HomeViewModel(IScreen host = null)
        {
            HostScreen = host ?? Locator.Current.GetService<IScreen>();

            var canExecute = this.WhenAnyValue(vm => vm.UserData)
                .Select(data => !string.IsNullOrWhiteSpace(data) && data.Length < 5);

            SubmitCommand = ReactiveCommand.CreateAsyncTask(
                canExecute,
                _ =>
                {
                    var oldData = UserData;
                    UserData = string.Empty;

                    Prompt = $"You entered {oldData} last time.";

                    return Task.CompletedTask;
                });
        }

        public string Label => "Enter up to four characters";

        string _userData = string.Empty;
        public string UserData
        {
            get { return _userData; }
            set { this.RaiseAndSetIfChanged(ref _userData, value); }
        }

        string _prompt = "You have not entered anything yet.";
        public string Prompt
        {
            get { return _prompt; }
            set { this.RaiseAndSetIfChanged(ref _prompt, value); }
        }

        public ReactiveCommand<Unit> SubmitCommand { get; }

        public string UrlPathSegment => "Home";
        public IScreen HostScreen { get; }
    }
}
