using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<int> list = new List<int>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            int random = Random.Range(0, list.Count - 1);
            list.RemoveAt(random);
        }
    }
}
