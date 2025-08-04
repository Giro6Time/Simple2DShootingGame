using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace GiroFrame
{


    public class AudioManager : ManagerBase<AudioManager>
    {
        [SerializeField]
        private AudioSource BGAudioSource;
        [SerializeField]
        private GameObject prefab_AudioPlay;
        // 场景中生效的音乐播放器
        [SerializeField]
        private List<AudioSource> audioPlayList = new List<AudioSource>();

        #region  音量、播放控制
        [SerializeField]
        [Range(0, 1)]
        [OnValueChanged("UpdateAllAudioPlay")]
        private float globalVolume;
        public float GlobalVolume
        {
            get => globalVolume; set
            {
                if (globalVolume == value) return;
                globalVolume = value; UpdateAllAudioPlay();
            }
        }
        [SerializeField]
        [Range(0, 1)]
        [OnValueChanged("UpdateBGAudioPlay")]
        private float bgVolume;
        public float BGVolume
        {
            get => bgVolume; set
            {
                if (bgVolume == value) return;
                bgVolume = value; UpdateBGAudioPlay();
            }
        }

        [SerializeField]
        [Range(0, 1)]
        [OnValueChanged("UpdateEffectAudioPlay")]
        private float effectVolume;
        public float EffectVolume
        {
            get => bgVolume;
            set
            {
                if (effectVolume == value) return;
                effectVolume = value; UpdateEffectAudioPlay();
            }
        }

        [SerializeField]
        [OnValueChanged("UpdateMute")]
        private bool isMute = false;
        public bool IsMute
        {
            get => isMute;
            set
            {
                if (isMute == value) return;
                isMute = value;
                UpdateMute();
            }
        }
        [SerializeField]
        [OnValueChanged("UpdateLoop")]
        private bool isLoop = true;
        public bool IsLoop
        {
            get => isLoop;
            set
            {
                if (isLoop == value) return;
                isLoop = value;
                UpdateLoop();
            }
        }
        [SerializeField]
        [OnValueChanged("UpdatePause")]
        private bool isPause = false;
        public bool IsPause
        {
            get => isPause;
            set
            {
                if (isPause == value) return;
                isPause = value;
                UpdatePause();
            }
        }

        /// <summary> 更新全部音乐
        /// </summary>
        private void UpdateAllAudioPlay()
        {
            UpdateBGAudioPlay();
            UpdateEffectAudioPlay();
        }
        /// <summary> 更新背景音乐
        /// </summary>
        private void UpdateBGAudioPlay()
        {
            BGAudioSource.volume = bgVolume * globalVolume;
        }
        /// <summary> 更新特效播放器
        /// </summary>
        private void UpdateEffectAudioPlay()
        {
            for (int i = audioPlayList.Count - 1; i >= 0; i--)
            {
                if (audioPlayList[i] != null)
                {
                    SetEffectAudioPlay(audioPlayList[i]);
                }
                else
                {
                    audioPlayList.RemoveAt(i);
                }

            }
        }
        /// <summary>设置音效播放器 
        /// </summary>
        private void SetEffectAudioPlay(AudioSource audioPlay, float spatial = -1)
        {
            audioPlay.mute = isMute;
            audioPlay.volume = effectVolume * globalVolume;
            if (spatial != -1)
            {
                audioPlay.spatialBlend = spatial;
            }
            if (isPause)
            {
                audioPlay.Pause();
            }
            else
            {
                audioPlay.UnPause();
            }
        }
        /// <summary>更新BGM静音情况
        /// </summary>
        private void UpdateMute()
        {
            BGAudioSource.mute = isMute;
            UpdateEffectAudioPlay();
        }
        /// <summary>更新BGM循环情况
        /// </summary>
        private void UpdateLoop()
        {
            BGAudioSource.loop = isLoop;
        }
        ///<summary> 更新BGM暂停情况
        ///</summary>
        private void UpdatePause()
        {
            if(isPause)
                BGAudioSource.Pause();
            else
                BGAudioSource.UnPause();
        }
        #endregion

        public override void Init()
        {
            base.Init();
            UpdateAllAudioPlay();
        }

        #region  背景音乐
        public void PlayBGAudio(AudioClip clip, bool loop = true, float volume = -1)
        {
            BGAudioSource.clip = clip;
            IsLoop = loop;
            if (volume != -1)
            {
                BGVolume = volume;
            }
            BGAudioSource.Play();

        }
        public void PlayBGAudio(string clipPath, bool loop = true, float volume = -1)
        {
            AudioClip clip = ResManager.LoadAsset<AudioClip>(clipPath);
            PlayBGAudio(clip, loop, volume);
        }
        #endregion

        #region  特效音乐
        private Transform audioPlayRoot = null;
        /// <summary> 获取音乐播放器
        /// </summary>
        private AudioSource GetAudioPlay(bool is3D)
        {
            if (!audioPlayRoot)
            {
                audioPlayRoot = new GameObject("AudioPlayRoot").transform;
            }
            //从对象池获取播放器
            AudioSource audioSource = PoolManager.Instance.GetGameObject<AudioSource>(prefab_AudioPlay, audioPlayRoot);
            SetEffectAudioPlay(audioSource, is3D ? 1f : 0f);
            audioPlayList.Add(audioSource);
            return audioSource;
        }
        /// <summary>回收播放器
        /// </summary>
        private void RecycleAudioPlay(AudioSource audioSource, AudioClip clip, UnityAction callBack, float time)
        {
            StartCoroutine(DoRecycleAudioPlay(audioSource, clip, callBack, time));
        }
        private IEnumerator DoRecycleAudioPlay(AudioSource audioSource, AudioClip clip, UnityAction callBack, float time)
        {
            //延迟Clip的长度+延迟时长
            yield return new WaitForSeconds(clip.length);
            if (audioSource != null)
            {
                audioSource.GiroGameObjectPushPool();
                yield return new WaitForSeconds(time);
                callBack?.Invoke();
            }

        }
        /// <summary>播放一次特效音乐
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="component">挂载组件</param>
        /// <param name="volumeScale">音量0-1</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后进行</param>
        /// <param name="callBackTime">回调函数-在音乐播放完成后进行的延迟时间</param>
        public void PlayOnShot(AudioClip clip, Component component, float volumeScale = 1, bool is3D = true, UnityAction callBack = null, float callBackTime = 0)
        {
            //初始化音乐播放器
            AudioSource audioSource = GetAudioPlay(is3D);
            audioSource.transform.SetParent(component.transform);
            audioSource.transform.localPosition = Vector3.zero;
            //播放一次音效
            audioSource.PlayOneShot(clip, volumeScale);
            RecycleAudioPlay(audioSource, clip, callBack, callBackTime);
        }

        /// <summary>播放一次特效音乐
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="position">音效的位置</param>
        /// <param name="component">挂载组件</param>
        /// <param name="volumeScale">音量0-1</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后进行</param>
        /// <param name="callBackTime">回调函数-在音乐播放完成后进行的延迟时间</param>
        public void PlayOnShot(AudioClip clip, Vector3 position, float volumeScale = 1, bool is3D = true, UnityAction callBack = null, float callBackTime = 0)
        {
            //初始化音乐播放器
            AudioSource audioSource = GetAudioPlay(is3D);
            audioSource.transform.position = position;

            //播放一次音效
            audioSource.PlayOneShot(clip, volumeScale);
            RecycleAudioPlay(audioSource, clip, callBack, callBackTime);
        }
        /// <summary>播放一次特效音乐
        /// </summary>
        /// <param name="clipPath">音效片段的路径</param>
        /// <param name="component">挂载组件</param>
        /// <param name="volumeScale">音量0-1</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后进行</param>
        /// <param name="callBackTime">回调函数-在音乐播放完成后进行的延迟时间</param>
        public void PlayOnShot(string clipPath, Component component, float volumeScale = 1, bool is3D = true, UnityAction callBack = null, float callBackTime = 0)
        {
            AudioClip audioClip = ResManager.LoadAsset<AudioClip>(clipPath);
            if (audioClip != null) PlayOnShot(audioClip, component, volumeScale, is3D, callBack, callBackTime);
        }
        /// <summary>播放一次特效音乐
        /// </summary>
        /// <param name="clipPath">音效片段的路径</param>
        /// <param name="component">挂载组件</param>
        /// <param name="volumeScale">音量0-1</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后进行</param>
        /// <param name="callBackTime">回调函数-在音乐播放完成后进行的延迟时间</param>
        public void PlayOnShot(string clipPath, Vector3 position, float volumeScale = 1, bool is3D = true, UnityAction callBack = null, float callBackTime = 0)
        {
            AudioClip audioClip = ResManager.LoadAsset<AudioClip>(clipPath);
            if (audioClip != null) PlayOnShot(audioClip, position, volumeScale, is3D, callBack, callBackTime);
        }
        #endregion
    }

}