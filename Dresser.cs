using UnityEngine;
using GadgetCore.API;
using System.Collections.Generic;
using System.Collections;
using Dresser.Scripts;
using ScrapYard.API;
using System.Reflection;
using GadgetCore.Util;

namespace Dresser
{
    [Gadget("Dresser", RequiredOnClients: true, Dependencies: new string[] { "ScrapYard" }, LoadAfter: new string[] { "ScrapYard" })]
    public class Dresser : Gadget 
    {
        public const string MOD_VERSION = "1.2";
        public const string CONFIG_VERSION = "1.1"; // Increment whenever config format changes.

        public static GadgetCore.GadgetLogger logger;

        public AssetBundle assetBundle;

        public static void Log(string text)
        {
            logger.Log(text);
        }

        protected override void PrePatch()
        {
            logger = base.Logger;
        }

        protected override void Initialize()
        {
            Logger.Log("Dresser v" + Info.Mod.Version);

            /* Uncomment to register a ton of test races/uniforms/augments to test the UI.
             * Each page can hold 24 races so this should display two full pages plus one entry on the third page, for each menu
            for (int i = 0; i < 49; i++)
            {
                CharacterRaceInfo testRace = new CharacterRaceInfo("Dresser Test Race" + i, "This RACE should not be in release", "testRACE", EquipStats.ONE, Resources.Load<Material>("i/i1"), Resources.Load<Material>("i/i1"));
                testRace.Register();
                testRace.SetFeatureUnlocked();
                CharacterAugmentInfo testAugment = new CharacterAugmentInfo("Dresser Test Augment" + i, "This AUGMENT should not be in release", "testAUG", Resources.Load<Material>("i/i1"), Resources.Load<Material>("i/i1"));
                testAugment.Register();
                testAugment.SetFeatureUnlocked();
                CharacterUniformInfo testUniform = new CharacterUniformInfo("Dresser Test Uniform" + i, "This UNIFORM should not be in release", "testUNIF", Resources.Load<Material>("i/i1"), Resources.Load<Material>("i/i1"));
                testUniform.Register();
                testUniform.SetFeatureUnlocked();
            }
            */


            assetBundle = GadgetCoreAPI.LoadAssetBundle("dresser");

            Texture2D dresserTex = GadgetCoreAPI.LoadTexture2D("Dresser.png");
            Texture2D dresserItemTex = GadgetCoreAPI.LoadTexture2D("DresserItem.png");

            ItemInfo dresserItem = new ItemInfo(ItemType.GENERIC, "Dresser", "", dresserItemTex, 0);
            dresserItem.Register("Dresser");

            GameObject dresserGO = GameObject.Instantiate((GameObject)Resources.Load("prop/2501"));
            dresserGO.transform.GetChild(0).transform.rotation = Quaternion.Euler(180,0,0);
            dresserGO.transform.GetChild(0).transform.position += new Vector3(0,0,0.1f);
            dresserGO.name = "Dresser";
            var rend = dresserGO.transform.FindChild("texture").GetComponent<Renderer>();
            rend.material = new Material(rend.material);
            rend.material.mainTexture = dresserTex;
            BoxCollider col = dresserGO.AddComponent<BoxCollider>();
            col.size = new Vector3(3, 3, 1);
            col.isTrigger = true;
            dresser = new TileInfo(TileType.INTERACTIVE, dresserTex, dresserGO, dresserItem);
            dresser.Register("Dresser");
            GameObject dresserUI = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>("Dresser"));
            dresserUI.AddComponent<DresserUIScript>(); // DresserScript's OnEnable resolves references to boxes, etc.

            for (int i = 0; i < boxes.childCount; i++)
            {
                GameObject box = boxes.GetChild(i).gameObject;
                if (int.TryParse(box.name, out int _))
                {
                    GameObject boxIcon = GameObject.Instantiate(box);
                    boxIcon.name = "Icon";
                    boxIcon.transform.SetParent(box.transform);
                    boxIcon.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                    boxIcon.transform.localPosition = new Vector3(0, 0, -0.025f);
                    Renderer r = boxIcon.GetComponent<Renderer>();
                    r.enabled = true;
                    r.material = null;
                    GameObject backgroundObject = GameObject.Instantiate(boxIcon);
                    backgroundObject.GetComponent<Renderer>().material = (Material)Resources.Load("mat/mUniformSlot");
                    GameObject.DestroyImmediate(backgroundObject.GetComponent<BoxCollider>());
                    backgroundObject.name = "Background";
                    backgroundObject.transform.SetParent(boxIcon.transform);
                    backgroundObject.transform.localScale = Vector3.one;
                    backgroundObject.transform.localRotation = Quaternion.identity;
                    backgroundObject.transform.localPosition = new Vector3(0, 0, -0.025f);

                }
            }

