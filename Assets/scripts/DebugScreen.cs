using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{

    World world;
    Player playerScript;
    Text text;

    float framerate;
    float timer;
    int halfWorldInBlocks;
    int halfWorldInChunks;

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        playerScript = world.player.GetComponent<Player>();
        text = GetComponent<Text>();

        timer = 0;
        halfWorldInBlocks = VoxelData.WorldSizeInBlocks / 2;
        halfWorldInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 1f) {
          framerate = (int)(1f / Time.unscaledDeltaTime);
          timer = 0;
        } else
          timer += Time.deltaTime;

        Vector3 playerPositionInChunk =
          new Vector3(
            world.player.transform.position.x - world.playerCurrentChunk.x * VoxelData.ChunkWidth,
            world.player.transform.position.y,
            world.player.transform.position.z - world.playerCurrentChunk.z * VoxelData.ChunkWidth
          );

        int pX = Mathf.FloorToInt(world.player.transform.position.x);
        int pY = Mathf.FloorToInt(world.player.transform.position.y);
        int pZ = Mathf.FloorToInt(world.player.transform.position.z);
          
        string debugText = "DEBUG\n";
        debugText += framerate + " FPS\n\n";
        
        debugText += "CHUNK X/Z: " + (world.playerCurrentChunk.x - halfWorldInChunks) + "/" + (world.playerCurrentChunk.z - halfWorldInChunks) + "\n";
        debugText += "PLAYER CHUNK X/Y/Z: " + (pX - (world.playerCurrentChunk.x * VoxelData.ChunkWidth)) + "/" + pY + "/" + (pZ - (world.playerCurrentChunk.z * VoxelData.ChunkWidth)) + "\n";
        debugText += "PLAYER WORLD X/Y/Z: " + (pX - halfWorldInBlocks) + "/" + pY + "/" + (pZ - halfWorldInBlocks) + "\n";
  
        debugText += "POINTED BLOCK: ";
        if (playerScript.reachedBlockID == BlockID.AIR)
          debugText += " -\n";
        else {
          debugText += VoxelData.blockTypes[playerScript.reachedBlockID].name;
          debugText += " " + playerScript.reachedBlockPosition.x + "/" + playerScript.reachedBlockPosition.y + "/" + playerScript.reachedBlockPosition.z + "\n";
        }

        text.text = debugText;
    }
}
