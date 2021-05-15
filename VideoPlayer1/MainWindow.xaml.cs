using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace VideoPlayer1
{
    public partial class MainWindow : Window
    {
        LibVLC _libVLC;
        MediaPlayer _mediaPlayer;

        private DispatcherTimer videotimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            Core.Initialize();

            _libVLC = new LibVLC();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            var FileStream = new FileStream(path: txtBFilePath.Text, mode: FileMode.Open, access: FileAccess.Read);

            var streamWrapper = new SeekableStreamWrapper(() =>
            {
                FileStream.Seek(0, SeekOrigin.Begin);
                RijndaelManaged AES = new RijndaelManaged();
                SHA256Cng SHA256 = new SHA256Cng();

                AES.Key = SHA256.ComputeHash(Encoding.ASCII.GetBytes("U[#x5:jg0$e-^etBx#MjWH5Zu_ndd9"));
                AES.Mode = CipherMode.ECB;
                return new CryptoStream(FileStream, AES.CreateDecryptor(), CryptoStreamMode.Read, true);
            });

            _mediaPlayer = new MediaPlayer(_libVLC);

            videoView.MediaPlayer = _mediaPlayer;

            _mediaPlayer.Play(new Media(_libVLC, new StreamMediaInput(streamWrapper)));

            Videostatusclock();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
        }

        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            if (o.ShowDialog() == true)
            {
                txtBFilePath.Text = o.FileName;
            }
        }

        private void Videostatusclock()
        {
            videotimer.Interval = TimeSpan.FromSeconds(1);
            videotimer.Tick += VideoTimerTick;
            videotimer.Start();
        }

        private void VideoTimerTick(object sender, EventArgs e)
        {
            if (_mediaPlayer == null)
            {
                return;
            }

            if (_mediaPlayer.Length != 0)
            {
                lblStart.Content = formatLongToTimeStr(_mediaPlayer.Time);
                lblTotal.Content = formatLongToTimeStr(_mediaPlayer.Length);
                sliderVideo.Maximum = _mediaPlayer.Length;
                sliderVideo.Value = _mediaPlayer.Time;
            }
        }

        public static string formatLongToTimeStr(long l)
        {
            int hour = 0;
            int minute = 0;
            int second = 0;

            second = (int)l / 1000;

            if (second > 60)
            {
                minute = second / 60;
                second = second % 60;
            }
            if (minute > 60)
            {
                hour = minute / 60;
                minute = minute % 60;
            }
            return $"{hour:00}:{minute:00}:{second:00}";
        }

        private void sliderVideo_Drag(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            if (_mediaPlayer == null)
            {
                return;
            }

            _mediaPlayer.Time = (long)sliderVideo.Value;
        }

        private void sliderVideo_MouseLefButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_mediaPlayer == null)
            {
                return;
            }

            _mediaPlayer.Time = (long)sliderVideo.Value;
        }

    }
}
