using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeData {
     
  public string name;

  public int terrainMinHeight;
  public int terrainMaxHeight;

  public float terrainScale;

  public float treeZoneScale;
  public float treeZoneThreshold;
  public float treePlacementScale;
  public float treePlacementThreshold;
  public int minTreeHeight;
  public int maxTreeHeight;

  public Lode caveLode;
  public Lode[] ressourceLodes;
  public Lode[] terrainLodes;

  public BiomeData(string _name,
    int _terrainMinHeight, int _terrainMaxHeight, float _terrainScale,
    float _treeZoneScale, float _treeZoneThreshold, float _treePlacementScale, float _treePlacementThreshold, int _minTreeHeight, int _maxTreeHeight,
    Lode _caveLode, Lode[] _ressourceLodes, Lode[] _terrainLodes
  ) {
    name = _name;

    terrainMinHeight = _terrainMinHeight;
    terrainMaxHeight = _terrainMaxHeight;
    terrainScale = _terrainScale;

    treeZoneScale = _treeZoneScale;
    treeZoneThreshold = _treeZoneThreshold;
    treePlacementScale = _treePlacementScale;
    treePlacementThreshold = _treePlacementThreshold;
    minTreeHeight = _minTreeHeight;
    maxTreeHeight = _maxTreeHeight;

    caveLode = _caveLode;
    ressourceLodes = _ressourceLodes;
    terrainLodes = _terrainLodes;
  }

}

public class Lode {

  public string name;
  public byte blockID;
  public int minHeight;
  public int maxHeight;
  public float scale;
  public float threshold;

  public Lode(string _name, byte _blockID, int _minHeight, int _maxHeight, float _scale, float _threshold) {
    name = _name;
    blockID = _blockID;
    minHeight = _minHeight;
    maxHeight = _maxHeight;
    scale = _scale;
    threshold = _threshold;
  }

}
