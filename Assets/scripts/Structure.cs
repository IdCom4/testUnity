using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure {

  public static Queue<BlockMod> MakeTree(Vector3 pos, int minTreeHeight, int maxTreeHeight) {

    Queue<BlockMod> queue = new Queue<BlockMod>();

    int height = (int)(maxTreeHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 532f, 3f));

    if (height < minTreeHeight)
      height = minTreeHeight;

    for (int i = 1; i < height; i++) {
      queue.Enqueue(new BlockMod(new Vector3(pos.x, pos.y + i, pos.z), BlockID.WOOD));
    }

    int min = -3;
    int max = 3;

    System.Random rng = new System.Random();

    for (int x = min; x <= max; x++) {
      for (int z = min; z <= max; z++) {
        for (int y = min; y <= max; y++) {
          bool draw = true;
          float normalizedX = ((float)(x < 0 ? x : -x) / max) / 2 + 0.5f;
          float normalizedY = ((float)(y < 0 ? y : -y) / max) / 2 + 0.5f;
          float normalizedZ = ((float)(z < 0 ? z : -z) / max) / 2 + 0.5f;

          float normalizedAverage = (normalizedX + normalizedY + normalizedZ) / 3;
          float marge = (float)rng.NextDouble() / 4;
          
          if (normalizedAverage < 0.35f - marge || normalizedAverage > 0.65f + marge)
            draw = false;
          if (draw && (y > height || x != 0 || z != 0))
            queue.Enqueue(new BlockMod(new Vector3(pos.x + x, pos.y + height + y, pos.z + z), BlockID.LEAVES));
        }
      }
    }

    return queue;
  }

}
