using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ChunkCoords {
  public readonly int x;
  public readonly int z;

  public ChunkCoords() {
    x = 0;
    z = 0;
  }

  public ChunkCoords(int _x, int _z) {
    x = _x;
    z = _z;
  }

  public ChunkCoords(Vector3 pos) {
    int xCheck = Mathf.FloorToInt(pos.x);
    int zCheck = Mathf.FloorToInt(pos.z);

    x = xCheck / VoxelData.ChunkWidth;
    z = zCheck / VoxelData.ChunkWidth;
  }

  public bool Equals(ChunkCoords other) {
    if (other == null)
      return false;
    else if (other.x == x && other.z == z)
      return true;
    else
      return false;
  }
}

public class Chunk {

  GameObject chunkObject;
  MeshRenderer meshRenderer;
  MeshFilter meshFilter;
  
  World world;
  public ChunkCoords coordinates;
  public byte[,,] blocks;
  public Queue<BlockMod> modifications = new Queue<BlockMod>();

  int vertexIndex = 0;
  List<Vector3> vertices = new List<Vector3>();
  List<int> triangles = new List<int>();
  List<int> transparentTriangles = new List<int>();
  List<Vector2> uvs = new List<Vector2>();

  private bool _isActive;
  private bool areBlocksGenerated = false;
  private bool threadLocked = false;

  public bool isActive {
    get { return _isActive; }
    set {
      _isActive = value;
      if (chunkObject != null)
        chunkObject.SetActive(value);
    }
  }

  public bool isEditable {
    get {
      if (!areBlocksGenerated || threadLocked)
        return false;
      else
        return true;
    }
  }

  public Vector3 position;

  public Chunk(ChunkCoords coords, World _world, bool generateOnLoad) {
    coordinates = coords;
    world = _world;
    _isActive = true;

    if (generateOnLoad) Init();
  }

  public void Init() {
    chunkObject = new GameObject();
    meshRenderer = chunkObject.AddComponent<MeshRenderer>();
    meshFilter = chunkObject.AddComponent<MeshFilter>();

    meshRenderer.materials = new Material[2] { world.blocksAtlas, world.transparentBlocksAtlas };
    
    chunkObject.transform.SetParent(world.transform);
    chunkObject.transform.position = new Vector3(coordinates.x * VoxelData.ChunkWidth, 0F, coordinates.z * VoxelData.ChunkWidth);
    chunkObject.name = "chunk " + coordinates.x + "," + coordinates.z;

    position = chunkObject.transform.position;
    if (!isActive) chunkObject.SetActive(false);

    Thread thread = new Thread(new ThreadStart(GenerateChunk));
    thread.Start();
  }

  void GenerateChunk() {
    blocks = new byte[VoxelData.ChunkWidth, VoxelData.ChunkWidth, VoxelData.ChunkHeight];

    for (int x = 0; x < VoxelData.ChunkWidth; x++) {
      for (int z = 0; z < VoxelData.ChunkWidth; z++) {
        for (int y = 0; y < VoxelData.ChunkHeight; y++) {
          blocks[x, z, y] = world.GetBlockAt(new Vector3(x, y, z) + position);
        }
      }
    }

    _updateChunk();

    areBlocksGenerated = true;
  }

  public void UpdateChunk() {

    Thread thread = new Thread(new ThreadStart(_updateChunk));
    
    thread.Start();
  }

  private void _updateChunk() {

    threadLocked = true;

    while(modifications.Count > 0) {
      BlockMod bm = modifications.Dequeue();

      Vector3 pos = bm.position - position;

      blocks[(int)pos.x, (int)pos.z, (int)pos.y] = bm.id;
    }

    ClearMeshData();

    for (int x = 0; x < VoxelData.ChunkWidth; x++) {
      for (int z = 0; z < VoxelData.ChunkWidth; z++) {
        for (int y = 0; y < VoxelData.ChunkHeight; y++) {
          if (VoxelData.blockTypes[blocks[x, z, y]].isVisible)
            UpdateBlockMeshData(new Vector3((float)x, (float)y, (float)z));
        }
      }
    }
    
    lock (world.chunksToDraw) {
      world.chunksToDraw.Enqueue(this);
    }

    threadLocked = false;
    
  }

  void ClearMeshData() {
    vertexIndex = 0;
    vertices.Clear();
    triangles.Clear();
    transparentTriangles.Clear();
    uvs.Clear();
  }

  bool isBlockInChunk(int x, int y, int z) {
    if (x >= VoxelData.ChunkWidth || x < 0 || z >= VoxelData.ChunkWidth || z < 0 || y >= VoxelData.ChunkHeight || y < 0)
      return false;
    else
      return true;
  }

  public byte GetBlockFromGlobalVector3(Vector3 pos) {
    
    int xCheck = Mathf.FloorToInt(pos.x);
    int zCheck = Mathf.FloorToInt(pos.z);
    int yCheck = Mathf.FloorToInt(pos.y);

    xCheck -= (int)Mathf.Round(position.x);
    zCheck -= (int)Mathf.Round(position.z);

    return blocks[xCheck, zCheck, yCheck];
  }

