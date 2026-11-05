using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip [] bgmClip;
    [SerializeField] AudioClip [] sfxClip;


    void Start()
    {
        PlayBGM(bgmClip[0]);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlayBGM(AudioClip clip)
    {
        bgmSource.loop = true;
        bgmSource.clip = clip;
        bgmSource.Play();
    }
    
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}