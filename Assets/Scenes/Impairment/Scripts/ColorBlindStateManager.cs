using UnityEngine;
using UnityEngine.Rendering;

public class ColorBlindStateManager : MonoBehaviour
{
    private readonly ColorBlindMatrixData colorBlindMatrixData;
    public EnumParameter<ColorBlindMatrixType> type = new(ColorBlindMatrixType.CoblisV1);

    public EnumParameter<ColorBlindMode> mode = new(ColorBlindMode.Normal);

    public ClampedFloatParameter severity = new(1f, 0f, 1f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateMaterialProperties();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMaterialProperties();
    }

    void UpdateMaterialProperties()
    {
        int current_type = (int) type.value;
        int current_mode = (int) mode.value;

        Debug.Log("type: " + current_type + " mode: " + current_mode);

        var current_matrix = colorBlindMatrixData.matrixSwitch[current_type];
        float current_severity = severity.value;

        var material = new Material(Shader.Find("Shader Graphs/LocalColorBlindShader"));
        material.SetColor("_R", current_matrix[current_mode, 0]);
        material.SetColor("_G", current_matrix[current_mode, 0]);
        material.SetColor("_B", current_matrix[current_mode, 0]);
        material.SetFloat("_Severity", current_severity);
    }
}
