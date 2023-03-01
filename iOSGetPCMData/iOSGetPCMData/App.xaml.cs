using iOSGetPCMData.Services;
using Xamarin.Forms;

namespace iOSGetPCMData
{
    public partial class App : Application
    {
        public static IAudioEngine AudioEngine { get; private set; }

        public App(IAudioEngine audioEngine)
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();

            MainPage = new AppShell();

            AudioEngine = audioEngine;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
