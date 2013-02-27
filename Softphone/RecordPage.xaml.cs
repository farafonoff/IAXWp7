using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Softphone
{
    public partial class RecordPage : PhoneApplicationPage
    {
        // Конструктор
        public RecordPage()
        {
            InitializeComponent();
            mic.BufferReady += Default_BufferReady;
            SoundEffect.MasterVolume = 1.0f;
        }
        MemoryStream ms;
        Microphone mic = Microphone.Default;

        // When the buffer's ready we need to empty it
        // We'll copy to a MemoryStream
        // We could push into IsolatedStorage etc
        void Default_BufferReady(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while ((bytesRead = mic.GetData(buffer, 0, buffer.Length)) > 0)
                ms.Write(buffer, 0, bytesRead);
        }

        // The user wants to start recording. If we've already made 
        // a recording, close that MemoryStream and create a new one.
        // Start recording on the default device.
        private void start_Click(object sender, RoutedEventArgs e)
        {
            if (ms != null)
                ms.Close();

            ms = new MemoryStream();

            mic.Start();
        }

        // The user wants to stop recording. Checks the microphone
        // is stopped. Reset the MemoryStream position.
        // Play back the recording. Pitch is based on slider value
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (mic.State != MicrophoneState.Stopped)
                mic.Stop();

            ms.Position = 0;

            SoundEffect se = 
                new SoundEffect(
                    ms.ToArray(), mic.SampleRate, AudioChannels.Mono);
            se.Play(1.0f, /*(float)slider1.Value*/0.0f, 0.0f);
        }
    }
}