using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[ExecuteInEditMode]
public class CityNames : MonoBehaviour
{
    public List<string> cityNames = new List<string>();
    public TextMeshProUGUI text;

    void Start()
    {
        int random = Random.Range(0, cityNames.Count-1);
        text.text = cityNames[random];
    }
}
