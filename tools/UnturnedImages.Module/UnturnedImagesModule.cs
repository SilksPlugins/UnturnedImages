using SDG.Framework.Modules;
using SDG.Unturned;
using UnityEngine;
using UnturnedImages.Module.Images;
using UnturnedImages.Module.Patches;
using UnturnedImages.Module.UI;

namespace UnturnedImages.Module
{
    public class UnturnedImagesModule : MonoBehaviour, IModuleNexus
    {
        private readonly HarmonyPatches _harmonyPatches;
        private readonly UIManager _uiManager;

        public static UnturnedImagesModule? Instance { get; private set; }

        public GameObject? GameObject;

        public UnturnedImagesModule()
        {
            _harmonyPatches = new HarmonyPatches();
            _uiManager = new UIManager();
        }

        public void initialize()
        {
            UnturnedLog.info("Loading UnturnedImages Module");

            Instance = this;

            GameObject = new GameObject();
            DontDestroyOnLoad(GameObject);

            _harmonyPatches.Patch();
            CustomImageTool.Load();
            CustomVehicleTool.Load();
            _uiManager.Load();
        }

        public void shutdown()
        {
            UnturnedLog.info("Unloading UnturnedImages Module");

            Destroy(GameObject);

            _uiManager.Unload();
            CustomVehicleTool.Unload();
            CustomImageTool.Unload();
            _harmonyPatches.Unpatch();

            Instance = null;
        }
    }
}
