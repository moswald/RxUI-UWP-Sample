namespace RxUI_UWP_Sample.Views
{
    using System.Reactive.Linq;
    using ReactiveUI;
    using ViewModels;
    using Windows.UI.Xaml;

    public sealed partial class HomeView : IViewFor<HomeViewModel>
    {
        public HomeView()
        {
            this.InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.Label, v => v.Label.Text);
            this.Bind(ViewModel, vm => vm.UserData, v => v.DataEntry.Text);

            // just excersizing BindTo - normally this would also be done wth OneWayBind
            this.WhenAnyValue(v => v.ViewModel.Prompt)
                .ObserveOnDispatcher()
                .BindTo(this, v => v.DataEntry.PlaceholderText);

            this.BindCommand(ViewModel, vm => vm.SubmitCommand, v => v.Button);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(HomeViewModel),
            typeof(HomeView),
            new PropertyMetadata(default(HomeViewModel)));

        public HomeViewModel ViewModel
        {
            get { return (HomeViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (HomeViewModel)value; }
        }
    }
}
