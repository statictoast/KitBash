#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Xml;

public class GenerateEventsInfo : MonoBehaviour 
{
    const string cCodeGenOutputPath = "Assets/Scripts/CodeGen/Generated/GameplayEvents.cs";
    const string cEventsPath = "Assets/Scripts/CodeGen/Xml/Events.xml";

    private struct EventParameterData
    {
        readonly public string name;
        readonly public string type;

        public EventParameterData(string aName, string aType)
        {
            name = aName;
            type = aType;
        }
    };

    [MenuItem("Tools/Generate Events")]
    static public void GenerateEvents()
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
        codeGen.Append("using UnityEngine;\r\nusing System;\r\n");

        XmlDocument eventXml = new XmlDocument();
        eventXml.Load(cEventsPath);

        XmlNode topNode = eventXml.DocumentElement.SelectSingleNode("/GameEvents");

        Dictionary<string, List<EventParameterData>> eventData = new Dictionary<string, List<EventParameterData>>();

        // Get a list of all required packages to include and append them first
        List<string> requiredPackages = new List<string>();
        foreach(XmlNode eventNode in topNode.ChildNodes)
        {
            XmlAttribute requiresAttribute = eventNode.Attributes["requires"];
            if(requiresAttribute != null)
            {
                string package = requiresAttribute.InnerText;
                if(!requiredPackages.Contains(package))
                {
                    requiredPackages.Add(package);
                }
            }
        }

        for(int i = 0; i < requiredPackages.Count; i++)
        {
            codeGen.Append("using " + requiredPackages[i] + ";\r\n");
        }

        codeGen.Append("\r\n");

        // Explore all events and make their classes
        foreach(XmlNode eventNode in topNode.ChildNodes)
        {
            string eventName = eventNode.Attributes["name"].InnerText;
            List<EventParameterData> parameterData = new List<EventParameterData>();
            eventData.Add(eventName, parameterData);

            foreach(XmlNode parameterNode in eventNode.ChildNodes)
            {
                string parameterName = parameterNode.Attributes["name"].InnerText;
                string parameterType = parameterNode.Attributes["type"].InnerText;
                parameterData.Add(new EventParameterData(parameterName, parameterType));
            }
        }

        var keys = eventData.Keys;

        // Event String Generation
        codeGen.Append("struct Events\r\n{\r\n");
        foreach (string eventName in keys)
        {
            string result = System.Text.RegularExpressions.Regex.Replace(eventName, "(?<=.)([A-Z])", "_$0", System.Text.RegularExpressions.RegexOptions.Singleline);
            string finalEventString = "EVENT_" + result.ToUpper();
            codeGen.Append("public static readonly string ");
            codeGen.Append(finalEventString);
            codeGen.Append(" = \"");
            codeGen.Append(eventName.ToLower());
            codeGen.Append("\";\r\n");
        }
        codeGen.Append("\r\n}\r\n\r\n");

        // Event Class Generation
        foreach (string eventName in keys)
        {
            List<EventParameterData> parameterData;
            if(eventData.TryGetValue(eventName, out parameterData))
            {
                if(parameterData.Count > 0)
                {
                    // begin class
                    string className = eventName + "Event";
                    codeGen.Append("public class ");
                    codeGen.Append(className);
                    codeGen.Append(" : CallbackEvent");
                    codeGen.Append("\r\n{\r\n");

                    for(int i = 0; i < parameterData.Count; i++)
                    {
                        EventParameterData data = parameterData[i];
                        codeGen.Append("    public ");
                        codeGen.Append(data.type);
                        codeGen.Append(" ");
                        codeGen.Append(data.name);
                        codeGen.Append(";\r\n");
                    }

                    codeGen.Append("    ");
                    codeGen.Append("public ");
                    codeGen.Append(className);
                    codeGen.Append("(");
                    for(int i = 0; i < parameterData.Count; i++)
                    {
                        EventParameterData data = parameterData[i];
                        codeGen.Append(data.type);
                        codeGen.Append(" a");
                        codeGen.Append(data.name);
                        if(i < parameterData.Count - 1)
                        {
                            codeGen.Append(", ");
                        }
                    }

                    codeGen.Append(")\r\n    {\r\n");
                    for (int i = 0; i < parameterData.Count; i++)
                    {
                        EventParameterData data = parameterData[i];
                        codeGen.Append("        ");
                        codeGen.Append(data.name);
                        codeGen.Append(" = a");
                        codeGen.Append(data.name);
                        codeGen.Append(";");
                        if(i < parameterData.Count - 1)
                        {
                            codeGen.Append("\r\n");
                        }
                    }
                    codeGen.Append("\r\n    }\r\n");

                    // end class
                    codeGen.Append("\r\n}\r\n\r\n");
                }
            }

        }

        eventData.Clear();

        return codeGen.ToString();
    }
}
#endif // UNITY_EDITOR