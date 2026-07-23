using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>Creates a repeatable 2D Animation skeleton and weighted grid for a single A-pose PNG.</summary>
public static class ExplorationSkinnedSpriteConfigurator
{
    public enum RigProfile
    {
        Default,
        Suit
    }

    public enum InfluenceRegion
    {
        Head,
        Torso,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    public readonly struct AlphaBounds
    {
        public readonly float MinX;
        public readonly float MaxX;
        public readonly float MinY;
        public readonly float MaxY;

        public AlphaBounds(float minX, float maxX, float minY, float maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }

        public Vector2 Min => new Vector2(MinX, MinY);
        public Vector2 Size => new Vector2(MaxX - MinX, MaxY - MinY);
    }

    public readonly struct RigBoneDefinition
    {
        public readonly string Name;
        public readonly int ParentIndex;
        public readonly Vector2 Position;

        public RigBoneDefinition(string name, int parentIndex, Vector2 position)
        {
            Name = name;
            ParentIndex = parentIndex;
            Position = position;
        }
    }

    public readonly struct SkinnedMeshData
    {
        public readonly Vertex2DMetaData[] Vertices;
        public readonly int[] Indices;
        public readonly Vector2Int[] Edges;

        public SkinnedMeshData(Vertex2DMetaData[] vertices, int[] indices, Vector2Int[] edges)
        {
            Vertices = vertices;
            Indices = indices;
            Edges = edges;
        }
    }

    public const int ReferenceWidth = 1024;
    public const int ReferenceHeight = 1536;
    public const int BoneCount = 14;
    public const int DefaultGridColumns = 16;
    public const int DefaultGridRows = 24;

    private static readonly string[] BoneNames =
    {
        "Root", "Hips", "Torso", "Head",
        "LeftUpperArm", "LeftForearm", "RightUpperArm", "RightForearm",
        "LeftThigh", "LeftCalf", "LeftFoot", "RightThigh", "RightCalf", "RightFoot"
    };

    private static readonly int[] ParentIndices =
    {
        -1, 0, 1, 2,
        2, 4, 2, 6,
        1, 8, 9, 1, 11, 12
    };

    public static IReadOnlyList<string> OrderedBoneNames => BoneNames;

    /// <summary>Configures a PNG as a Full Rect single Sprite, persists its rig data, reimports it, and returns the Sprite.</summary>
    public static Sprite Configure(string pngAssetPath, RigProfile profile, float pixelsPerUnit = 100f)
    {
        if (string.IsNullOrWhiteSpace(pngAssetPath))
            throw new ArgumentException("A PNG asset path is required.", nameof(pngAssetPath));
        if (pixelsPerUnit <= 0f || !float.IsFinite(pixelsPerUnit))
            throw new ArgumentOutOfRangeException(nameof(pixelsPerUnit));

        var importer = AssetImporter.GetAtPath(pngAssetPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"No TextureImporter exists at '{pngAssetPath}'.");

        ConfigureImporter(importer, pixelsPerUnit);
        importer.SaveAndReimport();

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngAssetPath);
        if (texture == null)
            throw new InvalidOperationException($"Could not load texture at '{pngAssetPath}' after import.");

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (dataProvider == null)
            throw new InvalidOperationException($"Sprite data providers are unavailable for '{pngAssetPath}'.");

        dataProvider.InitSpriteEditorDataProvider();
        var spriteRect = dataProvider.GetSpriteRects().SingleOrDefault();
        if (spriteRect == null)
            throw new InvalidOperationException($"'{pngAssetPath}' must import exactly one Sprite.");

        var boneProvider = dataProvider.GetDataProvider<ISpriteBoneDataProvider>();
        var meshProvider = dataProvider.GetDataProvider<ISpriteMeshDataProvider>();
        if (boneProvider == null || meshProvider == null)
            throw new InvalidOperationException("The current TextureImporter does not expose 2D Animation bone and mesh data providers.");

        boneProvider.SetBones(spriteRect.spriteID, CreateSpriteBones(profile, texture.width, texture.height));
        var mesh = CreateMeshData(profile, texture.width, texture.height);
        meshProvider.SetVertices(spriteRect.spriteID, mesh.Vertices);
        meshProvider.SetIndices(spriteRect.spriteID, mesh.Indices);
        meshProvider.SetEdges(spriteRect.spriteID, mesh.Edges);
        dataProvider.Apply();
        importer.SaveAndReimport();

        return AssetDatabase.LoadAllAssetsAtPath(pngAssetPath).OfType<Sprite>().SingleOrDefault();
    }

