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
using System.Net.Sockets;

namespace Softphone
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            acc = new Account("0099", "1234");
            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            SocketAsyncEventArgs sea = new SocketAsyncEventArgs();
            sea.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.55"), 4569);
            sea.Completed += new EventHandler<SocketAsyncEventArgs>(sea_Completed);
            soc.ConnectAsync(sea);
            onLog(0, 0, "STARTED");
        }

        FrameSender fsender;
        Account acc;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (fsender == null) return;
            Registration regger = new Registration(fsender);
            regger.Register(acc);
        }

        void sea_Completed(object sender, SocketAsyncEventArgs e)
        {
            onLog(0, 0, "CONNECTED");
            fsender = new FrameSender((Socket)sender);
            //ai.Start();
        }

        public void onLog(int dc, int sc, string s)
        {
            Dispatcher.BeginInvoke(() => { logbox.Items.Add(s); });
        }

        AudioIn ai = new AudioIn(100);
        AudioOut ao = new AudioOut();

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (fsender == null) return;
            Call cl = new Call(fsender, ai, ao);
            cl.log += onLog;
            cl.Dial(cnumber.Text,acc);
        }

        private void loop_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AudioLoopTest.xaml", UriKind.Relative));
        }

    }
}