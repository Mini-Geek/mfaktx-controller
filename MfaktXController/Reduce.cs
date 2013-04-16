using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfaktXController
{
    class Reduce
    {
        public Speed ReduceSpeed { get; private set; }
        public bool IsEnabled { get; private set; }
        public Reduce(IDictionary<string, Speed> values)
        {
            var allProcesses = Process.GetProcesses();
            foreach (var speed in from process in allProcesses
                                  join value in values.WithComparer(StringComparer.OrdinalIgnoreCase)
                                  on process.ProcessName equals value.Key
                                  select value.Value)
            {
                this.IsEnabled = true;
                this.ReduceSpeed = speed;
                if (speed == Speed.Stopped)
                    break;
            }
        }
    }
}