    public static void ConfigureImporter(TextureImporter importer, float pixelsPerUnit)
    {
        if (importer == null)
            throw new ArgumentNullException(nameof(importer));

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        // Set the direct importer properties last: SetTextureSettings can restore an older texture type.
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
    }

    public static AlphaBounds GetAlphaBounds(RigProfile profile, int width = ReferenceWidth, int height = ReferenceHeight)
    {
        var referenceBounds = profile == RigProfile.Suit
            ? new AlphaBounds(326f, 749f, 202f, 1456f)
            : new AlphaBounds(296f, 767f, 159f, 1457f);
        return new AlphaBounds(
            referenceBounds.MinX * width / ReferenceWidth,
            referenceBounds.MaxX * width / ReferenceWidth,
            referenceBounds.MinY * height / ReferenceHeight,
            referenceBounds.MaxY * height / ReferenceHeight);
    }

    public static RigBoneDefinition[] CreateRigDefinition(RigProfile profile, int width, int height)
    {
        ValidateDimensions(width, height);
        var bounds = GetAlphaBounds(profile, width, height);
        var normalizedPositions = new[]
        {
            new Vector2(0.50f, 0.00f), // Root: floor center.
            new Vector2(0.50f, 0.30f), // Hips.
            new Vector2(0.50f, 0.59f), // Torso.
            new Vector2(0.50f, 0.88f), // Head.
            new Vector2(0.27f, 0.71f), new Vector2(0.12f, 0.54f),
            new Vector2(0.73f, 0.71f), new Vector2(0.88f, 0.54f),
            new Vector2(0.39f, 0.29f), new Vector2(0.36f, 0.14f), new Vector2(0.29f, 0.04f),
            new Vector2(0.61f, 0.29f), new Vector2(0.64f, 0.14f), new Vector2(0.71f, 0.04f)
        };

        var definitions = new RigBoneDefinition[BoneCount];
        for (var i = 0; i < BoneCount; i++)
        {
            var position = bounds.Min + Vector2.Scale(bounds.Size, normalizedPositions[i]);
            definitions[i] = new RigBoneDefinition(BoneNames[i], ParentIndices[i], position);
        }

        return definitions;
    }

    public static List<SpriteBone> CreateSpriteBones(RigProfile profile, int width, int height)
    {
        var definition = CreateRigDefinition(profile, width, height);
        var bones = new List<SpriteBone>(BoneCount);
        for (var i = 0; i < definition.Length; i++)
        {
            var parentIndex = definition[i].ParentIndex;
            var localPosition = parentIndex < 0
                ? definition[i].Position
                : definition[i].Position - definition[parentIndex].Position;
            bones.Add(new SpriteBone
            {
                name = definition[i].Name,
                parentId = parentIndex,
                position = localPosition,
                rotation = Quaternion.identity,
                length = GetBoneLength(i, definition),
                color = Color.white,
                guid = Hash128.Compute($"MoshiReRe.ExplorationSkinnedSprite.{definition[i].Name}").ToString()
            });
        }

        return bones;
    }

