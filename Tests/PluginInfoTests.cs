namespace GoodForBusiness.Tests
{
    public class PluginInfoTests
    {
        public static bool Guid_IsNotEmpty()
        {
            return !string.IsNullOrEmpty(GoodForBusiness.PluginInfo.GUID);
        }

        public static bool Name_IsNotEmpty()
        {
            return !string.IsNullOrEmpty(GoodForBusiness.PluginInfo.Name);
        }

        public static bool Version_IsNotEmpty()
        {
            return !string.IsNullOrEmpty(GoodForBusiness.PluginInfo.Version);
        }

        public static void RunAll()
        {
            if (!Guid_IsNotEmpty()) throw new System.Exception("GUID is empty");
            if (!Name_IsNotEmpty()) throw new System.Exception("Name is empty");
            if (!Version_IsNotEmpty()) throw new System.Exception("Version is empty");
        }
    }
}