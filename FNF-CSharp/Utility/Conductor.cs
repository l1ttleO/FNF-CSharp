namespace FNF_CSharp.Utility;

public static class Conductor {
    private static float _bpm = 100;
    private static float _crochet = 60 / _bpm * 1000;

    private static float _stepCrochet = _crochet / 4;
    private static readonly BpmChangeEvent[] _bpmChanges = { };
    public static float PlayPosition = 0;

    private static float Crochet {
        get => _crochet;
        set => _crochet = 60 / value * 1000;
    }

    public static void ChangeBpm(float newBpm) {
        _bpm = newBpm;
        Crochet = _bpm;
        _stepCrochet = Crochet / 4;
    }

    public static BpmChangeEvent GetBpmAt(float millis) {
        var lastEvent = new BpmChangeEvent(0, 0, _bpm, _stepCrochet);
        foreach (var bpmChange in _bpmChanges)
            if (millis >= bpmChange.SongTime)
                return bpmChange;
        return lastEvent;
    }
}
