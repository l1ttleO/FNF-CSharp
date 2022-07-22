using System.Diagnostics.CodeAnalysis;
using MonoGame.Extended.Input.InputListeners;

namespace FNF_CSharp;

[SuppressMessage("ReSharper", "StringLiteralTypo")] // Asset and animation names
public class TitleState : State {
    public override void LoadContent() {
        var mainMusic = Game.Content.Load<Song>("Music/freakyMenu");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(mainMusic);

        // Coordinates are hand-picked, values from base FNF don't work :(
        var girlfriend = FnfGame.LoadTextureAtlas("gfDanceTitle", 900, 350);
        InWorld.Add(girlfriend);
        // Mistyping an animation name will throw a NullReferenceException!
        girlfriend.Item1.Play("gfDance").IsLooping = true;

        var logo = FnfGame.LoadTextureAtlas("logoBumpin", 350, 235);
        InWorld.Add(logo);
        logo.Item1.Play("logo bumpin").IsLooping = true;

        var pressEnter = FnfGame.LoadTextureAtlas("titleEnter", 850, 670);
        InWorld.Add(pressEnter);
        pressEnter.Item1.Play("ENTER IDLE").IsLooping = true;
    }

    public override void UnloadContent() {
        Game.Content.UnloadAsset("Images/gfDanceTitle");
        Game.Content.UnloadAsset("Images/logoBumpin");
        Game.Content.UnloadAsset("Images/titleEnter");
    }

    public override void OnKeyPress(object? sender, KeyboardEventArgs args) {
        // TODO: MainMenuState
        if (args.Key == Keys.Enter) InWorld.Last().Item1.Play("ENTER PRESSED").IsLooping = true;
    }
}
