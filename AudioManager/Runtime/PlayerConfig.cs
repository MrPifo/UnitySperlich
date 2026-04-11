using UnityEngine;

namespace Sperlich.Audio {
	public class PlayerConfig {

		public readonly Sounds Sound;
		public readonly VolumeType Type;
		public readonly PlayerPreset Preset;
		public readonly bool Is3D;

		public float FadeInDuration { get; set; } = 0f;
		public float FadeOutDuration { get; set; } = 0f;
		public float Volume { get; set; } = 1f;
		public float MinPitch { get; set; } = 1f;
		public float MaxPitch { get; set; } = 1f;
		public bool Loop { get; set; } = false;
		public float Pitch {
			get {
				return Mathf.Clamp(MinPitch * Mathf.Exp(Random.Range(0f, 1f) * Mathf.Log(MaxPitch / MinPitch)), MinPitch, MaxPitch);
			}
		}
		public AudioClip Clip { get; private set; }

		public PlayerConfig(AudioClip clip, VolumeType type) {
			this.Type = type;
			this.Preset = default;
			SetClip(clip);
		}
		public PlayerConfig(AudioClip clip, VolumeType type, PlayerPreset preset) {
			this.Type = type;
			this.Preset = preset;
			SetClip(clip);
		}
		public PlayerConfig(Sounds sound, VolumeType type) {
			this.Sound = sound;
			this.Type = type;
			this.Preset = default;
			Clip = AudioManager.Library.GetClip(sound);
		}
		public PlayerConfig(Sounds sound, VolumeType type, PlayerPreset preset) {
			this.Sound = sound;
			this.Type = type;
			this.Preset = preset;
			Clip = AudioManager.Library.GetClip(sound);
		}
		internal PlayerConfig(Sounds sound, VolumeType type, PlayerPreset preset, bool is3D) {
			this.Sound = sound;
			this.Type = type;
			this.Preset = preset;
			Is3D = is3D;
			Clip = AudioManager.Library.GetClip(sound);
		}
		public PlayerConfig(Sounds sound, VolumeType type, PlayerPreset preset, float volume, float minPitch, float maxPitch, bool loop) {
			this.Sound = sound;
			this.Type = type;
			this.Preset = preset;
			Volume = volume;
			MinPitch = minPitch;
			MaxPitch = maxPitch;
			Loop = loop;
			Clip = AudioManager.Library.GetClip(sound);
		}

		public virtual void Apply(AudioSource source) {
			source.pitch = Pitch;
			source.clip = Clip;
			source.loop = Loop;
			source.volume = Volume;
			source.spatialBlend = 0f;
			source.spread = 0;
			source.maxDistance = float.MaxValue;
			source.minDistance = 0;
			source.transform.position = Vector3.zero;
		}
		public void SetClip(AudioClip clip) {
			Clip = clip;
		}
	}

	public class PlayerConfig3D : PlayerConfig {

		private Vector3 _worldPos;

		public float MinDistance { get; set; }
		public float MaxDistance { get; set; } = float.MaxValue;
		public float Spread { get; set; } = 0f;
		public float Spatial { get; set; }
		public Vector3 Pos {
			get {
				if(Anchor == null) {
					return _worldPos;
				}

				return Anchor.position;
			}
			set {
				_worldPos = value;

				if (Anchor != null) {
					Anchor.position = value;
				}
			}
		}
		public Transform Anchor { get; set; }

		public PlayerConfig3D(AudioClip clip, VolumeType type) : base(clip, type) {
			
		}
		public PlayerConfig3D(AudioClip clip, VolumeType type, PlayerPreset preset) : base(clip, type, preset) {
			
		}
		public PlayerConfig3D(Sounds sound, VolumeType type) : base(sound, type, default, true) {

		}
		public PlayerConfig3D(Sounds sound, VolumeType type, PlayerPreset preset, Vector3 pos) : base(sound, type, preset, true) {
			this.Pos = pos;
		}
		public PlayerConfig3D(Sounds sound, VolumeType type, PlayerPreset preset, Vector3 pos, float spatial, float maxDist) : base(sound, type, preset, true) {
			this.Pos = pos;
			this.Spatial = spatial;
			this.MaxDistance = maxDist;
		}
		public PlayerConfig3D(Sounds sound, VolumeType type, PlayerPreset preset, Transform anchor, float spatial, float maxDist) : base(sound, type, preset, true) {
			Anchor = anchor;
			Pos = anchor.position;
			this.Spatial = spatial;
			this.MaxDistance = maxDist;
		}

		public override void Apply(AudioSource source) {
			base.Apply(source);

			source.spatialBlend = Spatial;
			source.minDistance = MinDistance;
			source.maxDistance = MaxDistance;
			source.spread = Spread;
			source.transform.position = Pos;
		}
	}
}