    public static SkinnedMeshData CreateMeshData(RigProfile profile, int width, int height, int columns = DefaultGridColumns, int rows = DefaultGridRows)
    {
        ValidateDimensions(width, height);
        if (columns < 1 || rows < 1)
            throw new ArgumentOutOfRangeException(nameof(columns), "The grid requires at least one cell in each axis.");

        var vertices = new Vertex2DMetaData[(columns + 1) * (rows + 1)];
        for (var y = 0; y <= rows; y++)
        {
            for (var x = 0; x <= columns; x++)
            {
                var position = new Vector2(width * x / (float)columns, height * y / (float)rows);
                vertices[y * (columns + 1) + x] = new Vertex2DMetaData
                {
                    position = position,
                    boneWeight = CreateBoneWeight(profile, position, width, height)
                };
            }
        }

        var indices = new int[columns * rows * 6];
        var triangle = 0;
        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < columns; x++)
            {
                var lowerLeft = y * (columns + 1) + x;
                var lowerRight = lowerLeft + 1;
                var upperLeft = lowerLeft + columns + 1;
                var upperRight = upperLeft + 1;
                indices[triangle++] = lowerLeft;
                indices[triangle++] = upperLeft;
                indices[triangle++] = upperRight;
                indices[triangle++] = lowerLeft;
                indices[triangle++] = upperRight;
                indices[triangle++] = lowerRight;
            }
        }

        var edges = new List<Vector2Int>(columns * (rows + 1) + rows * (columns + 1));
        for (var y = 0; y <= rows; y++)
            for (var x = 0; x < columns; x++)
                edges.Add(new Vector2Int(y * (columns + 1) + x, y * (columns + 1) + x + 1));
        for (var y = 0; y < rows; y++)
            for (var x = 0; x <= columns; x++)
                edges.Add(new Vector2Int(y * (columns + 1) + x, (y + 1) * (columns + 1) + x));

        return new SkinnedMeshData(vertices, indices, edges.ToArray());
    }

    public static InfluenceRegion GetInfluenceRegion(RigProfile profile, Vector2 position, int width, int height)
    {
        var bounds = GetAlphaBounds(profile, width, height);
        var normalized = new Vector2(
            Mathf.InverseLerp(bounds.MinX, bounds.MaxX, position.x),
            Mathf.InverseLerp(bounds.MinY, bounds.MaxY, position.y));

        if (normalized.y > 0.81f)
            return InfluenceRegion.Head;
        if (normalized.y < 0.39f)
            return normalized.x < 0.5f ? InfluenceRegion.LeftLeg : InfluenceRegion.RightLeg;
        if (normalized.y >= 0.47f && normalized.y <= 0.80f && normalized.x < 0.31f)
            return InfluenceRegion.LeftArm;
        if (normalized.y >= 0.47f && normalized.y <= 0.80f && normalized.x > 0.69f)
            return InfluenceRegion.RightArm;
        return InfluenceRegion.Torso;
    }

    public static bool HasValidBoneWeight(BoneWeight weight)
    {
        var values = new[] { weight.weight0, weight.weight1, weight.weight2, weight.weight3 };
        var sum = 0f;
        foreach (var value in values)
        {
            if (!float.IsFinite(value) || value < 0f)
                return false;
            sum += value;
        }

        return Mathf.Abs(sum - 1f) <= 0.0001f;
    }

    private static BoneWeight CreateBoneWeight(RigProfile profile, Vector2 position, int width, int height)
    {
        var bounds = GetAlphaBounds(profile, width, height);
        var normalized = new Vector2(
            Mathf.InverseLerp(bounds.MinX, bounds.MaxX, position.x),
            Mathf.InverseLerp(bounds.MinY, bounds.MaxY, position.y));
        var region = GetInfluenceRegion(profile, position, width, height);

        switch (region)
        {
            case InfluenceRegion.Head:
                return Blend(3, 2, Mathf.InverseLerp(0.81f, 0.86f, normalized.y));
            case InfluenceRegion.LeftArm:
                return Blend(4, 5, Mathf.InverseLerp(0.68f, 0.55f, normalized.y));
            case InfluenceRegion.RightArm:
                return Blend(6, 7, Mathf.InverseLerp(0.68f, 0.55f, normalized.y));
            case InfluenceRegion.LeftLeg:
                return CreateLegWeight(normalized, 8, 9, 10);
            case InfluenceRegion.RightLeg:
                return CreateLegWeight(normalized, 11, 12, 13);
            default:
                return Blend(2, 1, Mathf.InverseLerp(0.39f, 0.48f, normalized.y));
        }
    }

    private static BoneWeight CreateLegWeight(Vector2 normalized, int thighIndex, int calfIndex, int footIndex)
    {
        if (normalized.y < 0.08f)
            return Blend(footIndex, calfIndex, Mathf.InverseLerp(0.08f, 0.02f, normalized.y));
        if (normalized.y < 0.26f)
            return Blend(calfIndex, thighIndex, Mathf.InverseLerp(0.26f, 0.14f, normalized.y));
        return Blend(thighIndex, 1, Mathf.InverseLerp(0.39f, 0.31f, normalized.y));
    }

    private static BoneWeight Blend(int firstBone, int secondBone, float secondWeight)
    {
        secondWeight = Mathf.Clamp01(secondWeight);
        return new BoneWeight
        {
            boneIndex0 = firstBone,
            weight0 = 1f - secondWeight,
            boneIndex1 = secondBone,
            weight1 = secondWeight,
            boneIndex2 = 0,
            weight2 = 0f,
            boneIndex3 = 0,
            weight3 = 0f
        };
    }

    private static float GetBoneLength(int boneIndex, IReadOnlyList<RigBoneDefinition> definition)
    {
        for (var i = 0; i < definition.Count; i++)
            if (definition[i].ParentIndex == boneIndex)
                return Vector2.Distance(definition[i].Position, definition[boneIndex].Position);

        return 1f;
    }

    private static void ValidateDimensions(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Texture dimensions must be positive.");
    }
}
