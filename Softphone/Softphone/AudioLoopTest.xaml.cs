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

namespace Softphone
{
    public partial class AudioLoopTest : PhoneApplicationPage
    {
        public AudioLoopTest()
        {
            InitializeComponent();
            ain.Start();
            ain.FrameReady += new AudioIn.SampleEventHandler(ain_FrameReady);
            log.ItemsSource = LogService.log;
        }

        void ain_FrameReady(object sender, GenericEventArgs<byte[]> e)
        {
            aout.playFragment(e.data);
        }

        AudioIn ain = new AudioIn(100);
        AudioOut aout = new AudioOut();
    }
}