using UnityEngine;

public class Instrument : MonoBehaviour, IInteractable
{
    public Conductor conductor;
    public AudioClip clip;
    public int poolSize = 4;          // 2 is minimum, 4 is safer

    private AudioSource[] sources;
    private int sourceIndex = 0;

    private double nextPlayTime;
    private double beatInterval;
    private bool isPlaying = false;

    void Awake()
    {
        // Create a small pool of AudioSources
        sources = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.clip = clip;
            s.playOnAwake = false;
            s.loop = false;
            sources[i] = s;
        }
    }

    public void TurnOn()
    {
        if (isPlaying) return;

        isPlaying = true;
        nextPlayTime = conductor.GetNextBeatTime();
        ScheduleNextHit();
    }

    public void TurnOff()
    {
        isPlaying = false;
        foreach (var s in sources)
            s.Stop();
    }

    public void Interact(){
        Debug.Log("Hit");
    }

    void ScheduleNextHit()
    {
        AudioSource s = sources[sourceIndex];
        s.PlayScheduled(nextPlayTime);

        sourceIndex = (sourceIndex + 1) % sources.Length;
        nextPlayTime += beatInterval;
    }

    void Start()
    {
        beatInterval = 60.0 / conductor.bpm;
        // this.TurnOn();   // starts continuous on-beat playback
    }


    void Update()
    {
        if (!isPlaying) return;

        // Schedule slightly ahead of time
        while (AudioSettings.dspTime + 0.15f > nextPlayTime)
        {
            ScheduleNextHit();
        }
    }

    void OnDestroy()
    {
        TurnOff();
    }
}
