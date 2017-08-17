﻿using HueHue.Helpers;
using HueHue.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;

namespace HueHue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isRunning = false;
        AppSettings settings;
        SerialStream stream;
        TrayIcon icon;

        public MainWindow()
        {
            InitializeComponent();

            settings = new AppSettings();
            stream = new SerialStream();

            GridMain.DataContext = settings;
            Effects.Setup(settings.TotalLeds);
            Effects.ColorOne = (LEDBulb)settings.ColorOne;

            if (settings.DarkMode)
            {
                PaletteHelper helper = new PaletteHelper();
                helper.SetLightDark(settings.DarkMode);
            }

            //The tray icon can control effects too
            icon = new TrayIcon(settings, stream, this);

            //The app was auto started by windows from the user's startup folder
            if (Environment.GetCommandLineArgs() != null)
            {
                if (settings.AutoStart && Environment.GetCommandLineArgs().Length > 1)
                {
                    this.Minimize();
                    StartStop();
                }
            }

            List<Device> devices = new List<Device> { new Device() { Name = "Arduino", Type = "Arduino", Icon = "/Icons/Devices/Arduino.png" } };

            ListDevices.ItemsSource = devices;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartStop();
        }

        public void StartStop()
        {
            if (!isRunning)
            {
                isRunning = true;
                buttonStart.Content = "Stop";
                try
                {
                    stream.Start();
                }
                catch (Exception)
                {
                }
            }
            else
            {
                isRunning = false;
                buttonStart.Content = "Start";
                try
                {
                    stream.Stop();
                }
                catch (Exception)
                {
                }
            }

            icon.UpdateTrayLabel();
        }

        private void comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    frame.Navigate(new FixedColors(settings));
                    break;
                case 1:
                    frame.Navigate(new FixedColors(settings));
                    break;
                case 2:
                    frame.Navigate(new MusicMode());
                    break;
                case 3:
                    frame.Navigate(new ColorCycle(settings));
                    break;
                case 4:
                    frame.Navigate(new SnakeMode(settings));
                    break;
                case 5:
                    //Effects.ShutOff(); Breath?
                    break;
                case 6:
                    frame.NavigationService.RemoveBackEntry();
                    frame.Content = "LED's currently shut off";
                    Effects.ShutOff();
                    break;
                default:
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settings.Minimize)
            {
                Minimize();
            }
            else
            {
                icon.Close();
                stream.Stop();
            }
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Minimize();
            }
        }

        private void Minimize()
        {
            Hide();
            icon.ShowStandardBalloon();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            stream.Stop(); //Stop the communication with the arduino, it might cause problems if some settings are changed while it's running

            SettingsWindow window = new SettingsWindow(settings);
            window.ShowDialog();
            buttonStart.Content = "Start";

            stream.Start();
        }
    }
}
