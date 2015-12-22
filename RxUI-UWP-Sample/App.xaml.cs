namespace RxUI_UWP_Sample
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using ReactiveUI;
    using Splat;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Xaml;
    using ViewModels;

    static class DependencyResolverExtensions
    {
        public static void RegisterViewsForViewModels(this IMutableDependencyResolver resolver, Assembly assembly)
        {
            // for each type that implements IViewFor
            foreach (var ti in assembly.DefinedTypes
                .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IViewFor)))
                .Where(ti => !ti.IsAbstract))
            {
                var ivf = ti.ImplementedInterfaces.SingleOrDefault(t => t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IViewFor)));

                // need to check for null because some classes may implement IViewFor but not IViewFor<T> - we don't care about those
                if (ivf != null)
                {
                    var contract = ti.GetCustomAttribute<ViewContractAttribute>()?.Contract;
                    RegisterType(resolver, ti, ivf, contract);
                }
            }
        }

        public static void Register<TService>(this IMutableDependencyResolver resolver, Func<object> factory, string contract = null)
        {
            resolver.Register(factory, typeof(TService), contract);
        }

        public static void RegisterConstant<TService>(this IMutableDependencyResolver resolver, TService value, string contract = null)
        {
            resolver.RegisterConstant(value, typeof(TService), contract);
        }

        static void RegisterType(IMutableDependencyResolver resolver, TypeInfo ti, Type serviceType, string contract)
        {
            var factory = TypeFactory(ti.AsType());
            if (ti.GetCustomAttribute<LocatorSingleInstanceAttribute>() != null)
            {
                // *call* factory
                resolver.RegisterConstant(factory(), serviceType, contract);
            }
            else
            {
                // pass in factory
                resolver.Register(factory, serviceType, contract);
            }
        }

        static Func<object> TypeFactory(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type.GetConstructor(Type.EmptyTypes))).Compile();
        }
    }

    sealed partial class App
    {
        public App()
        {
            this.InitializeComponent();

            var locator = Locator.CurrentMutable;

            locator.RegisterConstant<ILogger>(new LogImpl());
            locator.RegisterConstant<IScreen>(this);

            locator.RegisterViewsForViewModels(typeof(App).GetTypeInfo().Assembly);
        }

        protected override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
                //DebugSettings.IsTextPerformanceVisualizationEnabled = true;
                //DebugSettings.IsOverdrawHeatMapEnabled = true;

                DebugSettings.Events().BindingFailed.Subscribe(
                    failedBinding =>
                    {
                        var t = this;
                        Debugger.Break();
                    });
            }

            await Router.Navigate.ExecuteAsyncTask(new HomeViewModel());
            RootFrame.Navigate(typeof(MainPage));
        }

        class LogImpl : ILogger
        {
            public void Write(string message, LogLevel logLevel)
            {
                if ((int)logLevel < (int)Level)
                {
                    return;
                }

                Debug.WriteLine(message);
            }

            public LogLevel Level { get; set; }
        }
    }
}
