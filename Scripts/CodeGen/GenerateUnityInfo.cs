#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEditorInternal;

public class GenerateUnityInfo : MonoBehaviour
{
    const string cCodeGenOutputPath = "Assets/Scripts/CodeGen/Generated/UnityValues.cs";

    [MenuItem("Tools/Generate Unity Info")]
    static public void GenerateUnityValues()
    {
        //Generate the definition
        string classDef = GenerateCode();

        //Write the class to disk
        File.WriteAllText(cCodeGenOutputPath, classDef);

        //Tell Unity to refresh. 
        AssetDatabase.Refresh();
    }

    static private string GenerateCode()
    {
        StringBuilder codeGen = new StringBuilder();
        codeGen.Append("using UnityEditor;");

        // Grab all our layers from Unity. 
        List<string> layers = new List<string>(InternalEditorUtility.layers);
        GenerateUnityValuesForLayers("GameplayLayers", ref codeGen);

        // Grab all tags from Unity
        List<string> tags = new List<string>(InternalEditorUtility.tags);
        GenerateUnityValuesForTags("GameplayTags", ref codeGen);

        return codeGen.ToString();
    }

    static private void GenerateUnityValuesForLayers(string aClassName, ref StringBuilder codeGen)
    {
        codeGen.Append("\r\n\r\npublic static class " + aClassName);
        codeGen.Append("\r\n{\r\n");
        codeGen.Append("\r\n\r\n// The numbers here represent the actual value of the layer in Unity\r\n");

        int numEmptyLayers = 0;
        //Grab all our layers from Unity. 
        List<string> layers = new List<string>(InternalEditorUtility.layers);

        for (int i = layers.Count - 1; i >= 0; i--)
        {
            //Remove empty entires
            if (string.IsNullOrEmpty(layers[i]))
            {
                layers[i] = "EMPTY_" + numEmptyLayers;
                numEmptyLayers++;
                continue;
            }

            //Remove spaces
            if (layers[i].Contains(" "))
            {
                layers[i] = layers[i].Replace(' ', '_');
            }
        }

        int layerNumber = 0;
        for (int i = 0; i < layers.Count; i++)
        {
            codeGen.Append("public const int ");
            codeGen.Append(layers[i].Replace(" ", ""));
            codeGen.Append(" = ");
            codeGen.Append(layerNumber.ToString());
            codeGen.Append(";");
            if (i < layers.Count - 1)
            {
                codeGen.Append("\r\n");
            }
            layerNumber++;

            if(layers[i].CompareTo("Ignore_Raycast") == 0)
            {
                layerNumber++;
            }
            else if(layers[i].CompareTo("UI") == 0)
            {
                layerNumber += 2;
            }
        }

        codeGen.Append("\r\n");

        codeGen.Append("\r\n  public enum Values\r\n  {\r\n    ");
        for (int i = 0; i < layers.Count; i++)
        {
            codeGen.Append(layers[i].Replace(" ", "").ToUpper());
            codeGen.Append(" = ");
            codeGen.Append(i.ToString());
            codeGen.Append(",");
            if (i < layers.Count - 1)
            {
                codeGen.Append("\r\n    ");
            }
        }

        codeGen.Append("\r\n  }");

        codeGen.Append("\r\n}\r\n");
    }

    static private void GenerateUnityValuesForTags(string aClassName, ref StringBuilder codeGen)
    {
        codeGen.Append("\r\n\r\npublic static class " + aClassName);
        codeGen.Append("\r\n{\r\n");

        //Grab all our layers from Unity. 
        List<string> layers = new List<string>(InternalEditorUtility.tags);

        for (int i = layers.Count - 1; i >= 0; i--)
        {
            //Remove empty entires
            if (string.IsNullOrEmpty(layers[i]))
            {
                layers.RemoveAt(i);
                continue;
            }

            //Remove spaces
            if (layers[i].Contains(" "))
            {
                layers[i] = layers[i].Replace(' ', '_');
            }
        }

        for (int i = 0; i < layers.Count; i++)
        {
            string tagName = layers[i].Replace(" ", "");
            codeGen.Append("public static readonly string ");
            codeGen.Append(tagName.ToUpper());
            codeGen.Append(" = \"");
            codeGen.Append(tagName);
            codeGen.Append("\";");
            if (i < layers.Count - 1)
            {
                codeGen.Append("\r\n");
            }
        }

        codeGen.Append("\r\n");

        codeGen.Append("\r\n  public enum Values\r\n  {\r\n    ");
        for (int i = 0; i < layers.Count; i++)
        {
            codeGen.Append(layers[i].Replace(" ", "").ToUpper());
            codeGen.Append(" = ");
            codeGen.Append(i.ToString());
            codeGen.Append(",");
            if (i < layers.Count - 1)
            {
                codeGen.Append("\r\n    ");
            }
        }

        codeGen.Append("\r\n  }");

        codeGen.Append("\r\n}\r\n");
    }
}
#endif // UNITY_EDITOR