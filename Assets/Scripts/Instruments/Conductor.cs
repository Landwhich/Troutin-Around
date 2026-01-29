using UnityEngine;

public class Conductor : MonoBehaviour
{
    public double bpm = 120.0;

    private double nextBeatTime;
    private double secondsPerBeat;

    void Start()
    {
        secondsPerBeat = 60.0 / bpm;
        nextBeatTime = AudioSettings.dspTime + 1.5;
    }

    public double GetNextBeatTime()
    {
        double t = nextBeatTime;
        nextBeatTime += secondsPerBeat;
        return t;
    }
}
