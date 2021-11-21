using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

  [SerializeField] public int seed;

  [SerializeField] public GameObject debugScreen;
  [SerializeField] public GameObject creativeInventory;
  [SerializeField] public GameObject cursorSlot;


  public Material blocksAtlas;
  public Material transparentBlocksAtlas;


  public Sprite[] itemSprites;

  public Transform player;
  public Vector3 spawnPoint;
  public ChunkCoords playerLastChunk;
  public ChunkCoords playerCurrentChunk;

  Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
  List<ChunkCoords> activeChunks = new List<ChunkCoords>();
  List<Chunk> chunksToUpdate = new List<Chunk>();
  List<ChunkCoords> chunksToCreate = new List<ChunkCoords>();
  public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
  bool applyingModifications = false;

  Queue<Queue<BlockMod>> modifications = new Queue<Queue<BlockMod>>();

  private bool _inUI = false;
  public bool inUI {
    get { return _inUI; }
    set {
      _inUI = value;
      if (_inUI) {
        Cursor.lockState = CursorLockMode.None;

        creativeInventory.SetActive(true);
        cursorSlot.SetActive(true);
      }
      else {
        Cursor.lockState = CursorLockMode.Locked;
    
        creativeInventory.SetActive(false);
        cursorSlot.SetActive(false);
      }
    }
  }


  private void Start() {
    
    Random.InitState(seed);

    GenerateWorld();
    spawnPoint = new Vector3(VoxelData.WorldSizeInBlocks / 2, 60f, VoxelData.WorldSizeInBlocks / 2);
    /*spawnPoint = new Vector3(VoxelData.WorldSizeInBlocks / 2, 0, VoxelData.WorldSizeInBlocks / 2);
    ChunkCoords coords = GetChunkCoordsFromVector3(new Vector3(spawnPoint.x, 0, spawnPoint.z));
    int y = VoxelData.ChunkHeight - 1;
    while (y > 0) {
      byte blockID = chunks[coords.x, coords.z].blocks[VoxelData.ChunkWidth / 2, VoxelData.ChunkWidth / 2, y];
      if (blockID != BlockID.AIR) break;
      y--;
    }
    spawnPoint.y = y + 4;*/
    player.position = spawnPoint;
    
    debugScreen.SetActive(false);
  }

  private void Update() {
    playerCurrentChunk = GetChunkCoordsFromVector3(player.position);

    if (playerCurrentChunk == null)
      playerCurrentChunk = playerLastChunk;

    if (!playerCurrentChunk.Equals(playerLastChunk)) {
      CheckViewDistance();
      playerLastChunk = playerCurrentChunk;
    }

    if (!applyingModifications)
      ApplyModifications();

    if (chunksToCreate.Count > 0)
      CreateChunk();
    
    if (chunksToUpdate.Count > 0)
      UpdateChunks();

    if (chunksToDraw.Count > 0) {
      lock (chunksToDraw) {
        if (chunksToDraw.Peek().isEditable)
          chunksToDraw.Dequeue().CreateMesh();
      }
    }

    GetDebugInputs();
  }

  private void GetDebugInputs() {
    if (Input.GetKeyDown(KeyCode.F3))
      debugScreen.SetActive(!debugScreen.activeSelf);
  }

  void GenerateWorld() {
    for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++) {
      for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++) {
        ChunkCoords coords = new ChunkCoords(x, z);
        chunks[x, z] = new Chunk(coords, this, true);
        activeChunks.Add(coords);
      }
    }

    /*while (modifications.Count > 0) {
  
      BlockMod bm = modifications.Dequeue();
      ChunkCoords cc = GetChunkCoordsFromVector3(bm.position);

      if (chunks[cc.x, cc.z] == null) {
        chunks[cc.x, cc.z] = new Chunk(cc, this, true);
        activeChunks.Add(cc);
      }

      chunks[cc.x, cc.z].modifications.Enqueue(bm);

      if (!chunksToUpdate.Contains(chunks[cc.x, cc.z]))
        chunksToUpdate.Add(chunks[cc.x, cc.z]);

    }

    for (int i = 0; i < chunksToUpdate.Count; i++) {
      chunksToUpdate[0].UpdateChunk();
      chunksToUpdate.RemoveAt(0);
    } */
  }

  void CreateChunk() {

    ChunkCoords c = chunksToCreate[0];
    chunksToCreate.RemoveAt(0);
    activeChunks.Add(c);
    chunks[c.x, c.z].Init();
  }

  void UpdateChunks() {
    bool updated = false;
    int index = 0;

    while(!updated && index < chunksToUpdate.Count) {
      if (chunksToUpdate[index].isEditable) {
        chunksToUpdate[index].UpdateChunk();
        chunksToUpdate.RemoveAt(index);
        updated = true;
      } else
        index++;
    }
  }

  void ApplyModifications() {
    applyingModifications = true;

    while (modifications.Count > 0) {

      Queue<BlockMod> queue = modifications.Dequeue();

      while (queue.Count > 0) {
        BlockMod bm = queue.Dequeue();
        ChunkCoords c = GetChunkCoordsFromVector3(bm.position);

        if (chunks[c.x, c.z] == null) {
          chunks[c.x, c.z] = new Chunk(c, this, true);
          activeChunks.Add(c);
        }

        chunks[c.x, c.z].modifications.Enqueue(bm);

        if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
          chunksToUpdate.Add(chunks[c.x, c.z]);
        }
    }

    applyingModifications = false;
  }

  void CheckViewDistance() {

    // unactive all out of bounds chunks
    for (int i = 0; i < activeChunks.Count; i++) {
      if (
          activeChunks[i].x < playerCurrentChunk.x - VoxelData.ViewDistanceInChunks || activeChunks[i].x >= playerCurrentChunk.x + VoxelData.ViewDistanceInChunks ||
          activeChunks[i].z < playerCurrentChunk.z - VoxelData.ViewDistanceInChunks || activeChunks[i].z >= playerCurrentChunk.z + VoxelData.ViewDistanceInChunks
        ) {
          chunks[activeChunks[i].x, activeChunks[i].z].isActive = false;
          activeChunks.RemoveAt(i);
          i--;
        }
    }

    // active or create all in bounds chunks
    for (int x = playerCurrentChunk.x - VoxelData.ViewDistanceInChunks; x < playerCurrentChunk.x + VoxelData.ViewDistanceInChunks; x++) {
      for (int z = playerCurrentChunk.z - VoxelData.ViewDistanceInChunks; z < playerCurrentChunk.z + VoxelData.ViewDistanceInChunks; z++) {
        ChunkCoords coords = new ChunkCoords(x, z);

        if (IsChunkInWorld(coords)) {
          if (chunks[x, z] == null) {
            chunks[x, z] = new Chunk(coords, this, false);
            chunksToCreate.Add(coords);
          } else if (!chunks[x, z].isActive) {
            chunks[x, z].isActive = true;
          }
          activeChunks.Add(coords);
        }
      }
    }
  }

  public ChunkCoords GetChunkCoordsFromVector3(Vector3 pos) {
    int x = (int)Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
    int z = (int)Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

    ChunkCoords coords = new ChunkCoords(x, z);
    if (IsChunkInWorld(coords))
      return coords;
    else
      return null;
  }

  public Chunk GetChunkFromVector3(Vector3 pos) {
    int x = (int)Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
    int z = (int)Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

    ChunkCoords coords = new ChunkCoords(x, z);
    if (IsChunkInWorld(coords))
      return (chunks[x, z]);
    else
      return null;
  }

  public byte CheckBlockIdAt(Vector3 pos) {

    ChunkCoords chunk = new ChunkCoords(pos);

    if (!IsBlockInWorld(pos))
      return BlockID.AIR;

    if (chunks[chunk.x, chunk.z] != null && chunks[chunk.x, chunk.z].isEditable)
      return chunks[chunk.x, chunk.z].GetBlockFromGlobalVector3(pos);

    return GetBlockAt(pos);
  }

  public BlockType CheckBlockTypeAt(Vector3 pos) {

    ChunkCoords chunk = new ChunkCoords(pos);

    if (!IsBlockInWorld(pos))
      return null;

    if (chunks[chunk.x, chunk.z] != null && chunks[chunk.x, chunk.z].isEditable)
      return VoxelData.blockTypes[chunks[chunk.x, chunk.z].GetBlockFromGlobalVector3(pos)];

    return VoxelData.blockTypes[GetBlockAt(pos)];
  }

  public byte GetBlockAt(Vector3 pos) {

    int y = Mathf.FloorToInt(pos.y);

    /* IMMUTABLE PATH */

    // if out of world return air
    if (!IsBlockInWorld(pos)) return (BlockID.AIR);

    // if bottom of the chunk return bedrock
    if (y == 0) return (BlockID.BEDROCK);

    /* BASIC TERRAIN PATH */

    int terrainHeight = Mathf.FloorToInt(VoxelData.biome.terrainMaxHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), seed, VoxelData.biome.terrainScale)) + VoxelData.biome.terrainMinHeight;
    
    byte blockID = BlockID.AIR;
    
    if (y > terrainHeight) return (BlockID.AIR);
    else if (y == terrainHeight) blockID = BlockID.GRASS; 
    else if (y > terrainHeight - 5) blockID = BlockID.DIRT;
    else blockID = BlockID.STONE;

    /* SECOND PATH */

    int offset = 0;

    if (blockID == BlockID.STONE) {

      // terrain
      foreach (Lode lode in VoxelData.biome.terrainLodes) {
        offset += 50;
        if (y >= lode.minHeight && y <= lode.maxHeight) {
          if (Noise.Get3DPerlin(pos, offset, lode.scale, lode.threshold))
            blockID = lode.blockID;
        }
      }

      // ressources
      foreach (Lode lode in VoxelData.biome.ressourceLodes) {
        offset += 50;
        if (y >= lode.minHeight && y <= lode.maxHeight) {
          if (Noise.Get3DPerlin(pos, offset, lode.scale, lode.threshold))
            blockID = lode.blockID;
        }
      }
    }
    
    // caves
    if (blockID != BlockID.AIR) {
      Lode caveLode = VoxelData.biome.caveLode;

      if (y >= caveLode.minHeight && y <= caveLode.maxHeight) {
          if (Noise.Get3DPerlin(pos, offset, caveLode.scale, caveLode.threshold))
            blockID = caveLode.blockID;
        }
    }

    /* TREE PATH */
    if (y == terrainHeight) {
      if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), seed, VoxelData.biome.treeZoneScale) > VoxelData.biome.treeZoneThreshold) {
        if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), seed / 2, VoxelData.biome.treePlacementScale) > VoxelData.biome.treePlacementThreshold) {
          modifications.Enqueue(Structure.MakeTree(pos, VoxelData.biome.minTreeHeight, VoxelData.biome.maxTreeHeight));
        }
      }
    }

    return (blockID);
  }

  bool IsChunkInWorld(ChunkCoords coords) {
    if (coords.x >= 0 && coords.x < VoxelData.WorldSizeInChunks && coords.z >= 0 && coords.z < VoxelData.WorldSizeInChunks)
      return true;
    else
      return false;
  }

  bool IsBlockInWorld(Vector3 pos) {
    if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInBlocks && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInBlocks)
      return true;
    else
      return false;
  }
}

public class BlockMod {
  public Vector3 position;
  public byte id;

  public BlockMod() {
    position = new Vector3();
    id = BlockID.AIR;
  }

  public BlockMod(Vector3 _pos, byte _id) {
    position = _pos;
    id = _id;
  }
}