using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SpeedLinesAlphaMap : MonoBehaviour
{
    

    public RawImage SpeedLines;

    public float value;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {
        Mathf.Clamp(value, 0f, 1f);
        SpeedLines.color = new Color(1f, 1f, 1f, value);
        
    }


}
