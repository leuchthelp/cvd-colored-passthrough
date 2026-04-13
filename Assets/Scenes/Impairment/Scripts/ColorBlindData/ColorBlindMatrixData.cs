using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorBlindMatrixData", menuName = "Scriptable Objects/ColorBlindMatrixData")]
public class ColorBlindMatrixData : ScriptableObject
{
    public static readonly Color[,] machadoRGB =
        {
            { new Color(1f,0f,-0f),   new Color(0f,1f,0f), new Color(-0f,0f,1f) },                                                                      // Normal
            { new Color(0.152286f, 1.052583f, -0.204868f), new Color(0.114503f, 0.786281f, 0.099216f), new Color(-0.003882f, -0.048116f, 1.051998f) },  // Protanopia
            { new Color(0.367322f, 0.860646f, -0.227968f), new Color(0.280085f, 0.672501f, 0.047413f), new Color(-0.011820f, 0.042940f, 0.968881f) },   // Deuteranopia
            { new Color(1.255528f, -0.076749f, -0.178779f), new Color(-0.078411f, 0.930809f, 0.147602f), new Color(0.004733f, 0.691367f, 0.303900f) },  // Tritanopia
            { new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f) },                                         // Achromatopsia
        };

    public static readonly Color[,] coblisV1RGB =
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
}
