using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using UnityPlayer;

namespace Channel9_Space_Station
{
    class App : IFrameworkView, IFrameworkViewSource
    {
        private WinRTBridge.WinRTBridge m_Bridge;
        private AppCallbacks m_AppCallbacks;

        public App()
        {
            SetupOrientation();
            m_AppCallbacks = new AppCallbacks();

            // Allow clients of this class to append their own callbacks.
            AddAppCallbacks(m_AppCallbacks);
            
        }

        //public async Task startRecording()
        //{
        //    var microphones =  await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);

        //    foreach (var microphone in microphones)
        //    {
                
        //    }


        //    var capture = new MediaCapture();

        //    await capture.InitializeAsync(new MediaCaptureInitializationSettings()
        //    {
        //        AudioDeviceId = microphones.FirstOrDefault().Id,
        //        AudioProcessing = new AudioPro
        //    });

        //    var file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("Myaudio.mp3");

        //    await capture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), file);
        //    await capture.StartRecordToCustomSinkAsync()
        //}

        public virtual void Initialize(CoreApplicationView applicationView)
        {
            applicationView.Activated += ApplicationView_Activated;
            CoreApplication.Suspending += CoreApplication_Suspending;

            // Setup scripting bridge
            m_Bridge = new WinRTBridge.WinRTBridge();
            m_AppCallbacks.SetBridge(m_Bridge);

            m_AppCallbacks.SetCoreApplicationViewEvents(applicationView);
        }

        /// <summary>
        /// This is where apps can hook up any additional setup they need to do before Unity intializes.
        /// </summary>
        /// <param name="appCallbacks"></param>
        virtual protected void AddAppCallbacks(AppCallbacks appCallbacks)
        {
        }

        private void CoreApplication_Suspending(object sender, SuspendingEventArgs e)
        {
        }

        private void ApplicationView_Activated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            CoreWindow.GetForCurrentThread().Activate();
        }

        public void SetWindow(CoreWindow coreWindow)
        {
            ApplicationView.GetForCurrentView().SuppressSystemOverlays = true;

            m_AppCallbacks.SetCoreWindowEvents(coreWindow);
            m_AppCallbacks.InitializeD3DWindow();
        }

        public void Load(string entryPoint)
        {
        }

        public void Run()
        {
            m_AppCallbacks.Run();
        }

        public void Uninitialize()
        {
        }

        [MTAThread]
        static void Main(string[] args)
        {
            var app = new App();
            CoreApplication.Run(app);
        }

        public IFrameworkView CreateView()
        {
            return this;
        }

        private void SetupOrientation()
        {
            Unity.UnityGenerated.SetupDisplay();
        }
    }
}
