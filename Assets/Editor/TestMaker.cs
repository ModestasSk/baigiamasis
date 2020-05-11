
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;


public class TestMaker
{
    [Test]
    public void FirstTest()
    {
        Generator gen = new Generator();
        gen.octaves = 2;
        gen.lacunarity = 2f;
        float[,] noise = Noise.GenerateNoiseMap(gen.seed, gen.worldWidth, gen.worldHeight, gen.noiseScale,
                                                gen.octaves, gen.persistance, gen.lacunarity, gen.offset, 
                                                gen.terrainMask, gen.terrainMaskStrenght);
        Assert.That(noise.Length, Is.EqualTo(gen.worldHeight* gen.worldHeight));
    }

    [Test]
    public void SecondTest()
    {
        Generator gen = new Generator();
        bool display = gen.DisplayView(gen.drawMode);
        Assert.That(display, Is.EqualTo(true));
    }

    [Test]
    public void ThirdTest()
    {
        Generator gen = new Generator();
        gen.drawMode = DrawMode.Noisemap;
        Assert.That(gen.drawMode, Is.EqualTo(DrawMode.Noisemap));
    }

    [Test]
    public void FourthTest()
    {
        Generator gen = new Generator();
        float[,] noise = Noise.GenerateNoiseMap(gen.seed, gen.worldWidth, gen.worldHeight, gen.noiseScale,
                                                gen.octaves, gen.persistance, gen.lacunarity, gen.offset,
                                                gen.terrainMask, gen.terrainMaskStrenght);

        TerrainHeight terrainHeight = new TerrainHeight();
        gen.heightRules.Add(terrainHeight);
        BiomeObject biomeObject = ScriptableObject.CreateInstance<BiomeObject>();
        gen.biomeObjects.Add(biomeObject);
        TerrainTemp terrainTemp = new TerrainTemp();
        gen.temperatureRules.Add(terrainTemp);
        gen.GenerateTerrain();
        Assert.That(gen.biomeLayer!= null, Is.EqualTo(true));
    }

    [Test]
    public void FifthTest()
    {
        Generator gen = new Generator();
        BiomeObject biomeObject = new BiomeObject();
        gen.biomeObjects.Add(biomeObject);
        Assert.That(gen.biomeObjects != null, Is.EqualTo(true));
    }

    [Test]
    public void SixthTest()
    {
        Generator gen = new Generator();
        BiomeObject biomeObject = new BiomeObject();
        biomeObject.name = "test";
        Assert.That(biomeObject.name, Is.EqualTo("test"));
    }

    [Test]
    public void SeventhTest()
    {
        Generator gen = new Generator();
        gen.octaves = 10;
        Assert.That(gen.octaves, Is.EqualTo(10));
    }

  
}
    