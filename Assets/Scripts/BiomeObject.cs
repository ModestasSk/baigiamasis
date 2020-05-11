using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Biome", menuName = "ScriptableObjects/BiomeObject", order = 1)]
public class BiomeObject : ScriptableObject
{
    public string name;
    public Color color;
    public float temperature;
    public float moisture;
    public List<BiomeRule> biomeRule = new List<BiomeRule>();
}
