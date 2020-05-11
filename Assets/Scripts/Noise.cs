using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Noise
{
   public static float[,] GenerateNoiseMap(int seed, int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, Texture2D mask, float maskStrenght)
    {
        Tile[,] tileMapTemp = new Tile[mapWidth, mapHeight];
        float[,] noiseMap = new float[mapWidth, mapHeight];
        scale = Mathf.Clamp(scale, 0.0001f, 10000);

        System.Random random = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetY = random.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        Color[] pixels = new Color[mapWidth * mapHeight];
        if (mask!= null)
            pixels = mask.GetPixels();
        else
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    pixels[y * mapWidth + x] = new Color(0,0,0);
                }
            }
        }
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    amplitude *= persistance;
                    frequency *= lacunarity;
                    float sampleX = (x- halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    var pixel = pixels[y * mapWidth + x];
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 -1 * (pixel.grayscale* maskStrenght);
                    tileMapTemp[x, y] = new Tile(); 
                    tileMapTemp[x, y].coordinates.x = x;
                    tileMapTemp[x, y].coordinates.y = y;
                    noiseHeight += perlinValue * amplitude;
                    //amplitude *= persistance;
                    //frequency *= lacunarity;
                }
                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
                
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                tileMapTemp[x, y].terrainType.height = noiseMap[x, y];
            }
        }
        return noiseMap;
    }
}
