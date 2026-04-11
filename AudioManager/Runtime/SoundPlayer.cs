using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.Audio {
	public class SoundPlayer : MonoBehaviour {

		public PlayerPreset preset;
		public bool isPlaying;
		public bool isPaused;
		public bool isLooping;
		public AudioClip Clip => Config.Clip;
		public AudioSource Source {
			get {
				if(_source == null) {
					_source = GetComponent<AudioSource>();
					_source.rolloffMode = AudioRolloffMode.Linear;
				}
				return _source;
			}
			set => _source = value;
		}
		private AudioSource _source;
		private AudioReverbFilter _reverbFilter;
		private Transform bindParent;
		public AudioReverbFilter ReverbFilter {
			get {
				if(_reverbFilter == null) {
					_reverbFilter = GetComponent<AudioReverbFilter>();
				}
				return _reverbFilter;
			}
			set => _reverbFilter = value;
		}
		public float Pitch { get => _source.pitch; set => _source.pitch = value; }
		public float Volume { get => _source.volume; }
		public float Spatial { get => _source.spatialBlend; set => _source.spatialBlend = value; }
		public UnityEvent OnPlayComplete { get; private set; } = new UnityEvent();
		public PlayerConfig Config { get; set; }

		private bool isVolumeFading;
		internal bool _isFree = true;
		/// <summary>
		/// Set this to true to avoid that this Player is automaticially freed.
		/// </summary>
		public bool Reserve { get; set; }
		public bool IsFree => _isFree && Reserve == false && Source.isPlaying == false;

		internal void Initialize(PlayerConfig config) {
			if (_isFree) {
				_isFree = false;
				Config = config;
				bindParent = null;
				name = $"{Clip.name}_Playing";
				Source.playOnAwake = false;
				isLooping = Config.Loop;
				isPlaying = true;
				config.Apply(_source);
				_source.Play();
				SetVolume(config.Volume);

				if(config.FadeInDuration > 0) {
					FadeIn(config.FadeInDuration);
				}

				if (Config.Loop == false) {
					StartCoroutine(IDelay());
					IEnumerator IDelay() {
						float time = 0;
						while (time < Clip.length) {
							yield return null;
							if (isPaused == false) {
								time += Time.deltaTime;
							}

							if(config.FadeOutDuration > 0 && time > Clip.length - config.FadeOutDuration) {
								break;
							}
						}

						if (config.FadeOutDuration <= 0) {
							OnPlayComplete.Invoke();
							isPlaying = false;
							Free();
						} else {
							Stop(config.FadeOutDuration);
						}
					}
				}
			}
		}
		public void Play(PlayerConfig config) {
			_isFree = true;
			Initialize(config);
		}
		public void Play() {
			_isFree = false;
			Initialize(Config);
		}
		public void Stop() {
			Source.Stop();
			OnPlayComplete.RemoveAllListeners();

			name = $"{Clip.name}_Stopped";
			bindParent = null;
			_isFree = true;
		}
		public void Stop(float fadeTime) {
			StartCoroutine(Fade());
			IEnumerator Fade() {
				float time = fadeTime;
				float startVolume = Source.volume;
				while (time > 0 && _source.isPlaying) {
					_source.volume = time.Remap(0f, fadeTime, 0f, startVolume);
					yield return null;
					time -= Time.deltaTime;
				}
				_source.Stop();
				bindParent = null;
				name = $"{Clip.name}_Stopped";
				Free();
			}
        }
		public void Pause() {
			Source.Pause();

			name = $"{Clip.name}_Paused";
		}
		public void Resume() {
			Source.UnPause();

			name = $"{Clip.name}_Playing";
		}
		public void SetClip(AudioClip clip) {
			_source.clip = clip;
		}
		public void SetPos(Vector3 pos) {
			transform.position = pos;
		}
		public void BindPos(Transform parent) {
			bindParent = parent;
			StartCoroutine(ICopyPos());

			IEnumerator ICopyPos() {
				while(bindParent != null) {
					transform.position = parent.position;
					yield return null;
				}
			}
		}
		public void SetVolume(float volume) {
			Config.Volume = Mathf.Clamp01(volume);
			float targetVolume;

			if(Config.Type == default) {
				targetVolume = volume * AudioManager.GlobalVolume;
			} else {
				targetVolume = AudioManager.AudioVolumes[Config.Type] * volume * AudioManager.GlobalVolume;
			}

			Config.Volume = volume;
			_source.volume = targetVolume;
		}

		float GetVolume() {
			float volume = Config.Volume;
			if (Config.Type == default) {
				volume *= AudioManager.GlobalVolume;
			} else {
				volume = AudioManager.AudioVolumes[Config.Type] * volume * AudioManager.GlobalVolume;
			}

			return volume;
		}
		internal void Free() {
			isPlaying = false;
			_isFree = true;
			name = $"{Clip.name}_Finished";
			OnPlayComplete.RemoveAllListeners();
		}

		public SoundPlayer FadeIn(float fadeTime) {
			StartCoroutine(Fade());

			IEnumerator Fade() {
				isVolumeFading = true;
				float targetVolume = GetVolume();
				float time = 0f;
				_source.volume = 0f;

				while (time < fadeTime && _source.isPlaying) {
					_source.volume = time.Remap(0f, fadeTime, 0f, targetVolume);

					yield return null;
					time += Time.deltaTime;
				}

				isVolumeFading = false;
				if (_source.isPlaying == false) {
					_source.volume = targetVolume;
				}
			}
			return this;
		}
		public SoundPlayer FadeOut(float fadeTime) {
			StartCoroutine(Fade());

			IEnumerator Fade() {
				isVolumeFading = true;
				float fromVolume = _source.volume;
				float time = 0f;
				_source.volume = 0f;

				while (time < fadeTime && _source.isPlaying) {
					_source.volume = time.Remap(0f, fadeTime, fromVolume, 0f);

					yield return null;
					time += Time.deltaTime;
				}

				isVolumeFading = false;
				if (_source.isPlaying == false) {
					_source.volume = 0f;
				}
			}
			return this;
		}
	}
}