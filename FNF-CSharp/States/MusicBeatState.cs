using FNF_CSharp.Utility;
using MonoGame.Extended.Input.InputListeners;

namespace FNF_CSharp.States;

public class MusicBeatState : State {
    private int _curBeat;
    private double _curDecBeat;
    private double _curDecStep;
    private int _curStep;

    private int CurStep {
        get => _curStep;
        set {
            _curStep = value;
            _curBeat = MathUtility.FloorToInt(value / 4.0);
        }
    }

    private double CurDecStep {
        get => _curDecStep;
        set {
            _curDecStep = value;
            _curDecBeat = value / 4.0;
        }
    }

    public override void Update() {
        var prevStep = CurStep;
        UpdateStep();

        if (prevStep != CurStep && CurStep > 0) OnStepHit();
    }

    private void UpdateStep() {
        var lastBpm = Conductor.GetBpmAt(Conductor.PlayPosition);
        var lastStepCrochet = (float)lastBpm.StepCrochet!;
        var remainder = (Conductor.PlayPosition - lastBpm.SongTime) / lastStepCrochet;
        CurDecStep = lastBpm.StepTime + remainder;
        CurStep = lastBpm.StepTime + MathUtility.FloorToInt(remainder);
    }

    private void OnStepHit() {
        if (CurStep % 4 == 0) OnBeatHit();
    }

    protected virtual void OnBeatHit() { }

    public override void LoadContent() { }
    public override void UnloadContent() { }
    public override void OnKeyPress(object? sender, KeyboardEventArgs args) { }
}
