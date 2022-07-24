namespace FNF_CSharp.Utility;

public struct BpmChangeEvent {
    public int StepTime;
    public float SongTime;
    public float Bpm;
    public float? StepCrochet;

    public BpmChangeEvent(int stepTime, float songTime, float bpm, float? stepCrochet) {
        StepTime = stepTime;
        SongTime = songTime;
        Bpm = bpm;
        StepCrochet = stepCrochet;
    }
}
