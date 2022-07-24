using MonoGame.Extended.Animations;
using MonoGame.Extended.Input.InputListeners;

namespace FNF_CSharp.States;

public abstract class State {
    protected readonly FnfGame Game = FnfGame.Get();
    public readonly List<Tuple<AnimatedSprite, Vector2>> InWorld = new(); // To draw and update sprites, add them here

    public abstract void LoadContent();
    public abstract void Update();
    public abstract void UnloadContent();
    public abstract void OnKeyPress(object? sender, KeyboardEventArgs args);
}
