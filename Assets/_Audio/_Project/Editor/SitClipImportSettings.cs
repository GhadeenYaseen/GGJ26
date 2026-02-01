using System.IO;
using UnityEditor;

public class SitClipImportSettings : AssetPostprocessor
{
    private static readonly string[] TargetNames =
    {
        "Sitting.fbx",
        "Sitting Talking.fbx"
    };

    private void OnPreprocessModel()
    {
        if (!IsTarget(assetPath))
        {
            return;
        }

        ModelImporter importer = (ModelImporter)assetImporter;
        if (!importer.importAnimation)
        {
            importer.importAnimation = true;
        }

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        for (int i = 0; i < clips.Length; i++)
        {
            clips[i].lockRootRotation = true;  // Bake Into Pose (Rotation)
            clips[i].lockRootPositionXZ = true; // Bake Into Pose (Position XZ)
            clips[i].lockRootHeightY = true; // Bake Into Pose (Position Y)
        }

        importer.clipAnimations = clips;
    }

    private static bool IsTarget(string path)
    {
        string fileName = Path.GetFileName(path);
        for (int i = 0; i < TargetNames.Length; i++)
        {
            if (fileName == TargetNames[i])
            {
                return true;
            }
        }

        return false;
    }
}
