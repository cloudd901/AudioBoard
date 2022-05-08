using Microsoft.Win32;
using NAudio.Wave;
using PCAFFINITY;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioBoard
{
    public enum PlayerActions
    {
        VolumeUp,
        VolumeDown,
        Pause,
        Stop,
        Forward,
        Back,
        None
    }

    public static class AudioBoardPreferences
    {
        public static int CurrentFile { get; set; } = 0;
        public static float MainVolume { get; set; } = 7.5f;
        public static string[] PlayFiles { get; } = new string[9];
        public static double WindowColorSlide { get; set; } = 915;
        public static double WindowOpacitySlide { get; set; } = 0.95;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HotkeysExtensionWindow
    {
        private NAudioWrapper AudioPlayer = new NAudioWrapper();

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
            AudioPlayer.TitleMsgChanged += AudioPlayer_TitleMsgChanged;
            AudioPlayer.StreamBuffer = 4;
        }

        private void AudioPlayer_TitleMsgChanged()
        {
            if (TitleLabel?.IsVisible == true)
            {
                Dispatcher.Invoke(() => TitleLabel.Content = AudioPlayer.Title);
            }
        }

        private HotkeyCommand HK { get; set; }
        private bool MovingSlider { get; set; }

        private static void SaveSettings(string key, string data, string section)
        {
            IniFile ini = new IniFile();
            ini.Write(key, data, section);
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                long loc = 0;
                int i = cb.SelectedIndex - 1;
                bool isPlaying = AudioPlayer.PlaybackState == PlaybackState.Playing;

                if (AudioPlayer.DeviceNumber != i)
                {
                    if (AudioPlayer.IsReady && i != -1)
                    {
                        loc = AudioPlayer.Position;
                    }

                    AudioPlayer.DeviceNumber = i;
                    SaveSettings("DeviceNumber", AudioPlayer.DeviceNumber.ToString(), "Settings");

                    if (AudioPlayer.IsReady && i != -1)
                    {
                        string file = AudioBoardPreferences.PlayFiles[AudioBoardPreferences.CurrentFile];
                        Play(file, loc, isPlaying);
                    }
                }
            }
        }

        private void GetSettings()
        {
            IniFile ini = new IniFile();
            _ = int.TryParse(ini.Read("DeviceNumber", "Settings"), out int dn);
            AudioPlayer.DeviceNumber = dn;

            _ = double.TryParse(ini.Read("ColorSlide", "Settings"), out double cs);
            AudioBoardPreferences.WindowColorSlide = cs == -1 ? AudioBoardPreferences.WindowColorSlide : cs;

            _ = double.TryParse(ini.Read("OpacitySlide", "Settings"), out double os);
            AudioBoardPreferences.WindowOpacitySlide = os == -1 ? AudioBoardPreferences.WindowOpacitySlide : os;

            _ = float.TryParse(ini.Read("Volume", "Settings"), out float vol);
            AudioBoardPreferences.MainVolume = vol == -1 ? AudioBoardPreferences.MainVolume : vol;
            AudioPlayer.Volume = AudioBoardPreferences.MainVolume / 10;
        }

        private void GetAudios()
        {
            IniFile ini = new IniFile();
            for (int i = 1; i < 9; i++)
            {
                AudioBoardPreferences.PlayFiles[i - 1] = ini.Read(i.ToString(), "Audio");
            }
        }

        private void Hotkey_Action(uint vk)
        {
            string key = null;
            PlayerActions action = PlayerActions.None;

            switch (vk)
            {
                case (uint)Keys.VK_NUMPAD1:
                    key = "1";
                    break;

                case (uint)Keys.VK_NUMPAD2:
                    key = "2";
                    break;

                case (uint)Keys.VK_NUMPAD3:
                    key = "3";
                    break;

                case (uint)Keys.VK_NUMPAD4:
                    key = "4";
                    break;

                case (uint)Keys.VK_NUMPAD5:
                    key = "5";
                    break;

                case (uint)Keys.VK_NUMPAD6:
                    key = "6";
                    break;

                case (uint)Keys.VK_NUMPAD7:
                    key = "7";
                    break;

                case (uint)Keys.VK_NUMPAD8:
                    key = "8";
                    break;

                case (uint)Keys.VK_NUMPAD9:
                    key = "9";
                    break;

                case (uint)Keys.VK_NUMPAD0:
                    action = PlayerActions.Pause;
                    break;

                case (uint)Keys.VK_ADD:
                    action = PlayerActions.VolumeUp;
                    break;

                case (uint)Keys.VK_SUBTRACT:
                    action = PlayerActions.VolumeDown;
                    break;

                case (uint)Keys.VK_DECIMAL:
                    action = PlayerActions.Stop;
                    break;

                case (uint)Keys.VK_RIGHT:
                    action = PlayerActions.Forward;
                    break;

                case (uint)Keys.VK_LEFT:
                    action = PlayerActions.Back;
                    break;
            }

            if (key != null)
            {
                _ = int.TryParse(key, out int k);
                if (k > 0)
                {
                    TrackPlay(k);
                }
            }
            else if (action != PlayerActions.None)
            {
                PlayerAction(action);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            if (sender is MenuItem m)
            {
                string header = m.Header.ToString();
                if (header == "Play")
                {
                    Play(AudioBoardPreferences.PlayFiles[index]);
                }
                else if (header.Contains("File"))
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if (openFileDialog.ShowDialog() == true)
                    {
                        string line = openFileDialog.FileName + "|" + openFileDialog.SafeFileName;
                        AudioBoardPreferences.PlayFiles[index] = line;
                        (listBox1.SelectedItem as ListBoxItem).Content = $"Ctrl {index + 1} = {openFileDialog.SafeFileName}";
                        SaveSettings((index + 1).ToString(), line, "Audio");
                    }
                }
                else if (header.Contains("URL"))
                {
                    PopupURL popupURLDialog = new PopupURL();
                    if (popupURLDialog.ShowDialog() == true)
                    {
                        string path = popupURLDialog.AudioUrl.Trim();
                        string title = popupURLDialog.AudioTitle.Trim();
                        string line = path + (string.IsNullOrEmpty(title) ? "" : $"|{title}");
                        AudioBoardPreferences.PlayFiles[index] = line;
                        (listBox1.SelectedItem as ListBoxItem).Content = $"Ctrl {index + 1} = {(string.IsNullOrEmpty(title) ? path : title)}";
                        SaveSettings((index + 1).ToString(), line, "Audio");
                    }
                }
            }
        }

        private void MenuItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            if (sender is MenuItem m)
            {
                Play(AudioBoardPreferences.PlayFiles[index - 1]);
            }
        }

        private void OnKeyAction(Window form, short id, string key)
        {
            string[] keyArr = key.Split('}');
            uint k = HotkeyCommand.FilterKeytoUint(keyArr[^1]);
            Hotkey_Action(k);
        }

        private void Play(string file, long loc = 0, bool start = true)
        {
            Thread play = new Thread(() => AudioPlayer.Play(file.Split('|')[0], loc, start));
            play.Start();

            new Thread(() =>
            {
                while (play.IsAlive)
                {
                    Thread.Sleep(10);
                }

                while (AudioPlayer.IsReady)
                {
                    Thread.Sleep(100);
                    if (!MovingSlider && AudioPlayer.Length > 0 && trackBar2?.IsVisible == true)
                    {
                        Dispatcher.Invoke(() => trackBar2.Value = (double)(AudioPlayer.Position * 100 / AudioPlayer.Length));
                    }

                    TimeSpan t1 = TimeSpan.Zero;
                    TimeSpan t2 = TimeSpan.Zero;

                    if (AudioPlayer.Length > 0 && AudioPlayer.IsStreaming)
                    {
                        t1 = TimeSpan.FromMilliseconds(AudioPlayer.Position / 10);
                        t2 = TimeSpan.FromMilliseconds(AudioPlayer.Length / 10);
                    }
                    else if (AudioPlayer.Length > 0)
                    {
                        t1 = AudioPlayer.CurrentTime;
                        t2 = AudioPlayer.TotalTime;
                    }

                    if (trackLabel?.IsVisible == true)
                    {
                        Dispatcher.Invoke(() => trackLabel.Content = $"{t1.Hours:D2}h.{t1.Minutes:D2}m.{t1.Seconds:D2}s : {t2.Hours:D2}h.{t2.Minutes:D2}m.{t2.Seconds:D2}s");
                    }
                }

                if (trackLabel?.IsVisible == true)
                {
                    Dispatcher.Invoke(() => trackLabel.Content = "00h.00m.00s : 00h.00m.00s", System.Windows.Threading.DispatcherPriority.Background);
                }
            })
            { IsBackground = true }.Start();
        }

        private void PlayerAction(PlayerActions action)
        {
            if (action == PlayerActions.Pause)
            {
                PlayToggle();
            }
            else if (action == PlayerActions.Stop)
            {
                AudioPlayer.Stop();
            }
            else if (action == PlayerActions.Forward) // Skip 5 seconds forward
            {
                if (AudioPlayer.IsReady)
                {
                    if (AudioPlayer.IsStreaming)
                    {
                        AudioPlayer.Position = (long)(AudioPlayer.Position + (9000 * 5));
                    }
                    else
                    {
                        AudioPlayer.Position = (long)(AudioPlayer.Position + (180000 * 5));
                    }
                }
                else
                {
                    trackBar2.Value = 0;
                }
            }
            else if (action == PlayerActions.Back)
            {
                if (AudioPlayer.IsReady)
                {
                    if (AudioPlayer.IsStreaming)
                    {
                        AudioPlayer.Position = (long)(AudioPlayer.Position - (9000 * 5));
                    }
                    else
                    {
                        AudioPlayer.Position = (long)(AudioPlayer.Position - (180000 * 5));
                    }
                }
                else
                {
                    trackBar2.Value = 0;
                }
            }
            else
            {
                if (action == PlayerActions.VolumeDown)
                {
                    AudioBoardPreferences.MainVolume--;
                    if (AudioBoardPreferences.MainVolume < 0)
                    {
                        AudioBoardPreferences.MainVolume = 0;
                    }
                }
                else if (action == PlayerActions.VolumeUp)
                {
                    AudioBoardPreferences.MainVolume++;
                    if (AudioBoardPreferences.MainVolume > 10)
                    {
                        AudioBoardPreferences.MainVolume = 10;
                    }
                }

                trackBar1.Value = AudioBoardPreferences.MainVolume;
                SaveSettings("Volume", AudioBoardPreferences.MainVolume.ToString(), "Settings");
            }
        }

        private void PlayToggle()
        {
            if (AudioPlayer.IsReady)
            {
                if (AudioPlayer.PlaybackState == PlaybackState.Playing)
                {
                    AudioPlayer.PauseToggle();
                }
                else
                {
                    new Thread(() => AudioPlayer.Start()).Start();

                        //// Set timer to start allows UI to update.
                        //DispatcherTimer timer = new DispatcherTimer
                        //{
                        //    Interval = TimeSpan.FromSeconds(0.25)
                        //};

                        //timer.Tick += (o, e) =>
                        //{
                        //    timer.IsEnabled = false;
                        //    AudioPlayer.Start();
                        //};

                        //timer.IsEnabled = true;
                    }
            }
        }

        private void SetOpacity(double d)
        {
            mainWindow.Background.Opacity = d;
            comboBox1.Opacity = d;
            comboBorder.Opacity = d;
            listBorder.Opacity = d * 1.3;
            listBox1.Opacity = d * 1.3;

            foreach (Control c in mainGrid.Children.OfType<Control>())
            {
                c.Opacity = d;
            }
        }

        private void TrackPlay(int k)
        {
            new Thread(() => Play(AudioBoardPreferences.PlayFiles[k - 1], 0, true)).Start();

            //// Set timer to start allows UI to update.
            //DispatcherTimer timer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromSeconds(0.25)
            //};

            //timer.Tick += (o, e) =>
            //{
            //    timer.IsEnabled = false;
            //    Play(AudioBoardPreferences.PlayFiles[k - 1], 0, true);
            //};

            //timer.IsEnabled = true;
        }

        private void WavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (btnSave.IsEnabled)
            {
                btnSave.IsEnabled = false;
            }
        }

        //==============================================================================================

        #region WindowActions

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AudioPlayer.ClearAudio();
            HK.HotkeyUnregisterAll();
            HK.Dispose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GetSettings();
            GetAudios();

            for (int i = 1; i < 9; i++)
            {
                ListBoxItem li = new ListBoxItem()
                {
                    Content = $"Ctrl {i} = {AudioBoardPreferences.PlayFiles[i - 1].Split('|')[^1]}",
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Top,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    Height = 16,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0)
                };

                _ = listBox1.Items.Add(li);
            }

            sliderColor.Value = AudioBoardPreferences.WindowColorSlide;
            sliderOpacity.Value = AudioBoardPreferences.WindowOpacitySlide;
            trackBar1.Value = AudioBoardPreferences.MainVolume;

            for (int i = -1; i < WaveOut.DeviceCount; i++)
            {
                WaveOutCapabilities WOC = WaveOut.GetCapabilities(i);
                _ = comboBox1.Items.Add(WOC.ProductName);
            }

            if (AudioPlayer.DeviceNumber >= WaveOut.DeviceCount)
            {
                AudioPlayer.DeviceNumber = -1;
                SaveSettings("DeviceNumber", AudioPlayer.DeviceNumber.ToString(), "Settings");
            }

            comboBox1.SelectedIndex = AudioPlayer.DeviceNumber + 1;
            comboBox1.Text = WaveOut.GetCapabilities(AudioPlayer.DeviceNumber).ProductName;

            HK = new HotkeyCommand(this, new string[15] {
                "{CTRL}NUMPAD0",
                "{CTRL}NUMPAD1",
                "{CTRL}NUMPAD2",
                "{CTRL}NUMPAD3",
                "{CTRL}NUMPAD4",
                "{CTRL}NUMPAD5",
                "{CTRL}NUMPAD6",
                "{CTRL}NUMPAD7",
                "{CTRL}NUMPAD8",
                "{CTRL}NUMPAD9",
                "{CTRL}NUMPAD-",
                "{CTRL}NUMPAD+",
                "{CTRL}NUMPAD.",
                "{CTRL}{ALT}RIGHT",
                "{CTRL}{ALT}LEFT"
            });
            HK.KeyActionCall += OnKeyAction;
            HK.StartHotkeys();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        #endregion WindowActions

        //==============================================================================================

        #region WindowSliderActions

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte[] values = BitConverter.GetBytes((int)e.NewValue);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(values);
            }

            byte b = values[0];
            byte g = values[1];
            byte r = values[2];

            Brush sb = new SolidColorBrush(Color.FromRgb(r, g, b));
            sliderColor.Foreground = sb;
            sliderColor.Background = sb;
            this.Background = sb;
            SetOpacity(sliderOpacity.Value);
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetOpacity(e.NewValue);
        }

        private void SliderColor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SaveSettings("ColorSlide", sliderColor.Value.ToString(), "Settings");
        }

        private void SliderOpacity_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SaveSettings("OpacitySlide", sliderOpacity.Value.ToString(), "Settings");
        }

        private void trackBar1_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SaveSettings("Volume", trackBar1.Value.ToString(), "Settings");
        }

        private void trackBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AudioBoardPreferences.MainVolume = (float)trackBar1.Value;
            AudioPlayer.Volume = AudioBoardPreferences.MainVolume / 10;
        }

        private void TrackBar2_GotMouseCapture(object sender, MouseEventArgs e)
        {
            MovingSlider = true;
        }

        private void TrackBar2_LostMouseCapture(object sender, MouseEventArgs e)
        {
            double pos = trackBar2.Value;
            if (AudioPlayer.IsReady)
            {
                AudioPlayer.Position = (long)(pos * AudioPlayer.Length / 100);
            }
            else
            {
                trackBar2.Value = 0;
            }

            MovingSlider = false;
        }

        #endregion WindowSliderActions

        //==============================================================================================

        #region WindowButtonActions

        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnToggle_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnToggle_Copy1_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Click(object sender, RoutedEventArgs e) // Pause
        {
            PlayerAction(PlayerActions.Pause);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Stop
        {
            PlayerAction(PlayerActions.Stop);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // VolUp
        {
            PlayerAction(PlayerActions.VolumeUp);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) // VolDn
        {
            PlayerAction(PlayerActions.VolumeDown);
        }

        #endregion WindowButtonActions

        //==============================================================================================
    }
}