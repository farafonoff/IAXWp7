using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Softphone
{
    public class LogService
    {
        private static ObservableCollection<string> _log = new ObservableCollection<string>();
        public static ObservableCollection<string> log
        {
            get { return _log; }
        }

        public static void Log(string str)
        {
            App.RunOnUI(() =>
            {
                log.Add(str);
                if (log.Count > 20)
                {
                    log.RemoveAt(0);
                }
            });
        }

    }
}
