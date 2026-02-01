using System.Collections.Generic;
using UnityEngine;

public class OutlineHighlight : MonoBehaviour
{
    [SerializeField] private Color outlineColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private float outlineThickness = 0.02f;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Renderer[] excludeRenderers;
    [SerializeField] private Transform[] excludeObjects;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Material[][] outlinedMaterials;
    private Material outlineMaterialInstance;
    private bool isHighlighted;

    private void Awake()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        renderers = FilterRenderers(allRenderers);
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

    private Renderer[] FilterRenderers(Renderer[] allRenderers)
    {
        if (allRenderers == null)
        {
            return allRenderers;
        }

        HashSet<Renderer> excluded = BuildExcludedSet();
        if (excluded.Count == 0)
        {
            return allRenderers;
        }

        List<Renderer> filtered = new List<Renderer>(allRenderers.Length);
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer renderer = allRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (!excluded.Contains(renderer))
            {
                filtered.Add(renderer);
            }
        }

        return filtered.ToArray();
    }

    private HashSet<Renderer> BuildExcludedSet()
    {
        HashSet<Renderer> excluded = new HashSet<Renderer>();

        if (excludeRenderers != null)
        {
            for (int i = 0; i < excludeRenderers.Length; i++)
            {
                if (excludeRenderers[i] != null)
                {
                    excluded.Add(excludeRenderers[i]);
                }
            }
        }

        if (excludeObjects != null)
        {
            for (int i = 0; i < excludeObjects.Length; i++)
            {
                Transform root = excludeObjects[i];
                if (root == null)
                {
                    continue;
                }

                Renderer[] childRenderers = root.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < childRenderers.Length; j++)
                {
                    if (childRenderers[j] != null)
                    {
                        excluded.Add(childRenderers[j]);
                    }
                }
            }
        }

        return excluded;
    }
}
