using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Softphone
{
    public class GenericEventArgs<T>:EventArgs
    {
        public T data { get; set; }
        public GenericEventArgs(T d) {
            data = d;
        }
    }
}
