namespace DP.Base.Reflection
{
    public static class AssemblyNameHelper
    {
        public static void GetAssemblyNameParts(string assemblyQualifiedTypeName, out string typeName, out string assemblyName)
        {
            int index = -1;
            int bcount = 0;
            for (int i = 0; i < assemblyQualifiedTypeName.Length; ++i)
            {
                if (assemblyQualifiedTypeName[i] == '[')
                {
                    ++bcount;
                }
                else if (assemblyQualifiedTypeName[i] == ']')
                {
                    --bcount;
                }
                else if (bcount == 0 && assemblyQualifiedTypeName[i] == ',')
                {
                    index = i;
                    break;
                }
            }

            int nextCommaIndex = assemblyQualifiedTypeName.IndexOf(",", index + 1);

            if (nextCommaIndex == -1)
            {
                assemblyName = assemblyQualifiedTypeName.Substring(index + 1).Trim();
            }
            else
            {
                assemblyName = assemblyQualifiedTypeName.Substring(index + 1, nextCommaIndex - (index + 1)).Trim();
            }

            typeName = assemblyQualifiedTypeName.Substring(0, index).Trim();
        }
    }
}
