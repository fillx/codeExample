using System;
using _Game.Scripts.Interfaces;
using _Game.Scripts.Systems.Save;
using Sirenix.Serialization;
using Zenject;

namespace _Game.Scripts.ScriptableObjects
{
	/// <summary>
	/// Используется для хранения внутриигровых параметров
	/// </summary>
	public class GameSettings
	{
		public event Action SoundChangedEvent, NotificationsEvent;

		[Inject] private SaveSystem _save;
		
		public bool IsMuteMusic { get; private set; }
		public bool IsMuteSound { get; private set; }
		public bool IsDisablePushNotifications { get; private set; }

		public bool MuteMusic
		{
			get => IsMuteMusic;
			set => SetMuteMusic(value);
		}
		
		public bool MuteSound
		{
			get => IsMuteSound;
			set => SetMuteSound(value);
		}
		
		public bool DisablePushNotifications
		{
			get => IsDisablePushNotifications;
			set => SetDisablePushNotification(value);
		}

		private void SetDisablePushNotification(bool value)
		{
			IsDisablePushNotifications = value;
			NotificationsEvent?.Invoke();
		}

		private void SetMuteMusic(bool mute)
		{
			IsMuteMusic = mute;
			SoundChangedEvent?.Invoke();
		}
		
		private void SetMuteSound(bool mute)
		{
			IsMuteSound = mute;
			SoundChangedEvent?.Invoke();
		}

		public void CopyFrom(object source)
		{
			var source1 = (GameSettings)source;
			IsMuteMusic = source1.IsMuteMusic;
			IsMuteSound = source1.IsMuteSound;
			IsDisablePushNotifications = source1.IsDisablePushNotifications;
		}
	}
}