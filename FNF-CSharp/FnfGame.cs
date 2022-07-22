using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using AnimatedSprite = MonoGame.Extended.Animations.AnimatedSprite;
using SpriteSheetAnimationData = MonoGame.Extended.Animations.SpriteSheets.SpriteSheetAnimationData;

namespace FNF_CSharp;

[SuppressMessage("ReSharper", "StringLiteralTypo")] // Asset and animation names
public class FnfGame : Game {
    public static FnfGame ThisInstance = null!;
    private readonly GraphicsDeviceManager _graphics;
    public readonly KeyboardListener Keyboard = new();
    private SpriteFont _font = null!;
    private TimeSpan _lastUpdateTime;
    private SpriteBatch _sprites = null!;
    public State CurrentState;

    public FnfGame() {
        ThisInstance = this;
        CurrentState = new TitleState();

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
        CurrentState.LoadContent();

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

        Components.Add(new InputListenerComponent(this, Keyboard));
        Keyboard.KeyPressed += OnKeyPress;
        Keyboard.KeyPressed += CurrentState.OnKeyPress;

        base.Initialize();
    }

    protected override void Draw(GameTime gameTime) {
        _graphics.GraphicsDevice.Clear(Color.Black);

        _sprites.Begin();
        foreach (var (sprite, vector) in CurrentState.InWorld) {
            var scaledVector = new Vector2(vector.X * (Window.ClientBounds.Width / 1280f),
                vector.Y * (Window.ClientBounds.Height /
                            720f)); // Scale the position because it depends on the window's bounds
            _sprites.Draw(sprite, scaledVector);
        }

        // Debug information
        _sprites.DrawString(_font,
            $"Draw FPS:{Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 2).ToString(CultureInfo.InvariantCulture)}",
            new Vector2(10, 10), Color.White);
        _sprites.DrawString(_font,
            $"Update FPS:{Math.Round(1 / _lastUpdateTime.TotalSeconds, 2).ToString(CultureInfo.InvariantCulture)}",
            new Vector2(10, 25), Color.White);
        _sprites.DrawString(_font,
            $"Allocated:{(GC.GetTotalAllocatedBytes(true) / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)}MB",
            new Vector2(10, 40), Color.White);
        _sprites.End();

        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime) {
        foreach (var spriteData in CurrentState.InWorld) spriteData.Item1.Update(gameTime);

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
        if (ThisInstance == null) throw new ArgumentNullException(); // Should never happen
        return ThisInstance;
    }

    public static void SwitchState(State newState) {
        var game = Get();
        game.CurrentState.UnloadContent();
        game.Keyboard.KeyPressed -= game.CurrentState.OnKeyPress;
        newState.LoadContent();
        game.Keyboard.KeyPressed += newState.OnKeyPress;
        game.CurrentState = newState;
    }

    public static Tuple<AnimatedSprite, Vector2> LoadTextureAtlas(string assetName, int x, int y) {
        var textureFile = Get().Content.Load<Texture2D>($"Images/{assetName}");

        var xml = new XmlDocument();
        xml.Load($"Content/Images/{assetName}.xml");
        var mainNode = xml.SelectNodes("TextureAtlas");
        var atlasMap = new Dictionary<string, Rectangle>();
        var startAnimAt = 0;
        var endAnimAt = -1;
        var lastAnim = "";
        var animData =
            new List<Tuple<string, int, int>>(); // Animation name, animation start index, animation end index

#pragma warning disable CS8602
#pragma warning disable CS8604
        foreach (XmlNode node in mainNode) { // Iterate over every node in TextureAtlas
            var subTextures = node.SelectNodes("SubTexture");
            foreach (XmlNode subTexture in subTextures) { // Iterate over every SubTexture found in TextureAtlas
                var attr = subTexture.Attributes;
                var name = attr.GetNamedItem("name").Value;
                if (lastAnim == "") lastAnim = name[..^4]; // Animation name without frame counter
                if (lastAnim != name[..^4]) { // We are at the next animation now
                    animData.Add(Tuple.Create(lastAnim!, startAnimAt, endAnimAt));
                    startAnimAt = endAnimAt + 1;
                }

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
                lastAnim = name[..^4];
                endAnimAt++;
            }
        }
#pragma warning restore CS8602
#pragma warning restore CS8604
        animData.Add(Tuple.Create(lastAnim, startAnimAt, endAnimAt)); // Add the last animation
        var animFactory = new SpriteSheetAnimationFactory(new TextureAtlas(assetName, textureFile, atlasMap));
        // Generates arrays with indices, they start from 1 and don't reset
        foreach (var anim in animData) {
            var arrayLength = MathHelper.Clamp(anim.Item3 + 1 - anim.Item2, 1, anim.Item3);
            var array = new int[arrayLength];
            var curIndices = anim.Item2;
            for (var i = 0; i < array.Length; ++i) array[i] = curIndices++;
            animFactory.Add(anim.Item1,
                new SpriteSheetAnimationData(array,
                    1 / 24f)); // Most animations run at 24FPS, TODO: support varying FPS
        }

        return Tuple.Create(new AnimatedSprite(animFactory), new Vector2(x, y));
    }
}
