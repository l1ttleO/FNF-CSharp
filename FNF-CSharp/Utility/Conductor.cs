namespace FNF_CSharp.Utility;

public static class Conductor {
    private static float _bpm = 100;
    private static float _crochet = 60 / _bpm * 1000;
    private static float _stepCrochet = _crochet / 4;
    private static readonly BpmChangeEvent[] BpmChanges = Array.Empty<BpmChangeEvent>();
    public static float PlayPosition = 0;

    public static float Bpm {
        get => _bpm;
        set {
            _bpm = value;
            _crochet = 60 / value * 1000;
            _stepCrochet = _crochet / 4;
        }
    }

    public static BpmChangeEvent GetBpmAt(float millis) {
        var lastEvent = new BpmChangeEvent(0, 0, Bpm, _stepCrochet);
        foreach (var bpmChange in BpmChanges)
            if (millis >= bpmChange.SongTime)
                return bpmChange;
        return lastEvent;
    }
}
