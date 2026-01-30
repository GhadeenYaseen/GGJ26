using UnityEngine;

public class OutlineHighlight : MonoBehaviour
{
    [SerializeField] private Color outlineColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private float outlineThickness = 0.02f;
    [SerializeField] private Material outlineMaterial;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Material[][] outlinedMaterials;
    private Material outlineMaterialInstance;
    private bool isHighlighted;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[renderers.Length][];
        outlinedMaterials = new Material[renderers.Length][];

        Shader shader = outlineMaterial != null ? outlineMaterial.shader : Shader.Find("Custom/OutlineBackface");
        if (shader == null)
        {
            Debug.LogWarning("OutlineHighlight: Missing shader Custom/OutlineBackface.");
            enabled = false;
            return;
        }

        outlineMaterialInstance = outlineMaterial != null
            ? new Material(outlineMaterial)
            : new Material(shader);

        outlineMaterialInstance.SetColor("_Color", outlineColor);
        outlineMaterialInstance.SetFloat("_Thickness", outlineThickness);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            originalMaterials[i] = renderer.sharedMaterials;

            Material[] combined = new Material[originalMaterials[i].Length + 1];
            for (int j = 0; j < originalMaterials[i].Length; j++)
            {
                combined[j] = originalMaterials[i][j];
            }

            combined[combined.Length - 1] = outlineMaterialInstance;
            outlinedMaterials[i] = combined;
        }
    }

    private void OnDestroy()
    {
        if (outlineMaterialInstance != null)
        {
            Destroy(outlineMaterialInstance);
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (isHighlighted == highlighted)
        {
            return;
        }

        isHighlighted = highlighted;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            renderers[i].sharedMaterials = highlighted ? outlinedMaterials[i] : originalMaterials[i];
        }
    }
}
