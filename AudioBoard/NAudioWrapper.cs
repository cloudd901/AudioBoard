using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AudioBoard
{
    public delegate void PlaybackStoppedEventHandler();
    public delegate void TitleMsgChangedEventHandler();

    public class NAudioWrapper
    {
        public NAudioWrapper()
        {
        }

        public event PlaybackStoppedEventHandler PlaybackStopped;
        public event TitleMsgChangedEventHandler TitleMsgChanged;

        public TimeSpan CurrentTime
        {
            get { return (MainMP3OutputStream == null && MainWAVOutputStream == null) ? default : MainMP3OutputStream != null ? MainMP3OutputStream.CurrentTime : MainWAVOutputStream.CurrentTime; }
            set
            {
                if (MainMP3OutputStream != null)
                {
                    MainMP3OutputStream.CurrentTime = value;
                }
                else if (MainWAVOutputStream != null)
                {
                    MainWAVOutputStream.CurrentTime = value;
                }
            }
        }

        public int StreamBuffer = 5;

        public int DeviceNumber
        {
            get { return deviceNumber; }
            set
            {
                deviceNumber = value;
                if (WavePlayer != null)
                {
                    WavePlayer.DeviceNumber = deviceNumber;
                }
            }
        }

        public bool IsReady
        {
            get { return ((MainMP3OutputStream != null || MainWAVOutputStream != null) || MainMemoryStream != null) && MainVolumeChannel != null; }
        }

        public bool IsStreaming
        {
            get { return StreamThread?.IsAlive == true; }
        }

        public long Length
        {
            get { return MainMemoryStream == null ? (MainMP3OutputStream == null && MainWAVOutputStream == null) ? -1 : MainMP3OutputStream != null? MainMP3OutputStream.Length : MainWAVOutputStream.Length : MainMemoryStream.Length; }
        }

        public PlaybackState PlaybackState
        {
            get { return WavePlayer == null ? PlaybackState.Stopped : WavePlayer.PlaybackState; }
        }

        public long Position
        {
            get { return MainMemoryStream == null ? (MainMP3OutputStream == null && MainWAVOutputStream == null) ? -1 : MainMP3OutputStream != null ? MainMP3OutputStream.Position : MainWAVOutputStream.Position : MainMemoryStream.Position; }
            set
            {
                if (MainMemoryStream != null)
                {
                    long temp = value;
                    if (value > MainMemoryStream.Length)
                    {
                        temp = MainMemoryStream.Length;
                    }
                    else if (value < 0)
                    {
                        temp = 0;
                    }

                    MainMemoryStream.Position = temp;
                }
                else if (MainMP3OutputStream != null)
                {
                    long temp = value;
                    if (value > MainMP3OutputStream.Length)
                    {
                        temp = MainMP3OutputStream.Length;
                    }
                    else if (value < 0)
                    {
                        temp = 0;
                    }

                    MainMP3OutputStream.Position = temp;
                }
                else if (MainWAVOutputStream != null)
                {
                    long temp = value;
                    if (value > MainWAVOutputStream.Length)
                    {
                        temp = MainWAVOutputStream.Length;
                    }
                    else if (value < 0)
                    {
                        temp = 0;
                    }

                    MainWAVOutputStream.Position = temp;
                }
            }
        }

        public TimeSpan TotalTime
        {
            get { return MainMemoryStream == null ? (MainMP3OutputStream == null && MainWAVOutputStream == null) ? default : MainMP3OutputStream != null ? MainMP3OutputStream.TotalTime : MainWAVOutputStream.TotalTime : default; }
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (MainVolumeChannel != null)
                {
                    MainVolumeChannel.Volume = volume;
                }
            }
        }

        public string Title { get; set; }

        private bool StopStream { get; set; } = false;
        private int deviceNumber { get; set; } = -1;
        private Stream MainMemoryStream { get; set; } = null;
        private Mp3FileReader MainMP3OutputStream { get; set; } = null;
        private WaveFileReader MainWAVOutputStream { get; set; } = null;
        private WaveChannel32 MainVolumeChannel { get; set; } = null;
        private Thread StreamThread { get; set; } = null;

        private float volume { get; set; } = 0.75f;
        private WaveOut WavePlayer { get; set; } = null;

        public void ClearAudio()
        {
            WavePlayer?.Stop();
            Position = 0;

            WavePlayer?.Dispose();
            MainWAVOutputStream?.Dispose();
            MainMP3OutputStream?.Dispose();
            MainMemoryStream?.Dispose();
            MainVolumeChannel?.Dispose();

            WavePlayer = null;
            MainWAVOutputStream = null;
            MainMP3OutputStream = null;
            MainMemoryStream = null;
            MainVolumeChannel = null;
        }

        public void PauseToggle()
        {
            if (WavePlayer == null)
            {
                return;
            }

            if (WavePlayer.PlaybackState == PlaybackState.Playing)
            {
                WavePlayer.Pause();
            }
            else if (WavePlayer.PlaybackState == PlaybackState.Paused)
            {
                WavePlayer.Resume();
            }
            else if (WavePlayer.PlaybackState == PlaybackState.Stopped)
            {
                WavePlayer.Play();
            }
        }

        public string Play(string file, long loc = 0, bool start = true)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            Title = "Loading...";
            TitleMsgChanged?.Invoke();
            StopStream = false;
            string retString = null;
            string temp = Environment.CurrentDirectory + "\\Audio\\";
            ClearAudio();

            StringBuilder data = new StringBuilder();

            if (!file.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                string f = file;
                if (f.Contains("|"))
                {
                    string[] farr = f.Split('|');
                    f = farr[0];
                    data.Append(farr[1]);
                }

                if (!f.Contains(":"))
                {
                    f = temp + file;
                }

                if (File.Exists(f))
                {

                    if (f.EndsWith("wav", StringComparison.OrdinalIgnoreCase))
                    {
                        MainWAVOutputStream = new WaveFileReader(f);

                        MainWAVOutputStream.Position = loc;



                    }
                    else if (f.EndsWith("mp3", StringComparison.OrdinalIgnoreCase))
                    {
                        MainMP3OutputStream = new Mp3FileReader(f);
                        MainMP3OutputStream.Position = loc;

                    }

                    TagLib.File tagFile = TagLib.File.Create(f);
                    string artist = tagFile.Tag.FirstAlbumArtist;
                    string title = tagFile.Tag.Title;
                    if (artist != null && title != null)
                    {
                        data.Clear();
                        data.Append(artist).Append(" - ").Append(title);
                    }

                    if (data.Length == 0)
                    {
                        FileInfo fi = new FileInfo(f);
                        data.Append(fi.Name.Replace(fi.Extension, ""));
                    }
                }
                else
                {
                    retString = "File does not exist.\r\n" + f;
                }

            }
            else if (file.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                string url = file;
                if (url.Contains("|"))
                {
                    string[] urlarr = url.Split('|');
                    url = urlarr[0];
                    data.Append(urlarr[1]);
                }

                if (url.Contains("Youtube", StringComparison.CurrentCultureIgnoreCase))
                {
                    Title = "Downloading...";
                    TitleMsgChanged?.Invoke();
                    string newFilename = "";
                    string[] split = url.Split('=');
                    newFilename = split[^1];
                    if (newFilename?.Length == 0 && split.Length - 1 != 0)
                    {
                        newFilename = split[^2];
                    }

                    if (data.Length == 0)
                    {
                        data.Append(newFilename);
                    }

                    var mp3OutputFolder = temp + "music\\";
                    if (!Directory.Exists(mp3OutputFolder))
                    {
                        Directory.CreateDirectory(mp3OutputFolder);
                    }

                    string temp1 = mp3OutputFolder + newFilename + "x.mp3";
                    string temp2 = mp3OutputFolder + newFilename + ".mp3";

                    Thread t1 = new Thread(() =>
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://youtube.com/get_video_info?video_id=" + newFilename);
                        request.UseDefaultCredentials = true;
                        request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        string datainfo = "";
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using Stream receiveStream = response.GetResponseStream();
                            using StreamReader readStream = new StreamReader(receiveStream);
                            datainfo = readStream.ReadToEnd();
                            response.Close();
                            readStream.Close();
                        }

                        if (datainfo.Contains("&"))
                        {
                            foreach (string s in datainfo.Split('&'))
                            {
                                if (s.Contains("title="))
                                {
                                    data.Clear();
                                    data.Append(s.Replace("title=", "").Replace('+', ' ').Replace("%27", "'").Replace("%5B", "[").Replace("%5D", "]"));
                                    break;
                                }
                            }
                        }
                    });
                    t1.Start();

                    if (!Directory.Exists(mp3OutputFolder))
                    {
                        Directory.CreateDirectory(mp3OutputFolder);
                    }

                    if (!File.Exists(temp2))
                    {
                        Thread t2 = new Thread(() =>
                        {
                            string youtubedl = Path.Combine(temp + "binaries", "youtube-dl.exe");
                            string ffmpeg = Path.Combine(temp + "binaries\\ffmpeg.exe");

                            if (!File.Exists(temp1))
                            {
                                using Process Process1 = new Process();
                                Process1.StartInfo.UseShellExecute = false;
                                Process1.StartInfo.FileName = youtubedl;
                                Process1.StartInfo.Arguments = string.Format(@"--continue  --no-overwrites --restrict-filenames --extract-audio --fixup detect_or_warn --prefer-ffmpeg --ffmpeg-location ""{2}"" --audio-format mp3 --audio-quality 5 {0} -o ""{1}""", url, temp1, ffmpeg);
                                Process1.Start();
                                while (!Process1.HasExited)
                                {
                                    Thread.Sleep(100);
                                }
                            }

                            //Reencode since youtube-dl does not correctly encode MP3
                            using Process Process2 = new Process();
                            Process2.StartInfo.UseShellExecute = false;
                            Process2.StartInfo.FileName = ffmpeg;
                            Process2.StartInfo.Arguments = string.Format(@"-i ""{0}"" -codec:a libmp3lame -qscale:a 5 ""{1}""", temp1, temp2);
                            Process2.Start();
                            while (!Process2.HasExited)
                            {
                                Thread.Sleep(200);
                            }

                            File.Delete(temp1);
                        });
                        t2.Start();

                        while (t2.IsAlive)
                        {
                            Thread.Sleep(100);
                        }
                    }

                    while (t1.IsAlive)
                    {
                        Thread.Sleep(100);
                    }

                    MainMP3OutputStream = null;
                    int trycnt = 0;
                    while (MainMP3OutputStream == null)
                    {
                        try
                        {
                            MainMP3OutputStream = new Mp3FileReader(temp2);
                            MainMP3OutputStream.Position = loc;
                        }
                        catch (InvalidOperationException)
                        {
                            trycnt++;
                            if (trycnt >= 10)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Title = "Buffering...";
                    TitleMsgChanged?.Invoke();
                    string[] split = url.Split('/');
                    if (data.Length == 0)
                    {
                        data.Append(split[^1]);
                    }
                    else
                    {
                        data.Append(" - ").Append(split[^1]);
                    }

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.UserAgent = "Fiddler";
                    request.UseDefaultCredentials = true;
                    request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    HttpWebResponse response = null;

                    try
                    {
                        response = request.GetResponse() as HttpWebResponse;
                    }
                    catch (WebException ex)
                    {
                        response = ex.Response as HttpWebResponse;
                    }

                    if (response.StatusCode.ToString() == "NotFound")
                    {
                        ClearAudio();
                        Title = "File stream not found.";
                        TitleMsgChanged?.Invoke();
                        return Title;
                    }
                    else
                    {
                        StreamThread = new Thread(() =>
                        {
                            using var stream = response.GetResponseStream();
                            byte[] buffer = new byte[32000]; // 32KB chunks
                            int read;
                            MainMemoryStream = new MemoryStream();
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (MainMemoryStream == null)
                                {
                                    break;
                                }
                                else if (StopStream)
                                {
                                    if (MainMemoryStream.Length > 0)
                                    {
                                        MainMemoryStream.SetLength(0);
                                        MainMemoryStream.Position = 0;
                                    }

                                    Thread.Sleep(10);
                                    continue;
                                }

                                var pos = MainMemoryStream.Position;
                                MainMemoryStream.Position = MainMemoryStream.Length;
                                MainMemoryStream.Write(buffer, 0, read);
                                MainMemoryStream.Position = pos;
                            }
                        })
                        { IsBackground = true };
                        StreamThread.Start();

                        Thread t1 = new Thread(() =>
                        {
                            string tempdata = response.GetResponseHeader("icy-name");
                            if (tempdata != "")
                            {
                                data.Clear();
                                data.Append(tempdata);
                            }
                        })
                        { IsBackground = true };
                        t1.Start();

                        int buffTime = 0;
                        while (t1.IsAlive)
                        {
                            Thread.Sleep(100);
                            buffTime += 100;
                        }

                        while (buffTime < StreamBuffer * 1000)
                        {
                            Thread.Sleep(100);
                            buffTime += 100;
                        }

                        MainMemoryStream.Position = 0;
                        MainMP3OutputStream = null;
                        int tries = 0;
                        while (MainMP3OutputStream == null)
                        {
                            // Memorystream sometimes causes error if started too soon.
                            try
                            {
                                MainMP3OutputStream = new Mp3FileReader(MainMemoryStream);
                            }
                            catch (InvalidDataException)
                            {
                                tries++;
                            }

                            if (tries >= 20)
                            {
                                break;
                            }
                        }
                    }

                    request = null;
                    response = null;
                }
            }

            if (MainMP3OutputStream == null && MainWAVOutputStream == null)
            {
                Title = "Error Initializing Output\r\nUnable to load Stream.";
                TitleMsgChanged?.Invoke();
                return Title;
            }

            MainVolumeChannel = new WaveChannel32(MainMP3OutputStream != null ? MainMP3OutputStream : MainWAVOutputStream)
            {
                Volume = volume
            };

            try
            {
                WavePlayer = NewPlayer();
                WavePlayer.PlaybackStopped += WavePlayer_PlaybackStopped;
                WavePlayer.Init(MainVolumeChannel);
            }
            catch (Exception initException)
            {
                ClearAudio();
                Title = "Error Initializing Output\r\n" + initException.Message;
                return Title;
            }

            float tempvol = MainVolumeChannel.Volume;
            MainVolumeChannel.Volume = 1;
            MainVolumeChannel.Volume = 0;
            MainVolumeChannel.Volume = tempvol;

            retString = data.ToString();

            WavePlayer.Play();
            if (!start)
            {
                WavePlayer.Pause();
            }

            Title = retString;
            TitleMsgChanged?.Invoke();
            return Title;
        }

        public void Start()
        {
            StopStream = false;
            if (WavePlayer.PlaybackState == PlaybackState.Stopped && IsStreaming)
            {
                string temp = Title;
                Title = "Buffering...";
                TitleMsgChanged?.Invoke();
                Thread.Sleep(StreamBuffer * 1000);
                Title = temp;
                TitleMsgChanged?.Invoke();
            }

            WavePlayer?.Play();
        }

        public void Stop()
        {
            if (WavePlayer?.PlaybackState != PlaybackState.Stopped)
            {
                //ClearAudio();
                StopStream = true;
                WavePlayer?.Stop();
                Position = 0;
            }



            //Position = 0;
        }

        private WaveOut NewPlayer()
        {
            WaveOut player = new WaveOut(WaveCallbackInfo.FunctionCallback()) { DeviceNumber = DeviceNumber };
            player.PlaybackStopped += WavePlayer_PlaybackStopped;
            return player;
        }

        private void WavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackStopped?.Invoke();
        }
    }
}