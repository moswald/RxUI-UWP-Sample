namespace RxUI_UWP_Sample
{
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;

    // BootstrapApplication is a drop-in replacement of Application
    // - OnInitializeAsync is the first in the pipline, called if launching
    // - OnStartAsync is required, called second
    public abstract partial class BootstrapApplication
    {
        protected enum StartKind
        {
            Launch,
            Activate
        };
        
        // OnInitializeAsync is called when the app starts, but only if it is _not_ being restored.
        protected virtual Task OnInitializeAsync(IActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }

        // OnStartAsync is required, it is called when your app starts, after the app state has been restored.
        // App state will be restored when the app is suspended and then terminated (PreviousExecutionState == Terminated)
        protected abstract Task OnStartAsync(StartKind startKind, IActivatedEventArgs args);

        // Typically, this can be ignored, as BootstrapApplication takes care of backstack persistence.
        protected virtual Task OnSuspendingAsync(SuspendingEventArgs e)
        {
            return Task.CompletedTask;
        }

        // Typically, this can be ignored, as BootstrapApplication takes care of backstack persistence.
        // However, the app could be suspended for hours or days, meaning transient resources like network
        // connections will need to be restored.
        protected virtual Task OnResumingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
