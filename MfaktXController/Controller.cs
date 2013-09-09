using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MfaktXController
{
    enum MfaktXStatus
    {
        Stopped,
        Stopping,
        Running
    }
    enum Speed
    {
        Unknown,
        Stopped,
        Slow,
        Medium,
        Fast
    }

    [ImplementPropertyChanged]
    sealed class Controller : INotifyPropertyChanged
    {
        private static Controller _instance;
        public static Controller Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Controller();
                }
                return _instance;
            }
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        public event DataReceivedEventHandler DataReceived;
        public Reduce Reduced { get; set; }
        public Speed? ReducedFrom { get; set; }
        public MfaktXStatus Status { get; private set; }
        public Speed CurrentSpeed { get; private set; }
        public Speed? SwitchingToSpeed { get; private set; }
        public Task StopTask { get; private set; }
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case MfaktXStatus.Stopped:
                        return Status.ToString();
                    case MfaktXStatus.Running:
                    case MfaktXStatus.Stopping:
                        return string.Format("{0} ({1})", Status, CurrentSpeed);
                    default:
                        throw new InvalidEnumArgumentException("Status", (int)Status, typeof(MfaktXStatus));
                }
            }
        }
        Process process;

        private Controller()
        {
        }

        public async Task Start(Speed speed, bool manual)
        {
            if (speed == Speed.Stopped)
            {
                await Stop(manual);
                return;
            }
            if (speed != Speed.Fast && speed != Speed.Medium && speed != Speed.Slow)
                throw new ArgumentException("Speed must be Slow, Medium, Fast, or Stopped", "speed");

            if (manual)
            {
                this.Reduced = null;
                this.ReducedFrom = null;
            }

            this.SwitchingToSpeed = speed;

            if (this.Status == MfaktXStatus.Running)
            {
                await Stop(manual);
            }

            File.Copy(Utilities.IniFile(speed), Utilities.IniFile(null), true);

            ValidateStartNewInstance();
            if (process != null)
            {
                process.Exited -= process_Exited;
                process.Close();
                process.Dispose();
            }
            process = new Process();
            process.StartInfo = new ProcessStartInfo(Utilities.ExeFile, Utilities.MfaktXArguments)
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += process_ErrorDataReceived;
            process.OutputDataReceived += process_OutputDataReceived;
            process.Exited += process_Exited;

            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            this.Status = MfaktXStatus.Running;
            this.CurrentSpeed = speed;
            this.SwitchingToSpeed = null;
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            RaiseDataReceived(e);
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            RaiseDataReceived(e);
        }

        void RaiseDataReceived(DataReceivedEventArgs e)
        {
            var handler = this.DataReceived;
            if (handler != null)
                handler(this, e);
        }

        void ValidateStartNewInstance()
        {
            if (process != null && !process.HasExited)
                throw new InvalidOperationException("Process is still running");
            var exeFileName = Path.GetFileNameWithoutExtension(Utilities.ExeFile);
            var processes = Process.GetProcessesByName(exeFileName);
            if (processes.Length >= Utilities.MaxInstances)
                throw new InvalidOperationException(string.Format("{0} instance(s) of {1} are already running", processes.Length, exeFileName));
        }

        public async Task Stop(bool manual)
        {
            this.StopTask = _stop(manual);
            await this.StopTask;
            this.StopTask = null;
        }

        private async Task _stop(bool manual)
        {
            if (process == null)
                return;
            this.Status = MfaktXStatus.Stopping;
            if (manual)
            {
                this.Reduced = null;
                this.ReducedFrom = null;
            }
            var sendCtrlCode = new Process();
            sendCtrlCode.StartInfo = new ProcessStartInfo(Utilities.SendCtrlCode, process.Id + " 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            sendCtrlCode.Start();
            bool result = await Task.Run(() => process.WaitForExit(Utilities.Timeout));
            if (!result)
                process.Kill();
            await Task.Delay(20);
        }

        public void StopImmediately()
        {
            if (process != null)
                process.Kill();
        }

        void process_Exited(object sender, EventArgs e)
        {
            this.Status = MfaktXStatus.Stopped;
            this.CurrentSpeed = Speed.Stopped;
        }
    }
}
