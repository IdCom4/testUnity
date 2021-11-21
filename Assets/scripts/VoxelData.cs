using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 256;

    public static readonly int WorldSizeInChunks = 100;
    public static int WorldSizeInBlocks {
      get { return (WorldSizeInChunks * ChunkWidth); }
    }

    public static readonly int ViewDistanceInChunks = 5;

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize {
      get { return (1f / TextureAtlasSizeInBlocks); }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8] {
      new Vector3(0.0f, 0.0f, 0.0f),
      new Vector3(1.0f, 0.0f, 0.0f),
      new Vector3(1.0f, 1.0f, 0.0f),
      new Vector3(0.0f, 1.0f, 0.0f),
      new Vector3(0.0f, 0.0f, 1.0f),
      new Vector3(1.0f, 0.0f, 1.0f),
      new Vector3(1.0f, 1.0f, 1.0f),
      new Vector3(0.0f, 1.0f, 1.0f)
    };

    public static readonly Vector3[] faceChecks = new Vector3[6] {
      new Vector3(0.0f, 0.0f, -1.0f),
      new Vector3(0.0f, 0.0f, 1.0f),
      new Vector3(0.0f, 1.0f, 0.0f),
      new Vector3(0.0f, -1.0f, 0.0f),
      new Vector3(-1.0f, 0.0f, 0.0f),
      new Vector3(1.0f, 0.0f, 0.0f),
    };

    public static readonly int[,] voxelTris = new int[6,4] {
      { 0, 3, 1, 2 }, // Back face
      { 5, 6, 4, 7 }, // Front face
      { 3, 7, 2, 6 }, // Top face
      { 1, 5, 0, 4 }, // Bottom face
      { 4, 7, 0, 3 }, // Left face
      { 1, 2, 5, 6 }  // Right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4] {
      new Vector2(0.0f, 0.0f),
      new Vector2(0.0f, 1.0f),
      new Vector2(1.0f, 0.0f),
      new Vector2(1.0f, 1.0f)
    };

    public static readonly BiomeData biome = new BiomeData(
      "HILL",
      40, 40, 0.25f, // terrain height & scale
      1.3f, 0.6f, 15f, 0.8f, 12, 5, // trees
      new Lode("CAVE", BlockID.AIR, 0, VoxelData.ChunkHeight, 0.1f, 0.60f),
      new Lode[4] {
        new Lode("COAL", BlockID.COAL_ORE, 0, VoxelData.ChunkHeight, 0.6f, 0.60f),
        new Lode("IRON", BlockID.IRON_ORE, 0, VoxelData.ChunkHeight, 0.6f, 0.62f),
        new Lode("GOLD", BlockID.GOLD_ORE, 0, 40, 0.6f, 0.66f),
        new Lode("DIAMOND", BlockID.DIAMOND_ORE, 0, 12, 0.6f, 0.68f)
      },
      new Lode[2] {
        new Lode("DIRT", BlockID.DIRT, 0, VoxelData.ChunkHeight, 0.6f, 0.5f),
        new Lode("GRAVEL", BlockID.GRAVEL, 0, VoxelData.ChunkHeight, 0.6f, 0.55f)
      }
    );

    public static readonly BlockType[] blockTypes = new BlockType[17] {
      new BlockType("AIR", false, false, true, null),
      new BlockType("STONE", true, true, false, new byte[6] { 0, 0, 0, 0, 0, 0 }),
      new BlockType("GRASS", true, true, false, new byte[6] { 2, 2, 3, 1, 2, 2 }),
      new BlockType("DIRT", true, true, false, new byte[6] { 1, 1, 1, 1, 1, 1 }),
      new BlockType("BEDROCK", true, true, false, new byte[6] { 4, 4, 4, 4, 4, 4 }),
      new BlockType("FURNACE", true, true, false, new byte[6] { 5, 7, 6, 6, 5, 5 }),
      new BlockType("SAND", true, true, false, new byte[6] { 9, 9, 9, 9, 9, 9 }),
      new BlockType("GRAVEL", true, true, false, new byte[6] { 10, 10, 10, 10, 10, 10 }),
      new BlockType("COAL_ORE", true, true, false, new byte[6] { 11, 11, 11, 11, 11, 11 }),
      new BlockType("IRON_ORE", true, true, false, new byte[6] { 12, 12, 12, 12, 12, 12 }),
      new BlockType("GOLD_ORE", true, true, false, new byte[6] { 13, 13, 13, 13, 13, 13 }),
      new BlockType("DIAMOND_ORE", true, true, false, new byte[6] { 14, 14, 14, 14, 14, 14 }),
      new BlockType("GLASS", true, true, true, new byte[6] { 15, 15, 15, 15, 15, 15 }),
      new BlockType("COBBLESTONE", true, true, false, new byte[6] { 16, 16, 16, 16, 16, 16 }),
      new BlockType("WOOD", true, true, false, new byte[6] { 17, 17, 18, 18, 17, 17 }),
      new BlockType("LEAVES", true, true, true, new byte[6] { 19, 19, 19, 19, 19, 19 }),
      new BlockType("PLANK", true, true, false, new byte[6] { 20, 20, 20, 20, 20, 20 })
    };
}

public class BlockType {
  public readonly string name;
  public readonly bool isSolid;
  public readonly bool isVisible;
  public readonly bool isTransparent;

  public byte[] faceTextureID;

  public BlockType(string _name, bool _isSolid, bool _isVisible, bool _isTransparent, byte[] _faceTextureID) {
    name = _name;

    isSolid = _isSolid;
    isVisible = _isVisible;
    isTransparent = _isTransparent;
  
    if (_faceTextureID != null) {
        faceTextureID = new byte[6] {
        _faceTextureID[Face.BACK],
        _faceTextureID[Face.FRONT],
        _faceTextureID[Face.TOP],
        _faceTextureID[Face.BOTTOM],
        _faceTextureID[Face.LEFT],
        _faceTextureID[Face.RIGHT]
      };
    }
    
  } 

  // Back, Front, Top, Bottom, Left, Right
  public byte GetTextureID(int faceIndex) {
    switch (faceIndex) {
      case 0:
        return faceTextureID[Face.BACK];
      case 1:
        return faceTextureID[Face.FRONT];
      case 2:
        return faceTextureID[Face.TOP];
      case 3:
        return faceTextureID[Face.BOTTOM];
      case 4:
        return faceTextureID[Face.LEFT];
      case 5:
        return faceTextureID[Face.RIGHT];
      default:
        Debug.Log("Error in GetTextureID, invalid face index");
        return 0;
    }
  }
}

public static class Face {

  public static readonly int BACK = 0;
  public static readonly int FRONT = 1;
  public static readonly int TOP = 2;
  public static readonly int BOTTOM = 3;
  public static readonly int LEFT = 4;
  public static readonly int RIGHT = 5;

}

public static class BlockID {

  public static byte AIR = 0;
  public static byte STONE = 1;
  public static byte GRASS = 2;
  public static byte DIRT = 3;
  public static byte BEDROCK = 4;
  public static byte FURNACE = 5;
  public static byte SAND = 6;
  public static byte GRAVEL = 7;
  public static byte COAL_ORE = 8;
  public static byte IRON_ORE = 9;
  public static byte GOLD_ORE = 10;
  public static byte DIAMOND_ORE = 11;
  public static byte GLASS = 12;
  public static byte COBBLESTONE = 13;
  public static byte WOOD = 14;
  public static byte LEAVES = 15;
  public static byte PLANK = 16;
  
}
