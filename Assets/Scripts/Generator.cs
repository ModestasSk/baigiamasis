using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BiomeAddon
{
    public BiomeObject biomeAppearace;
    public Transform addon;
    public int ammount;
    public float density;
}

public enum Moisture {
    Dryiest,
    Dryer,
    Dry,
    Wet,
    Wetter,
    Wettest
}

public enum Temperature
{
    Coldest,
    Colder,
    Cold,
    Hot,
    Hotter,
    Hottest
}

[System.Serializable]
public class TerrainTemp
{
    public float weight;
    public float temperature;
    public Color color;
    public Temperature tempType;
}

[System.Serializable]
public class TerrainMoisture
{
    public float weight;
    public Moisture moistureType;
}

[System.Serializable]
public class Tile
{
    public Vector2 coordinates;
    public Color color;
    public TerrainHeight terrainType;
    public BiomeObject biomeType;
    public string biomeName;
    public Moisture moisture;
    public Temperature tempType;
}

[System.Serializable]
public struct TerrainHeight
{
    public float temperatureHeighImpact;
    public float height;
}

[System.Serializable]
public struct BiomeRule
{
    public Temperature tempType;
    public Moisture moistureType;
}

public enum DrawMode { Standart, Noisemap, TempMap, MoistureMap};
public class Generator : MonoBehaviour
{
    Tile[,] tileMap;
    
    public RawImage DisplayImage;
    public DrawMode drawMode;
    public bool autoDraw;
    public bool hasAddons;
    public bool debugMode;

    [Header("Noise parameters")]
    public int seed = 0;
    public Vector2 offset = new Vector2(0, 0);
    [Min(1)]
    public int worldHeight = 256;
    [HideInInspector]
    [Min(1)]
    public int worldWidth = 256;
    [Range(1, 500)]
    public float noiseScale = 120f;
    [Range(1, 10)]
    public int octaves = 4;
    [Range(0.01f, 10)]
    public float persistance = 0.5f;
    [Range(0.01f, 10)]
    public float lacunarity = 2.5f;
    [Range(1, 50)]
    public float terrainSmoothness = 15f;


    [Header("Terrain masks")]
    public Texture2D terrainMask;
    public Texture2D temperatureMask;
    public float terrainMaskStrenght = -2f;
    Texture2D noiseLayer;
    [HideInInspector]
    public Texture2D biomeLayer;
    Texture2D temperatureMap;
    Texture2D moistureMap;

    [Header("Biome properties")]
    public AnimationCurve moistureCurve;
    public List<TerrainHeight> heightRules = new List<TerrainHeight>();
    public List<TerrainTemp> temperatureRules = new List<TerrainTemp>();
    public List<TerrainMoisture> moistureRules = new List<TerrainMoisture>();
    public List<BiomeObject> biomeObjects = new List<BiomeObject>();
    List<float[,]> biomeMaps = new List<float[,]>();
    public float brightness = 2f;

    [Header("TerrainAddons")]
    public List<BiomeAddon> biomeAddons = new List<BiomeAddon>();
    public List<Transform> spawnedAddons = new List<Transform>();

    //Neprivaloma/neieina i sistema, cia kad lengviau testuoti palikau
    [Header("Debug")]
    public RawImage defaultNoiseMap;
    public RawImage noiseMap;
    public RawImage noiseMask;
    public RawImage heatMap;
    public RawImage heatMask;
    public RawImage heatmoistureMap;
    public RawImage biomeMap;


   


