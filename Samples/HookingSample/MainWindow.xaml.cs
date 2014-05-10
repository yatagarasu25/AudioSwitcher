﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Hooking;
using DataFlow = AudioSwitcher.AudioApi.Hooking.ComObjects.DataFlow;
using Role = AudioSwitcher.AudioApi.Hooking.ComObjects.Role;

namespace HookingSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DefaultDeviceHook _hook;
        private Process _selectedProcess;
        private IAudioDevice _selectedAudioDevice;

        public ObservableCollection<Process> Processes
        {
            get;
            private set;
        }

        public ObservableCollection<IAudioDevice> AudioDevices
        {
            get;
            private set;
        }

        public Process SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
                _selectedProcess = value;
                OnPropertyChanged("SelectedProcess");
            }
        }

        public IAudioDevice SelectedAudioDevice
        {
            get { return _selectedAudioDevice; }
            set
            {
                _selectedAudioDevice = value;
                OnPropertyChanged("SelectedAudioDevice");
            }
        }

        public bool IsHookSet
        {
            get
            {
                return Hook != null;
            }
        }

        public CoreAudioContext AudioContext
        {
            get;
            private set;
        }

        public DefaultDeviceHook Hook
        {
            get { return _hook; }
            private set
            {
                _hook = value;
                OnPropertyChanged("Hook");
                OnPropertyChanged("IsHookSet");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Processes = new ObservableCollection<Process>();
            AudioDevices = new ObservableCollection<IAudioDevice>();

            AudioContext = new CoreAudioContext();

            this.DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Hook != null)
            {
                UnHook();
            }
        }

        private void RefreshProcesses(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Process[] processlist = Process.GetProcesses();
                Processes.Clear();
                foreach (var p in processlist.OrderBy(x => x.ProcessName))
                {
                    Processes.Add(p);
                }
            }));
        }

        private void HookProcess(object sender, RoutedEventArgs e)
        {
            if (Hook != null)
            {
                UnHook();
                return;
            }

            if (SelectedProcess == null || SelectedAudioDevice == null)
                return;

            var sId = SelectedAudioDevice.RealId;

            Hook = new DefaultDeviceHook(SelectedProcess.Id, (dataFlow, role) =>
            {
                return sId;
            });
            Hook.Hook();
            AudioContext.Controller.SetDefaultDevice(AudioContext.Controller.DefaultPlaybackDevice);
        }

        private void UnHook()
        {
            if (Hook != null)
            {
                Hook.Dispose();
                Hook = null;
                AudioContext.Controller.SetDefaultDevice(AudioContext.Controller.DefaultPlaybackDevice);
            }
        }

        private void RefreshAudioDevices(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var devices = AudioContext.Controller.GetPlaybackDevices(DeviceState.Active);
                AudioDevices.Clear();
                foreach (var d in devices.OrderBy(x => x.ShortName))
                {
                    AudioDevices.Add(d);
                }
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
