using System;
using System.Windows;
using System.Windows.Threading;

namespace FfmpegWpfTest
{
	public sealed class Timer
	{
		private DispatcherTimer _timer;
		private TimeSpan _time;
		private Action<TimeSpan> _action;

		public Timer(Action<TimeSpan> action)
		{
			_time = TimeSpan.FromSeconds(0);
			_action = action;
		}

		public void Start()
		{
			_action(_time);
			_timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Background, (s, e) => {
				_time = _time.Add(TimeSpan.FromSeconds(1));
				_action(_time);
			}, Application.Current.Dispatcher);
		}

		public void Stop()
		{
			_timer.Stop();
		}

		public void Reset()
		{
			_time = TimeSpan.FromSeconds(0);
		}
	}
}