  public void EditBlock(Vector3 pos, byte newBlockID) {

    if (pos.y >= VoxelData.ChunkHeight) return ;

    int xCheck = Mathf.FloorToInt(pos.x);
    int zCheck = Mathf.FloorToInt(pos.z);
    int yCheck = Mathf.FloorToInt(pos.y);

    xCheck -= (int)Mathf.Round(position.x);
    zCheck -= (int)Mathf.Round(position.z);

    blocks[xCheck, zCheck, yCheck] = newBlockID;

    UpdateChunk();

    UpdateSurroundingBlocks(xCheck, yCheck, zCheck);

  }

  void UpdateSurroundingBlocks(int x, int y, int z) {
    Vector3 baseBlock = new Vector3(x, y, z);

    for (int i = 0; i < 6; i++) {
      Vector3 currentBlock = baseBlock + VoxelData.faceChecks[i];

      if (!isBlockInChunk((int)currentBlock.x, (int)currentBlock.y, (int)currentBlock.z)) {
        Chunk surroundingChunk =  world.GetChunkFromVector3(currentBlock + position);
        if (surroundingChunk != null)
          surroundingChunk.UpdateChunk();
      }
    }
  }

  // CheckIfBlockAt
  bool CheckIfOpaqueBlockAt(Vector3 pos) {
    int x = (int)Mathf.Round(pos.x);
    int z = (int)Mathf.Round(pos.z);
    int y = (int)Mathf.Round(pos.y);

    if (!isBlockInChunk(x, y, z)) {
      BlockType bt = world.CheckBlockTypeAt(pos + position);
      if (bt == null) return false;
      else return !bt.isTransparent;
    }
    else
      return (!VoxelData.blockTypes[blocks[x, z, y]].isTransparent);
  }

  void UpdateBlockMeshData(Vector3 pos) {
    byte blockID = blocks[(int)pos.x, (int)pos.z, (int)pos.y];
    bool isTransparent = VoxelData.blockTypes[blockID].isTransparent;

    for (int i = 0; i < 6; i++) {
      if (!CheckIfOpaqueBlockAt(pos + VoxelData.faceChecks[i])) {

        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 0]]);
        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 1]]);
        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 2]]);
        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 3]]);

        AddTexture(VoxelData.blockTypes[blockID].GetTextureID(i));

        if (!isTransparent) {
          triangles.Add(vertexIndex);
          triangles.Add(vertexIndex + 1);
          triangles.Add(vertexIndex + 2);
          triangles.Add(vertexIndex + 2);
          triangles.Add(vertexIndex + 1);
          triangles.Add(vertexIndex + 3);
        } else {
          transparentTriangles.Add(vertexIndex);
          transparentTriangles.Add(vertexIndex + 1);
          transparentTriangles.Add(vertexIndex + 2);
          transparentTriangles.Add(vertexIndex + 2);
          transparentTriangles.Add(vertexIndex + 1);
          transparentTriangles.Add(vertexIndex + 3);
        }
        
        vertexIndex += 4;
      }
    }

    if (vertices.Count != uvs.Count) {
      Debug.Log("\nblocks: ");
      Debug.Log("vertices: " + vertices.Count);
    Debug.Log("uvs: " + uvs.Count);

      for (int x = 0; x < VoxelData.ChunkWidth; x++) {
        for (int z = 0; z < VoxelData.ChunkWidth; z++) {
          for (int y = 0; y < VoxelData.ChunkHeight; y++) {
            Debug.Log(x + "/" + y + "/" + z + ": " + VoxelData.blockTypes[blocks[x, z, y]].name);
          }
        }
      }
    }
  }

  public void CreateMesh() {
    Mesh mesh = new Mesh();

    
    /*Debug.Log("=====\nChunk: " + position.x + "/" + position.z);
    if (vertices.Count != uvs.Count) {
      Debug.Log("\nblocks: ");

      for (int x = 0; x < VoxelData.ChunkWidth; x++) {
        for (int z = 0; z < VoxelData.ChunkWidth; z++) {
          for (int y = 0; y < VoxelData.ChunkHeight; y++) {
            Debug.Log(x + "/" + y + "/" + z + ": " + VoxelData.blockTypes[blocks[x, z, y]].name);
          }
        }
      }
    }
    
    Debug.Log("vertices: " + vertices.Count);
    Debug.Log("uvs: " + uvs.Count);*/

    mesh.vertices = vertices.ToArray();

    mesh.subMeshCount = 2;
    mesh.SetTriangles(triangles.ToArray(), 0);
    mesh.SetTriangles(transparentTriangles.ToArray(), 1);

    mesh.uv = uvs.ToArray();

    mesh.RecalculateNormals();

    meshFilter.mesh = mesh;
  }

  void AddTexture(int textureID) {
    float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
    float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

    x *= VoxelData.NormalizedBlockTextureSize;
    y *= VoxelData.NormalizedBlockTextureSize;

    y = 1f - y - VoxelData.NormalizedBlockTextureSize;

    uvs.Add(new Vector2(x, y));
    uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
    uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
    uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
  }
}
