using System.Diagnostics.CodeAnalysis;
using FNF_CSharp.Utility;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Input.InputListeners;

namespace FNF_CSharp.States;

[SuppressMessage("ReSharper", "StringLiteralTypo")] // Asset and animation names
public class TitleState : MusicBeatState {
    private SoundEffectInstance _confirmSfx = null!;
    private Tuple<AnimatedSprite, Vector2> _logo = null!;
    private Tuple<AnimatedSprite, Vector2> _pressEnter = null!;

    public override void LoadContent() {
        var mainMusic = Game.Content.Load<Song>("Music/freakyMenu");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(mainMusic);
        Conductor.ChangeBpm(102);

        // Coordinates are hand-picked, values from base FNF don't work :(
        var girlfriend = FnfGame.LoadTextureAtlas("gfDanceTitle", 900, 350);
        InWorld.Add(girlfriend);
        // Mistyping an animation name will throw a NullReferenceException!
        girlfriend.Item1.Play("gfDance").IsLooping = true;

        _logo = FnfGame.LoadTextureAtlas("logoBumpin", 350, 235);
        InWorld.Add(_logo);

        _pressEnter = FnfGame.LoadTextureAtlas("titleEnter", 850, 670);
        InWorld.Add(_pressEnter);
        _pressEnter.Item1.Play("ENTER IDLE").IsLooping = true;

        _confirmSfx =
            Game.Content.Load<SoundEffect>("Sounds/confirmMenu").CreateInstance(); // Load now to avoid a freeze
    }

    protected override void OnBeatHit() {
        _logo.Item1.Play("logo bumpin");
    }

    public override void Update() {
        Conductor.PlayPosition = (float)MediaPlayer.PlayPosition.TotalMilliseconds;
        base.Update();
    }

    public override void UnloadContent() {
        Game.Content.UnloadAsset("Images/gfDanceTitle");
        Game.Content.UnloadAsset("Images/logoBumpin");
        Game.Content.UnloadAsset("Images/titleEnter");
    }

    public override void OnKeyPress(object? sender, KeyboardEventArgs args) {
        // TODO: MainMenuState
        if (args.Key != Keys.Enter && args.Key != Keys.Space) return;
        _pressEnter.Item1.Play("ENTER PRESSED").IsLooping = true;
        _confirmSfx.Play();
    }
}
