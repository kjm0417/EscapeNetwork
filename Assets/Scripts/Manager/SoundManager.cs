using System;
using System.Collections.Generic;
using UnityEngine;

public enum AudioCategory
{
    WalkSound,
    SFX
}

public class SoundManager : Singleton<SoundManager>
{
    private AudioSource sfxSource;
    private AudioSource walkSource;
    
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>(); //오디오 데이터에 정보를 이벤트 별로 저장
    
    [Serializable]
    public class AudioData
    {
        public string eventName;
        public AudioCategory audioCategory;
        public AudioClip clip;
    }

    public List<AudioData> audioDataList;

    private new void Awake()
    {
        sfxSource = GetComponent<AudioSource>();
        walkSource = GetComponent<AudioSource>();
        
        walkSource.loop = true; //걷는 소리는 눌렸을 때 무한으로 발생하기 때문에
        
        for (int i = 0; i < audioDataList.Count; i++)
        {
            audioClips.Add(audioDataList[i].eventName, audioDataList[i].clip);
        }
    }

    private void OnEnable() //등록을 위한 곳
    {
        for (int i = 0; i < audioDataList.Count; i++)
        {
            if (audioDataList[i].audioCategory == AudioCategory.SFX)
            {
                var i1 = i;
                EventBus.Subscribe(audioDataList[i].eventName, () => SfxPlay(audioDataList[i1].eventName));
            }
        }
    }

    private void SfxPlay(string eventName)
    {
        if (audioClips.TryGetValue(eventName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void SetWalkingState(bool isWalking)
    {
        if (isWalking)
        {
            if (!walkSource.isPlaying)
            {
                if (audioClips.TryGetValue("Walking", out AudioClip walkingClip))
                {
                    walkSource.clip = walkingClip;
                    walkSource.Play();
                }
            }
        }
        else
        {
            if (walkSource.isPlaying)
            {
                walkSource.Stop();
                
            }
        }
    }



}
