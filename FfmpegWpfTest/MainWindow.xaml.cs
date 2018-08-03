using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FfmpegWpfTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool _isRecording;
		private Timer _timer;
		private VideoRecording _videoRecording;
		private VideoRecordingOptions _videoRecordingOptions;

		public MainWindow()
		{
			InitializeComponent();

			_timer = new Timer((TimeSpan time) => {
				TimerLbl.Content = time.ToString();
			});

			_videoRecordingOptions = new VideoRecordingOptions
			{
				FrameRate = 10,
				Height = 900,
				Width = 900,
				X = 300,
				Y = 300,
				Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos"),
				FileName = "out.mp4"
			};
		}

		private void StartBtn_Click(object sender, RoutedEventArgs e)
		{
			_timer.Reset();
			_timer.Start();
			StopBtn.Visibility = Visibility.Visible;
			StartBtn.Visibility = Visibility.Hidden;

			_isRecording = !_isRecording;

			_videoRecording = new VideoRecording(_videoRecordingOptions);
			_videoRecording.Start();
		}

		private async void StopBtn_Click(object sender, RoutedEventArgs e)
		{
			_timer.Stop();
			StopBtn.Visibility = Visibility.Hidden;
			StartBtn.Visibility = Visibility.Visible;

			_isRecording = !_isRecording;

			await _videoRecording.Stop();
			OpenFolder(_videoRecordingOptions.Path);
		}

		private static void OpenFolder(string path)
		{
			var startInformation = new ProcessStartInfo { FileName = path };
			Process.Start(startInformation);
		}
	}
}
