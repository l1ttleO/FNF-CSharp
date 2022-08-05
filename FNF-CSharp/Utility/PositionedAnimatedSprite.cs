using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.TextureAtlases;

namespace FNF_CSharp.Utility;

public struct PositionedAnimatedSprite {
    public Texture2D Image;
    public Dictionary<string, Rectangle> Regions;
    public List<Tuple<string, int, int>> AnimationData;
    public SpriteSheetAnimationFactory AnimationFactory;
    public AnimatedSprite Sprite;
    public Vector2 Position;

    public void BuildAnimation(string name, int startIndex, int endIndex, int[]? indices = null) {
        // Generates arrays with indices, they start from 0 and don't reset
        int[] array;
        if (indices == null) {
            var arrayLength = MathHelper.Clamp(endIndex + 1 - startIndex, 1, endIndex);
            array = new int[arrayLength];
            var curIndices = startIndex;
            for (var i = 0; i < array.Length; ++i) array[i] = curIndices++;
        } else { array = indices; }

        AnimationFactory.Add(name,
            new SpriteSheetAnimationData(array,
                1 / 24f)); // Most animations run at 24FPS, TODO: support varying FPS
    }

    public void AddAnimation(string animName, string xmlName, int[]? frameIndices = null) {
        var newAnimRegions = FnfGame.ReadAnimationData(out var indices, xmlName);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        // Impossible to access 'this' fields in LINQ
        foreach (var x in newAnimRegions)
            if (!Regions.ContainsKey(x.Key))
                Regions.Add(x.Key, x.Value);
        AnimationFactory = new SpriteSheetAnimationFactory(new TextureAtlas(Image.Name, Image, Regions));
        BuildAnimation(animName, indices.First().Item2, indices.First().Item3, frameIndices);
    }

    public void BuildSprite() {
        Sprite = new AnimatedSprite(AnimationFactory);
    }
}
