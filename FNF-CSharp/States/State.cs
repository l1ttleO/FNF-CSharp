using FNF_CSharp.Utility;
using MonoGame.Extended.Input.InputListeners;

namespace FNF_CSharp.States;

public abstract class State {
    public readonly List<PositionedAnimatedSprite>
        AnimatedSprites = new(); // To draw and update animated sprites, add them here

    protected readonly FnfGame Game = FnfGame.Get();

    public abstract void LoadContent();
    public abstract void Update();
    public abstract void UnloadContent();
    public abstract void OnKeyPress(object? sender, KeyboardEventArgs args);
}