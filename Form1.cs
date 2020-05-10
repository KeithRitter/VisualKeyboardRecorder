using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VKBFreqFormTest
{
    public partial class Form1 : Form
    {

        private IKeyboardMouseEvents k_Hook;
        private HttpClient _http;
        private readonly string HTTP_BASE_URI;

        public Form1()
        {
            InitializeComponent();
            _http = new HttpClient();
#if DEBUG
            HTTP_BASE_URI = @"https://localhost:44313/api/dev/"; // Set this to wherever the API is running at
#else
            HTTP_BASE_URI = @"https://localhost:5001/api/prod/"; // Set this to wherever the API is running at
#endif
            BeginDetection();
        }

        public async Task BeginDetection()
        {
            Subscribe();
            await DeletePastData();

            while (true)
            {
                var cTime = DateTime.Now;
                var newModel = new FeedbackMod()
                {
                    Count = 0,
                    Date = cTime
                };

                await _http.PostAsync(HTTP_BASE_URI + "add", new StringContent(JsonConvert.SerializeObject(newModel), Encoding.UTF8, "application/json"));
                Thread.Sleep(0); // Just to keep the core unlocked
                await Task.Delay(cTime.AddMinutes(1) - DateTime.Now);
            }
        }

        private async Task DeletePastData()
        {
            await _http.DeleteAsync(HTTP_BASE_URI);
        }

        private void Subscribe()
        {
            k_Hook = Hook.GlobalEvents();
            k_Hook.KeyPress += GlobalHookKeyPress;
        }

        private async void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {

            EventHitModel mod = new EventHitModel()
            {
                Hit = true
            };

            await _http.PostAsync(HTTP_BASE_URI + "update", new StringContent(JsonConvert.SerializeObject(mod), Encoding.UTF8, "application/json"));
        }
    }
}
