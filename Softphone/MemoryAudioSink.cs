using System;
using System.IO;
using System.Windows.Media;
namespace Softphone
{
	internal class MemoryAudioSink : AudioSink
	{
		private MemoryStream stream;
		private AudioFormat _format;
		private bool isMuted;
		public event EventHandler<GenericEventArgs<byte[]>> OnBufferFulfill;
		public event EventHandler MuteChanged;
		public bool IsMuted
		{
			get
			{
				return this.isMuted;
			}
			set
			{
				this.isMuted = value;
			}
		}
		public Stream BackingStream
		{
			get
			{
				return this.stream;
			}
		}
		public AudioFormat CurrentFormat
		{
			get
			{
				return this._format;
			}
		}
		public bool setMute(bool mute)
		{
			this.isMuted = mute;
			this.MuteChanged.Invoke(true, null);
			return true;
		}
		protected override void OnCaptureStarted()
		{
			this.stream = new MemoryStream();
		}
		protected override void OnCaptureStopped()
		{
		}
		protected override void OnFormatChange(AudioFormat audioFormat)
		{
			if (audioFormat.WaveFormat != WaveFormatType.Pcm)
			{
				throw new InvalidOperationException("MemoryAudioSink supports only PCM audio format.");
			}
			this._format = audioFormat;
		}
		protected override void OnSamples(long sampleTime, long sampleDuration, byte[] sampleData)
		{
			if (this.OnBufferFulfill != null)
			{
				int num = sampleData.Length;
				if (this.IsMuted)
				{
					byte[] item = new byte[num];
					this.OnBufferFulfill.Invoke(this, new GenericEventArgs<byte[]>(item));
					return;
				}
				this.OnBufferFulfill.Invoke(this, new GenericEventArgs<byte[]>(sampleData));
			}
		}
	}
}
