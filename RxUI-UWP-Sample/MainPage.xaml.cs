namespace RxUI_UWP_Sample
{
    using Windows.Foundation.Metadata;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;

    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["StatusBarBrush"]).Color;
                statusBar.BackgroundOpacity = 1;
            }

            // we're a "page" not a view, which means we're created by the app, directly
            // this means we don't fit within the ViewModel-first navigation style RxUI gives us
            RoutedViewHost.Router = BootstrapApplication.Current.Router;
        }
    }
}
