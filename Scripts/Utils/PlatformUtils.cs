using System.Text;

public static class PlatformUtils
{
    public static bool RemoveBackspaceFromPath(ref string aPath)
    {
        StringBuilder path = new StringBuilder(aPath);
        bool changedPath = false;
        for (int j = 0; j < path.Length; ++j)
        {
            if (path[j] == '\\')
            {
                path[j] = '/';
                changedPath = true;
            }
        }

        if (changedPath)
        {
            aPath = path.ToString();
        }

        return changedPath;
    }
}