    public void GenerateTerrain()
    {
        //siuo metu nepalaiko skirtingu dimensiju, tad matmenys bus tie patys kolkas
        worldWidth = worldHeight;

            TextureScale.Point(temperatureMask, worldHeight, worldWidth);
        if (terrainMask != null)
            TextureScale.Point(terrainMask, worldHeight, worldWidth);

        tileMap = new Tile[worldHeight, worldWidth];
        float[,] noiseMap = Noise.GenerateNoiseMap(seed, worldWidth, worldHeight, noiseScale, octaves, persistance, lacunarity, offset, terrainMask, terrainMaskStrenght);
        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                tileMap[x, y] = new Tile();
                tileMap[x, y].coordinates = new Vector2(x, y);
                tileMap[x, y].terrainType.height = noiseMap[x, y];
                float currentHeight = tileMap[x, y].terrainType.height;
                for (int i = 0; i < heightRules.Count; i++)
                {
                    if (currentHeight <= heightRules[i].height)
                    {
                        tileMap[x, y].terrainType.temperatureHeighImpact = heightRules[i].temperatureHeighImpact;
                        break;
                    }
                }
            }
        }

        noiseLayer = TextureFromHeightMap(noiseMap);
        GenerateBiomes(noiseMap);
        if (DisplayImage)
            DisplayView(drawMode);
        DisplayImage.rectTransform.localScale = new Vector3(worldWidth, worldHeight);
        DisplayImage.rectTransform.position = new Vector3(worldWidth / 2, worldHeight / 2);
        ClearAddons();
        if (hasAddons)
            GenerateAddons();
        DebugImage();
    }

    public void GenerateBiomes(float[,] noiseMap)
    {
        temperatureMap = GenerateTemperatureColorMap(noiseMap);
        moistureMap = GenerateMoistureMap(noiseMap);
        Color[] colorMap = new Color[worldWidth * worldHeight];
        float brightness;
        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                if (heightRules != null && tileMap[x, y].terrainType.height > heightRules[0].height && tileMap[x, y].terrainType.height < heightRules[heightRules.Count-1].height)
                {
                    for (int i = 0; i < biomeObjects.Count; i++)
                    {
                        for (int k = 0; k < biomeObjects[i].biomeRule.Count; k++)
                        {
                            if (tileMap[x, y].moisture == biomeObjects[i].biomeRule[k].moistureType && tileMap[x, y].tempType == biomeObjects[i].biomeRule[k].tempType)
                            {
                               
                                tileMap[x, y].biomeType =  biomeObjects[i];
                                brightness = Mathf.Clamp((Mathf.Round((Mathf.Abs(1 - tileMap[x, y].terrainType.height)) * terrainSmoothness) / terrainSmoothness), 0.2f, 10f) * this.brightness;
                                tileMap[x, y].color = new Color(biomeObjects[i].color.r * brightness, biomeObjects[i].color.g* brightness, biomeObjects[i].color.b* brightness);
                                tileMap[x, y].biomeName = "";


                            }
                        }
                       
                    }
                }
                else if(tileMap[x, y].terrainType.height <= heightRules[0].height)
                {
                    tileMap[x, y].biomeType = biomeObjects[0];
                    brightness = Mathf.Clamp((Mathf.Round((Mathf.Abs(tileMap[x, y].terrainType.height)) * terrainSmoothness) / terrainSmoothness), 0.2f, 10f) * this.brightness*2;
                    tileMap[x, y].color = new Color(biomeObjects[0].color.r * brightness, biomeObjects[0].color.g * brightness, biomeObjects[0].color.b * brightness);
                    tileMap[x, y].biomeName = "";
                }

                else
                {
                    tileMap[x, y].biomeType = biomeObjects[1];
                    brightness = Mathf.Clamp((Mathf.Round((Mathf.Abs(tileMap[x, y].terrainType.height)) * terrainSmoothness) / terrainSmoothness), 0.2f, 10f) * this.brightness * 2;
                   
                    tileMap[x, y].biomeName = "";
                }
                
                
            }
        }

        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                colorMap[y * worldWidth + x] = tileMap[x, y].color;
            }
        }

        biomeLayer = TextureFromColorMap(colorMap, worldWidth, worldHeight);     
    }

    public Texture2D GenerateTemperatureColorMap(float[,] noiseMap)
    {
        float[,] tempMap = new float[worldWidth, worldHeight];
        Color[] colorMap = new Color[worldWidth * worldHeight];
        Color[] colorMapLat = new Color[worldWidth * worldHeight];
        Color[] pixelsLat = CreateColorArray(temperatureMask, worldWidth, worldHeight);

        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                var pixel = pixelsLat[y * worldWidth + x].grayscale;
                for (int i = 0; i < temperatureRules.Count; i++)
                {
                    if ((pixel - (tileMap[x, y].terrainType.height * tileMap[x, y].terrainType.temperatureHeighImpact)) <= temperatureRules[i].weight)
                    {
                        tileMap[x, y].tempType = temperatureRules[i].tempType;
                        colorMapLat[y * worldWidth + x] = new Color(temperatureRules[i].color.r, temperatureRules[i].color.g, temperatureRules[i].color.b, 1);
                        break;
                    }
                }
            }
        }

        return TextureFromColorMap(colorMapLat, worldWidth, worldHeight);
    }

    public Texture2D GenerateMoistureMap(float[,] noiseMap)
    {
        Color[] colorMap = new Color[worldWidth * worldHeight];
        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                noiseMap[x, y] -= this.moistureCurve.Evaluate(tileMap[x, y].terrainType.height * tileMap[x, y].terrainType.height);
                for (int i = 0; i < moistureRules.Count; i++)
                {
                    if (noiseMap[x, y] <= moistureRules[i].weight)
                    {
                        tileMap[x, y].moisture = moistureRules[i].moistureType;
                        break;
                    }
                }
            }
        }
        return TextureFromHeightMap(noiseMap);
    }

    public bool DisplayView(DrawMode mode)
    {       
        if (mode.Equals(DrawMode.Standart))
        {
            if(biomeLayer != null)
                DisplayImage.texture = biomeLayer;
            return true;
        }
        if (mode.Equals(DrawMode.TempMap))
        {
            if (temperatureMap != null)
                DisplayImage.texture = temperatureMap;
            return true;
        }
        if (mode.Equals(DrawMode.MoistureMap))
        {
            if (moistureMap != null)
                DisplayImage.texture = moistureMap;
            return true;
        }
        if (mode.Equals(DrawMode.Noisemap))
        {
            if (noiseLayer != null)
                DisplayImage.texture = noiseLayer;
            return true;
        }
        return false;
    }

    public void ClearAddons()
    {
        if (spawnedAddons.Count > 0)
        {
            foreach (Transform transform in spawnedAddons)
            {
                DestroyImmediate(transform.gameObject);
            }
        }
        spawnedAddons.Clear();
    }

    public void GenerateAddons()
    {
        List<Vector2> finalPoints = new List<Vector2>();
        List<Transform> objectsToSpawn = new List<Transform>();
        foreach (BiomeAddon addon in biomeAddons)
        {
            List<Transform> tempObjectList = new List<Transform>();
            List<Vector2> tempPoints = new List<Vector2>();
            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    Vector2 t = new Vector2(x,y);
                    if (tileMap[x, y].biomeType != null && tileMap[x, y].biomeType.Equals(addon.biomeAppearace) && !finalPoints.Contains(t))
                    {
                        tempPoints.Add(new Vector2(x,y));
                    }
                }
            }
            if (tempPoints.Count > 0)
            {
                int randomNumber = 0;   
                for (int i = 0; i < addon.ammount; i++)
                {
                    while (true)
                    {
                        randomNumber = Random.Range(0, tempPoints.Count);
                        bool check = true;
                        
                            foreach (Vector2 p in finalPoints)
                            {
                                if (tempPoints != null && Vector2.Distance(tempPoints[randomNumber], p) < addon.density)
                                {
                                    tempPoints.RemoveAt(randomNumber);
                                    check = false;
                                    break;
                                }
                            }
                        
                        if (check)
                        {
                            finalPoints.Add(tempPoints[randomNumber]);
                            tempPoints.RemoveAt(randomNumber);
                            objectsToSpawn.Add(addon.addon);
                            //tempPoints.Remove(tempPoints[randomNumber]);
                            break;
                        }
                    }
                }
            }
            
        }
        for (int i = 0; i < finalPoints.Count; i++)
        {
            var temp = Instantiate(objectsToSpawn[i], finalPoints[i], objectsToSpawn[i].rotation);
            temp.parent = this.transform;
            spawnedAddons.Add(temp.transform);
        }
    }


    //Papildomos palengvinamos bet neprivalomos funkcijos, tiesiog isskaidyta dėl patogumo.

    public void DebugImage()
    {
        if (debugMode)
        {
            defaultNoiseMap.texture = TextureFromHeightMap(Noise.GenerateNoiseMap(seed, worldWidth, worldHeight, noiseScale, octaves, persistance, lacunarity, offset, terrainMask, 0f));
            noiseMap.texture = noiseLayer;
            if(terrainMask!= null)
                noiseMask.texture = terrainMask;
            heatMap.texture = temperatureMap;
            heatMask.texture = temperatureMask;
            heatmoistureMap.texture = moistureMap;
        }
    }

    public Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        return TextureFromColorMap(colorMap, width, height);
    }

    Color[] CreateColorArray(Texture2D mask, int worldWidth, int worldHeight)
    {
        Color[] colArray = new Color[worldWidth * worldHeight];
        if (mask != null)
            colArray = mask.GetPixels();
        else
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    colArray[y * worldWidth + x] = new Color(0, 0, 0);
                }
            }
        }
        return colArray;
    }
}
