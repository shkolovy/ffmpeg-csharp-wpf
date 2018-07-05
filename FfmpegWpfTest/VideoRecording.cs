using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace FfmpegWpfTest
{
	public sealed class VideoRecording : IDisposable
	{
		private VideoRecordingOptions _videoRecordingOptions;
		private BlockingCollection<Bitmap> _frames;
		private VideoWriter _writer;

		private Task _writeFrameTask;
		private Task _captureFrameTask;

		public VideoRecording(VideoRecordingOptions videoRecordingOptions)
		{
			_frames = new BlockingCollection<Bitmap>();
			_writer = new VideoWriter(videoRecordingOptions);
			_videoRecordingOptions = videoRecordingOptions;
		}

		public void Start()
		{
			_captureFrameTask = Task.Run(() => CaptureFrames());
			_writeFrameTask = Task.Run(() => WriteFrames());
		}

		public void Dispose()
		{
			_writer.Dispose();
		}

		public async Task Stop()
		{
			_frames.CompleteAdding();
			await _captureFrameTask;
			await _writeFrameTask;
			Dispose();
		}

		private Bitmap CaptureFrame()
		{
			var bitmap = new Bitmap(_videoRecordingOptions.Width, _videoRecordingOptions.Height);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.CopyFromScreen(_videoRecordingOptions.X, _videoRecordingOptions.Y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
			}

			return bitmap;
		}

		private async Task CaptureFrames()
		{
			Task<Bitmap> task = null;
			var frameInterval = TimeSpan.FromSeconds(1.0 / _videoRecordingOptions.FrameRate);

			while (!_frames.IsAddingCompleted)
			{
				var timestamp = DateTime.Now;

				task = Task.Run(() => CaptureFrame());

				if (task != null)
				{
					var frame = await task;

					try
					{
						_frames.Add(frame);
					}
					catch (InvalidOperationException)
					{
						//ignore this ex. Last frame can't be added if IsAddingCopleated is set in another thread
					}
				}

				var timeTillNextFrame = timestamp + frameInterval - DateTime.Now;

				if (timeTillNextFrame > TimeSpan.Zero)
				{
					Thread.Sleep(timeTillNextFrame);
				}
			}
		}

		private async Task WriteFrames()
		{
			while (!_frames.IsCompleted)
			{
				_frames.TryTake(out var frame, -1);

				if (frame != null)
				{
					await _writer.WriteFrame(frame);
				}
			}
		}
	}
}