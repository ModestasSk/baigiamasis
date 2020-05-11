using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    int[,] defMap;
    public Renderer textureRenderer;
    public int height;
    public int lenght;


    Color AddColor(int num)
    {
        Color col = new Color();
        if (num == 0)
            col = Color.black;
        if (num == 1)
            col = Color.green;
        if (num == 2)
            col = Color.yellow;
        if (num == 3)
            col = Color.gray;
        if (num == 4)
            col = new Color(0.66f, 0.80f, 0.60f, 1);
        if (num == 5)
            col = Color.white;
        if (num == 6)
            col = new Color(111f/255f, 132f / 255f, 101f / 255f, 1);
        return col;
    }

    void Start()
    {
        defMap = new int[5,5];
        defMap[0, 0] = 5;
        defMap[0, 1] = 5;
        defMap[0, 2] = 5;
        defMap[0, 3] = 3;
        defMap[0, 4] = 5;

        defMap[1, 0] = 3;
        defMap[1, 1] = 4;
        defMap[1, 2] = 4;
        defMap[1, 3] = 4;
        defMap[1, 4] = 4;

        defMap[2, 0] = 1;
        defMap[2, 1] = 1;
        defMap[2, 2] = 6;
        defMap[2, 3] = 1;
        defMap[2, 4] = 1;

        defMap[3, 0] = 1;
        defMap[3, 1] = 1;
        defMap[3, 2] = 6;
        defMap[3, 3] = 1;
        defMap[3, 4] = 1;

        defMap[4, 0] = 4;
        defMap[4, 1] = 5;
        defMap[4, 2] = 5;
        defMap[4, 3] = 5;
        defMap[4, 4] = 4;


        Color[] colorMap = new Color[lenght * height];
        for (int i = 0; i < height; i++)
        {
            for (int k = 0; k < lenght; k++)
            {
                colorMap[i * lenght + k] = AddColor(defMap[i, k]);
            }
        }
        DrawTexture(TextureFromColorMap(colorMap, lenght, height));
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Generate(ref lenght, ref height, ref defMap);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            Smooth();
        }
    }

    public void Generate(ref int lenght, ref int height, ref int[,] originalMap)
    {
        lenght = (lenght * 2)-1;
        height = (height * 2)-1;
        int[,] map = new int[lenght, height];
        Color[] colorMap = new Color[lenght * height];

        //place old pixels on a larger array
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                if ((y % 2) == 0 && (x % 2) == 0)
                    map[y, x] = originalMap[y / 2, x/2];
                else map[y, x] = 0;
            }
        }

        List<int> temp = new List<int>();
        //filling pixels inbetween  the lines
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                if (map[y, x] == 0)
                {
                    temp.Clear();
                    if (y % 2 == 0)
                    {
                        temp.Add(map[y, x - 1]);
                        temp.Add(map[y, x + 1]);
                        int rand = Random.Range(0, 2);
                        map[y, x] = temp[rand];
                    }             
                    if((y % 2) == 1)
                    {
                        if (x % 2 == 0)
                        {
                            temp.Add(map[y - 1, x]);
                            temp.Add(map[y + 1, x]);
                            int rand = Random.Range(0, 2);
                            map[y, x] = temp[rand];
                        }
                    }                
                }
            }
        }

        //coverring the remaining holes
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                if (map[y, x] == 0)
                {
                    temp.Clear();
                    temp.Add(map[y, x - 1]);
                    temp.Add(map[y, x + 1]);
                    temp.Add(map[y+1, x]);
                    temp.Add(map[y-1, x]);
                    int rand = Random.Range(0, 4);
                    map[y, x] = temp[rand];
                }
            }
        }

        //applyng colors
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                colorMap[y * lenght + x] = AddColor(map[y, x]);
            }
        }
        originalMap = map;
        DrawTexture(TextureFromColorMap(colorMap, lenght, height));
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.material.SetTexture("_BaseMap", texture);
    }

    public void Smooth()
    {
        Color[] colorMap = new Color[lenght * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                if (x != 0 && x != (lenght - 1))
                {
                    if (y != 0 && y != (height - 1))
                    {
                        Debug.Log(x+"-"+y);
                        if (defMap[y - 1, x] == defMap[y + 1, x] && defMap[y, x + 1] == defMap[y, x - 1])
                        {
                            List<int> temp = new List<int>();
                            temp.Add(defMap[y - 1, x]);
                            temp.Add(defMap[y, x - 1]);
                            int rand = Random.Range(0, 2);
                            defMap[y, x] = temp[rand];
                        }
                        else if ((defMap[y - 1, x] == defMap[y + 1, x]))
                        {
                            defMap[y, x] = defMap[y - 1, x];
                        }
                        else if ((defMap[y, x + 1] == defMap[y, x - 1]))
                        {
                            defMap[y, x] = defMap[y, x + 1];
                        }
                    }
                }
            }

        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                colorMap[y * lenght + x] = AddColor(defMap[y, x]);

            }
        }
        DrawTexture(TextureFromColorMap(colorMap, lenght, height));
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

}