            Material buttonMat = assetBundle.LoadAsset<Material>("butonnnMat");
            Material buttonMat2 = assetBundle.LoadAsset<Material>("buttonselectMat");
            var menuAugBtn = menuAug.transform.parent.gameObject.AddComponent<AudioSource>().gameObject.AddComponent<ButtonMenu>();
            menuAugBtn.minorButton = true;
            menuAugBtn.button = buttonMat;
            menuAugBtn.buttonSelect = buttonMat2;
            var menuUnifBtn = menuUnif.transform.parent.gameObject.AddComponent<AudioSource>().gameObject.AddComponent<ButtonMenu>();
            menuUnifBtn.minorButton = true;
            menuUnifBtn.button = buttonMat;
            menuUnifBtn.buttonSelect = buttonMat2;

            var SceneInjector = typeof(GadgetCoreAPI).Assembly.GetType("GadgetCore.SceneInjector");
            var LeftArrow = (Material)SceneInjector.GetProperty("LeftArrow", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            var LeftArrow2 = (Material)SceneInjector.GetProperty("LeftArrow2", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            var RightArrow = (Material)SceneInjector.GetProperty("RightArrow", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            var RightArrow2 = (Material)SceneInjector.GetProperty("RightArrow2", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            pageBack = GameObject.Instantiate(boxes.GetChild(0).gameObject).AddComponent<AudioSource>().gameObject.AddComponent<ButtonMenu>();
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in pageBack.transform)
                children.Add(child.gameObject);
            foreach (GameObject child in children)
                GameObject.DestroyImmediate(child);
            pageBack.name = "bSelectorPageBack";
            pageBack.transform.SetParent(boxes);
            pageBack.transform.localScale = new Vector3(10, 10, 10);
            pageBack.transform.localPosition = new Vector3(-14f, 3.85f, -2.7f);
            pageBack.gameObject.layer = 0;
            pageBack.GetComponent<BoxCollider>().size = new Vector3(0.15f, 0.15f, 0.1f);
            pageBack.GetComponent<MeshRenderer>().enabled = true;
            pageBack.GetComponent<MeshRenderer>().material = LeftArrow;
            pageBack.minorButton = true;
            pageBack.button = LeftArrow;
            pageBack.buttonSelect = LeftArrow2;
            pageForward = GameObject.Instantiate(pageBack).GetComponent<ButtonMenu>();
            pageForward.name = "bSelectorPageForward";
            pageForward.transform.SetParent(boxes);
            pageForward.transform.localPosition = new Vector3(14f, 3.85f, -2.7f);
            pageForward.GetComponent<MeshRenderer>().material = RightArrow;
            pageForward.button = RightArrow;
            pageForward.buttonSelect = RightArrow2;

            menu = new MenuInfo(MenuType.EXCLUSIVE, dresserUI, dresser);
            menu.Register("Dresser");

            ShopPlatform.DefaultObjects.AddShopPlatformEntry(new ShopPlatformEntry(dresserItem.GetID(), 100));


        }

        internal static TileInfo dresser;

        internal static MenuInfo menu;

        internal static GameObject dresserInstance;
        internal enum CurrentUI
        {
            MAIN,
            AUG,
            UNIF,
            CLOSED
        }
        internal static CurrentUI currentUI = CurrentUI.CLOSED;
        internal static Transform menuDresser;
        internal static Transform menuStuffSelect;

        internal static Transform boxes;
        internal static Transform hover;
        internal static Transform chosen;

        internal static TextMesh name;
        internal static TextMesh name2;
        internal static TextMesh unlock;
        internal static TextMesh unlock2;
        internal static TextMesh desc;
        internal static TextMesh desc2;

        internal static TextMesh menuAug;
        internal static TextMesh menuAug2;
        internal static TextMesh menuUnif;
        internal static TextMesh menuUnif2;

        internal static MeshRenderer mMenuStuffSelect;

        internal static Material mUpgrade = (Material)Resources.Load("mat/mUpgrade");
        internal static Material mUniform = (Material)Resources.Load("mat/mUniform");

        internal static ButtonMenu pageBack;
        internal static ButtonMenu pageForward;
    }
}