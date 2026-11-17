using UnityEngine;
using IslandSurvival.Core;

namespace IslandSurvival.Audio
{
    /// <summary>
    /// Lightweight ambient audio using procedurally generated soft noise clips.
    /// Replace with licensed audio assets for production polish.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Range(0, 1)] public float masterVolume = 0.7f;
        [Range(0, 1)] public float ambientVolume = 0.35f;
        [Range(0, 1)] public float sfxVolume = 0.6f;

        AudioSource _ambient;

        void Awake()
        {
            Instance = this;
            _ambient = gameObject.AddComponent<AudioSource>();
            _ambient.loop = true;
            _ambient.playOnAwake = false;
            _ambient.spatialBlend = 0f;
            _ambient.clip = MakeSoftNoise(48000 * 4);
            _ambient.volume = ambientVolume * masterVolume;
            _ambient.Play();
            GameEvents.OnItemCollected += OnCollect;
        }

        void OnDestroy() => GameEvents.OnItemCollected -= OnCollect;

        void OnCollect(string id, int amt)
        {
            PlayBlip(0.6f, 0.08f);
        }

        public void PlayBlip(float pitch, float duration)
        {
            var go = new GameObject("SFX");
            var src = go.AddComponent<AudioSource>();
            src.clip = MakeTone(880f * pitch, duration);
            src.volume = sfxVolume * masterVolume;
            src.Play();
            Destroy(go, duration + 0.05f);
        }

        AudioClip MakeSoftNoise(int samples)
        {
            var clip = AudioClip.Create("Ambient", samples, 1, 22050, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
                data[i] = (Mathf.PerlinNoise(i * 0.001f, 0.3f) - 0.5f) * 0.15f;
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip MakeTone(float freq, float duration)
        {
            int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var clip = AudioClip.Create("Tone", samples, 1, sampleRate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float env = 1f - (t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.25f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
