using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorBlindStateManager : MonoBehaviour
{
    private static readonly Color[,] machadoRGB =
        {
            { new Color(1f,0f,-0f),   new Color(0f,1f,0f), new Color(-0f,0f,1f) },                                                                      // Normal
            { new Color(0.152286f, 1.052583f, -0.204868f), new Color(0.114503f, 0.786281f, 0.099216f), new Color(-0.003882f, -0.048116f, 1.051998f) },  // Protanopia
            { new Color(0.367322f, 0.860646f, -0.227968f), new Color(0.280085f, 0.672501f, 0.047413f), new Color(-0.011820f, 0.042940f, 0.968881f) },   // Deuteranopia
            { new Color(1.255528f, -0.076749f, -0.178779f), new Color(-0.078411f, 0.930809f, 0.147602f), new Color(0.004733f, 0.691367f, 0.303900f) },  // Tritanopia
            { new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f) },                                         // Achromatopsia
        };

    private static readonly Color[,] coblisV1RGB =
        {
            { new Color(1f,0f,0f),   new Color(0f,1f,0f), new Color(0f,0f,1f) },                                    // Normal
            { new Color(.56667f, .43333f, 0f), new Color(.55833f, .44167f, 0f), new Color(0f, .24167f, .75833f) },  // Protanopia
            { new Color(.625f, .375f, 0f), new Color(.70f, .30f, 0f), new Color(0f, .30f, .70f) },                  // Deuteranopia
            { new Color(.95f, .05f, 0), new Color(0f, .43333f, .56667f), new Color(0f, .475f, .525f) },             // Tritanopia
            { new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f) },     // Achromatopsia
            //{ new Color(.81667f, .18333f, 0f), new Color(.33333f, .66667f, 0f), new Color(0f, .125f, .875f) },      // Protanomaly
            //{ new Color(.80f, .20f, 0f), new Color(.25833f, .74167f, 0), new Color(0f, .14167f, .85833f) },         // Deuteranomaly
            //{ new Color(.96667f, .03333f, 0), new Color(0f, .73333f, .26667f), new Color(0f, .18333f, .81667f) },   // Tritanomaly
            //{ new Color(.618f, .32f, .062f), new Color(.163f, .775f, .062f), new Color(.163f, .320f, .516f)  }      // Achromatomaly
        };

    public readonly List<Color[,]> matrixSwitch = new()
        {
            coblisV1RGB,
            machadoRGB,
        };

    public enum ColorBlindType
    {
        CoblisV1,
        Machado,
    }

    public ColorBlindType type;

    public enum ColorBlindMode
    {
        Normal,
        Protanopia,
        Deuteranopia,
        Tritanopia,
        Achromatopsia,
    }

    public ColorBlindMode mode;

    public float severity = 1f;

    public Material material;

    private bool once = false;

    // Update is called once per frame
    void Update()
    {
        UpdateMaterialProperties();
    }

    void UpdateMaterialProperties()
    {
        if (once == false)
        {
            GetComponent<Renderer>().material = new Material(material);
            once = true;
        }

        var current_material = GetComponent<Renderer>().material;
        Color[] current_matrix = { current_material.GetColor("_R"), current_material.GetColor("_G"), current_material.GetColor("_B") };

        int requested_type = (int)type;
        int requested_mode = (int)mode;
        var requested_matrix = matrixSwitch[requested_type];

        //Debug.Log("current R" + current_material.GetColor("_R") + " current G: " + current_material.GetColor("_G") + " current B: " + current_material.GetColor("_B"));
        //Debug.Log("type: " + requested_type + " mode: " + requested_mode);
        //Debug.Log("new R: " + requested_matrix[requested_mode, 0] + " new G: " + requested_matrix[requested_mode, 1] + " new B: " + requested_matrix[requested_mode, 2]);

        if (current_matrix[0] != requested_matrix[requested_mode, 0])
        {

            current_material.SetColor("_R", requested_matrix[requested_mode, 0]);
            current_material.SetColor("_G", requested_matrix[requested_mode, 1]);
            current_material.SetColor("_B", requested_matrix[requested_mode, 2]);

        }

        float current_severity = current_material.GetFloat("_Severity");
        float requested_severity = severity;

        if (requested_severity != current_severity)
            current_material.SetFloat("_Severity", requested_severity);
    }
}
