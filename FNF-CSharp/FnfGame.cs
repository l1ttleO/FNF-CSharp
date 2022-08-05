using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using FNF_CSharp.States;
using FNF_CSharp.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace FNF_CSharp;

[SuppressMessage("ReSharper", "StringLiteralTypo")] // Asset and animation names
public class FnfGame : Game {
    private static FnfGame _thisInstance = null!;
    private static readonly List<Tuple<string, int, int>> EmptyAnimData = new();
    private static readonly XmlDocument Xml = new(); // Do not allocate a new XmlDocument in LoadTextureAtlas
    private readonly GraphicsDeviceManager _graphics;
    private readonly KeyboardListener _keyboard = new();
    private State _currentState;
    private SpriteFont _font = null!;
    private TimeSpan _lastUpdateTime;
    private SpriteBatch _sprites = null!;

    public FnfGame() {
        _thisInstance = this;
        _currentState = new TitleState();

        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.HardwareModeSwitch = true;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void LoadContent() {
        _sprites = new SpriteBatch(_graphics.GraphicsDevice);

        _font = Content.Load<SpriteFont>("Fonts/vcr");
        _currentState.LoadContent();

        base.LoadContent();
    }

    protected override void UnloadContent() {
        Content.Unload();

        base.UnloadContent();
    }

    protected override void Initialize() {
        Window.Title = "Friday Night Funkin': C#";
        Window.AllowAltF4 = false;
        Window.AllowUserResizing = true;

        Components.Add(new InputListenerComponent(this, _keyboard));
        _keyboard.KeyPressed += OnKeyPress;
        _keyboard.KeyPressed += _currentState.OnKeyPress;

        base.Initialize();
    }

    protected override void Draw(GameTime gameTime) {
        if (!IsActive) return;
        _graphics.GraphicsDevice.Clear(Color.Black);

        _sprites.Begin();
        foreach (var sprite in _currentState.AnimatedSprites) {
            var scaledVector = new Vector2(sprite.Position.X * (Window.ClientBounds.Width / 1280f),
                sprite.Position.Y * (Window.ClientBounds.Height /
                                     720f)); // Scale the position because it depends on the window's bounds
            _sprites.Draw(sprite.Sprite, scaledVector);
        }

        // Debug information
        _sprites.DrawString(_font,
            $"Draw FPS:{Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 2).ToString(CultureInfo.InvariantCulture)}",
            new Vector2(10, 10), Color.White);
        _sprites.DrawString(_font,
            $"Update FPS:{Math.Round(1 / _lastUpdateTime.TotalSeconds, 2).ToString(CultureInfo.InvariantCulture)}",
            new Vector2(10, 25), Color.White);
        _sprites.DrawString(_font,
            $"Memory:{(GC.GetTotalMemory(false) / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)}MB",
            new Vector2(10, 40), Color.White);
        _sprites.End();

        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime) {
        if (!IsActive) return;
        foreach (var spriteData in _currentState.AnimatedSprites) spriteData.Sprite.Update(gameTime);
        _currentState.Update();

        _lastUpdateTime = gameTime.ElapsedGameTime; // For FPS display

        base.Update(gameTime);
    }

    private void OnKeyPress(object? sender, KeyboardEventArgs args) {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        // No need to handle every key
        switch (args.Key) {
            case Keys.F11:
                _graphics.ToggleFullScreen();
                break;
            case Keys.D0:
                MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                break;
            case Keys.OemMinus:
                MediaPlayer.Volume -= 0.2f;
                break;
            case Keys.OemPlus:
                MediaPlayer.Volume += 0.2f;
                break;
        }
    }

    public static FnfGame Get() {
        if (_thisInstance == null) throw new ArgumentNullException(); // Should never happen
        return _thisInstance;
    }

    public static void SwitchState(MusicBeatState newState) {
        var game = Get();
        game._currentState.UnloadContent();
        game._keyboard.KeyPressed -= game._currentState.OnKeyPress;
        newState.LoadContent();
        game._keyboard.KeyPressed += newState.OnKeyPress;
        game._currentState = newState;
    }

    public static PositionedAnimatedSprite BuildAnimatedSprite(string assetName, int x, int y,
        bool autoLoadAnims = true) {
        Xml.Load($"Content/Images/{assetName}.xml");
        var sprite = new PositionedAnimatedSprite {
            Image = Get().Content.Load<Texture2D>($"Images/{assetName}"),
            AnimationData = EmptyAnimData,
            Position = new Vector2(x, y)
        };
        sprite.Regions = autoLoadAnims
            ? ReadAnimationData(out sprite.AnimationData)
            : new Dictionary<string, Rectangle>();
        sprite.AnimationFactory
            = new SpriteSheetAnimationFactory(new TextureAtlas(assetName, sprite.Image, sprite.Regions));
        if (!autoLoadAnims) return sprite;
        foreach (var anim in sprite.AnimationData) sprite.BuildAnimation(anim.Item1, anim.Item2, anim.Item3);
        sprite.BuildSprite();
        return sprite;
    }

    public static Dictionary<string, Rectangle> ReadAnimationData(out List<Tuple<string, int, int>> indices,
        string loadAnimName = null!) {
        var mainNode = Xml.SelectNodes("TextureAtlas");
        var lastAnim = "";
        indices = new List<Tuple<string, int, int>>();
        var startAnimAt = 0;
        var endAnimAt = -1;
        var atlasMap = new Dictionary<string, Rectangle>();
#pragma warning disable CS8602
#pragma warning disable CS8604
        foreach (XmlNode node in mainNode) {
            // Iterate over every node in TextureAtlas
            var subTextures = node.SelectNodes("SubTexture");
            foreach (XmlNode subTexture in subTextures) {
                // Iterate over every SubTexture found in TextureAtlas
                var attr = subTexture.Attributes;
                var name = attr.GetNamedItem("name").Value;
                var skipAnim = loadAnimName != null && loadAnimName != name[..^4];
                if (lastAnim == "") lastAnim = name[..^4]; // Animation name without frame counter
                if (lastAnim != name[..^4]) {
                    // We are at the next animation now
                    if (!skipAnim) indices.Add(Tuple.Create(lastAnim!, startAnimAt, endAnimAt));
                    startAnimAt = endAnimAt + 1;
                }

                if (!skipAnim) {
                    // Uses frame positions and boundaries to avoid "bouncing". Algorithm from flixel.graphics.frames.FlxAtlasFrames#fromSparrow
                    Rectangle size;
                    if (attr.GetNamedItem("frameX") != null)
                        size = new Rectangle(int.Parse(attr.GetNamedItem("frameX").Value),
                            int.Parse(attr.GetNamedItem("frameY").Value),
                            int.Parse(attr.GetNamedItem("frameWidth").Value),
                            int.Parse(attr.GetNamedItem("frameHeight").Value));
                    else
                        size = new Rectangle(0, 0,
                            int.Parse(attr.GetNamedItem("width").Value),
                            int.Parse(attr.GetNamedItem("height").Value));

                    atlasMap.Add(name, new Rectangle(int.Parse(attr.GetNamedItem("x").Value) + size.Left - 2,
                        int.Parse(attr.GetNamedItem("y").Value) + size.Top + 2, // Two-pixel offset to prevent bleeding
                        size.Width,
                        size.Height
                    ));
                }

                lastAnim = name[..^4];
                endAnimAt++;
            }
        }
#pragma warning restore CS8602
#pragma warning restore CS8604
        indices.Add(Tuple.Create(lastAnim, startAnimAt, endAnimAt)!); // Add the last animation
        return atlasMap;
    }
}
