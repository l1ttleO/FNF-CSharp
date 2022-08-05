using System.Diagnostics.CodeAnalysis;
using FNF_CSharp.Utility;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Input.InputListeners;

namespace FNF_CSharp.States;

[SuppressMessage("ReSharper", "StringLiteralTypo")] // Asset and animation names
public class TitleState : MusicBeatState {
    private SoundEffectInstance _confirmSfx = null!;
    private bool _danceLeft;
    private PositionedAnimatedSprite _girlfriend;
    private PositionedAnimatedSprite _logo;
    private PositionedAnimatedSprite _pressEnter;

    public override void LoadContent() {
        var mainMusic = Game.Content.Load<Song>("Music/freakyMenu");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(mainMusic);
        Conductor.Bpm = 102;

        // Coordinates are hand-picked, values from base FNF don't work :(
        _girlfriend = FnfGame.BuildAnimatedSprite("gfDanceTitle", 900, 350, false);
        _girlfriend.AddAnimation("danceLeft", "gfDance",
            new[] { 30, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
        _girlfriend.AddAnimation("danceRight", "gfDance",
            new[] { 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 });
        _girlfriend.BuildSprite();
        AnimatedSprites.Add(_girlfriend);

        _logo = FnfGame.BuildAnimatedSprite("logoBumpin", 350, 235, false);
        _logo.AddAnimation("bump", "logo bumpin");
        _logo.BuildSprite();
        AnimatedSprites.Add(_logo);

        _pressEnter = FnfGame.BuildAnimatedSprite("titleEnter", 850, 670);
        AnimatedSprites.Add(_pressEnter);
        // Mistyping an animation name will throw a NullReferenceException!
        _pressEnter.Sprite.Play("ENTER IDLE").IsLooping = true;

        _confirmSfx =
            Game.Content.Load<SoundEffect>("Sounds/confirmMenu").CreateInstance(); // Load now to avoid a freeze
    }

    protected override void OnBeatHit() {
        _logo.Sprite.Play("bump");
        _danceLeft = !_danceLeft;
        _girlfriend.Sprite.Play(_danceLeft ? "danceLeft" : "danceRight");
    }

    public override void Update() {
        if (MediaPlayer.State == MediaState.Playing)
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
        _pressEnter.Sprite.Play("ENTER PRESSED").IsLooping = true;
        _confirmSfx.Play();
    }
}
