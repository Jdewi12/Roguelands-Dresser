using GadgetCore;
using GadgetCore.API;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Dresser.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Update")]
    [HarmonyGadget("Dresser")]
    public static class Patch_GameScript_Update
    {
        static int hovering = -1;

        static FieldInfo stuffSelecting = typeof(Menuu).GetField("stuffSelecting", BindingFlags.NonPublic | BindingFlags.Instance);

        const float leftSlotX = -11;
        const float topSlotY = 6.875f;
        const float slotSize = 2;
        const int slotsPerRow = 12;

        [HarmonyPrefix]
        public static void Prefix()
        {
            if(Dresser.currentUI != Dresser.CurrentUI.CLOSED)
            {
                if(GameScript.inventoryOpen && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit, 100))
                {
                    GameObject hit = raycastHit.transform.gameObject;
                    if(hit.layer == 22) // stuffSelect
                    {
                        if(int.TryParse(hit.name, out int slot)) // hovering slot
                        {
                            if(hovering != slot)
                            {
                                MoveHover(slot);
                                SoundHover();
                            }
                            if(Input.GetMouseButtonDown(0)) // clicked slot
                            {
                                SoundConfirm();
                                ICharacterFeatureRegistry characterFeatureRegistry;
                                if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
                                else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
                                else
                                    return;

                                int id;
                                if(characterFeatureRegistry.GetCurrentPage() == 1)
                                {
                                    id = slot;
                                }
                                else
                                {
                                    id = slot + characterFeatureRegistry.GetCurrentPage() * 24;
                                }

                                if(characterFeatureRegistry.TryGetEntryInterface(id, out _))
                                {
                                    if(characterFeatureRegistry.IsFeatureUnlocked(id))
                                    {
                                        if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                                            Menuu.curAugment = id;
                                        else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                                            Menuu.curUniform = id;
                                        MoveChosen(id);
                                        ChangeChar();
                                    }
                                }
                                else if(characterFeatureRegistry.GetCurrentPage() == 1)
                                {
                                    if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                                    {
                                        if(Menuu.unlockedAugment[slot] != 0)
                                        {
                                            Menuu.curAugment = slot;
                                            MoveChosen(slot);
                                            ChangeChar();
                                        }
                                    }
                                    else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                                    {
                                        if(Menuu.unlockedUniform[slot] != 0)
                                        {
                                            Menuu.curUniform = slot;
                                            MoveChosen(slot);
                                            ChangeChar();
                                        }
                                    }
                                }
                            }
                        }
                        else // somehow hovering stuff select but not any slots (not sure if possible)
                        {
                            UnHover();
                        }
                    }
                    else // not hovering stuff select
                    {
                        UnHover();
                        if(Input.GetMouseButtonDown(0))
                        {
                            switch(hit.name)
                            {
                                case "AUG":
                                    SoundConfirm();
                                    Dresser.menuDresser.gameObject.SetActive(false);
                                    Dresser.menuStuffSelect.gameObject.SetActive(true);
                                    Dresser.currentUI = Dresser.CurrentUI.AUG;
                                    stuffSelecting.SetValue(InstanceTracker.Menuu, 1);
                                    if(Menuu.curAugment < 24)
                                        CharacterAugmentRegistry.CurrentPage = 1;
                                    else
                                        CharacterAugmentRegistry.CurrentPage = Menuu.curAugment / 24;
                                    MoveChosen(Menuu.curAugment); // move chosen to the current augment
                                    SetDescription(Menuu.curAugment);
                                    UpdateBoxes();
                                    break;
                                case "UNIF":
                                    SoundConfirm();
                                    Dresser.menuDresser.gameObject.SetActive(false);
                                    Dresser.menuStuffSelect.gameObject.SetActive(true);
                                    Dresser.currentUI = Dresser.CurrentUI.UNIF;
                                    stuffSelecting.SetValue(InstanceTracker.Menuu, 2);
                                    if(Menuu.curUniform < 24)
                                        CharacterUniformRegistry.CurrentPage = 1;
                                    else
                                        CharacterUniformRegistry.CurrentPage = Menuu.curUniform / 24;
                                    MoveChosen(Menuu.curUniform); // move chosen to the current uniform
                                    SetDescription(Menuu.curUniform);
                                    UpdateBoxes();
                                    break;
                                case "bSelectorPageBack":
                                    SoundConfirm();
                                    if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                                    {
                                        if(CharacterAugmentRegistry.CurrentPage > 1)
                                        {
                                            CharacterAugmentRegistry.CurrentPage -= 1;
                                            MoveChosen(Menuu.curAugment);
                                        }
                                    }
                                    else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                                    {
                                        if(CharacterUniformRegistry.CurrentPage > 1)
                                        {
                                            CharacterUniformRegistry.CurrentPage -= 1;
                                            MoveChosen(Menuu.curUniform);
                                        }
                                    }
                                    UpdateBoxes();
                                    break;
                                case "bSelectorPageForward":
                                    SoundConfirm();
                                    if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                                    {
                                        if((CharacterAugmentRegistry.CurrentPage - 1) * 24 < CharacterAugmentRegistry.Singleton.Count())
                                        {
                                            CharacterAugmentRegistry.CurrentPage += 1;
                                            MoveChosen(Menuu.curAugment);
                                        }
                                    }
                                    else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                                    {
                                        if((CharacterUniformRegistry.CurrentPage - 1) * 24 < CharacterUniformRegistry.Singleton.Count())
                                        {
                                            CharacterUniformRegistry.CurrentPage += 1;
                                            MoveChosen(Menuu.curUniform);
                                        }
                                    }
                                    UpdateBoxes();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        static void SoundConfirm()
        {
            var a = InstanceTracker.GameScript.GetComponent<AudioSource>();
            if(a != null)
                a.PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
        }
        static void SoundHover()
        {
            var a = InstanceTracker.GameScript.GetComponent<AudioSource>();
            if(a != null)
                a.PlayOneShot((AudioClip)Resources.Load("Au/hover"), Menuu.soundLevel / 10f);
        }
        static void MoveHover(int slot)
        {
            if(hovering != slot)
            {
                hovering = slot;

                Vector2 slotPos = GetSlotPosition(slot);
                Dresser.hover.transform.localPosition = new Vector3(slotPos.x, slotPos.y, Dresser.hover.transform.localPosition.z);

                ICharacterFeatureRegistry characterFeatureRegistry;
                if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
                else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
                else
                    return;

                int id;
                if(characterFeatureRegistry.GetCurrentPage() == 1)
                    id = slot;
                else
                    id = slot + characterFeatureRegistry.GetCurrentPage() * 24;
                SetDescription(id);
            }
        }
        static void MoveChosen(int id)
        {
            ICharacterFeatureRegistry characterFeatureRegistry;
            if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
            else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
            else
                return;
            // if same page
            if((characterFeatureRegistry.GetCurrentPage() == 1 && id < 24) ||
                (id / 24 == characterFeatureRegistry.GetCurrentPage()))
            {
                int slot = id % 24;
                Vector2 slotPos = GetSlotPosition(slot);

                Dresser.chosen.transform.localPosition = new Vector3(slotPos.x, slotPos.y, Dresser.chosen.transform.localPosition.z);
            }
            else // if different page
            {
                Dresser.chosen.transform.localPosition = new Vector3(-10000, 0, Dresser.chosen.transform.localPosition.z);
            }
        }

        static Vector2 GetSlotPosition(int slot)
        {
            return new Vector2(
                leftSlotX + (slot % slotsPerRow) * slotSize,
                topSlotY - (slot / slotsPerRow /*integer division*/) * slotSize
            );
        }

        static void UnHover()
        {
            if(hovering != -1)
            {
                hovering = -1;
                Dresser.hover.transform.position = new Vector3(-10000, 0, Dresser.hover.transform.position.z);
                SetDescription((Dresser.currentUI == Dresser.CurrentUI.AUG) ? Menuu.curAugment : Menuu.curUniform);
            }
        }

        static void SetDescription(int id)
        {
            string name = "";
            string desc = "";
            string unlock = "";
            ICharacterFeatureRegistry characterFeatureRegistry;
            if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
            else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
            else
                return;

            if(characterFeatureRegistry.TryGetEntryInterface(id, out ICharacterFeatureRegistryEntry entry))
            {
                if(characterFeatureRegistry.IsFeatureUnlocked(id))
                {
                    name = entry.GetName();
                    desc = entry.GetDesc();
                    unlock = entry.GetUnlockCondition();
                }
                else
                {
                    name = entry.GetLockedName();
                    desc = entry.GetLockedDesc();
                    unlock = entry.GetLockedUnlockCondition();
                }
            }
            else if(id < 24) // vanilla
            {
                if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                {
                    if(Menuu.unlockedAugment[id] != 0)
                    {
                        name = InstanceTracker.Menuu.GetAugmentName(id);
                        desc = InstanceTracker.Menuu.GetAugmentDesc(id);
                    }
                    unlock = InstanceTracker.Menuu.GetAugmentUnlock(id);
                }
                else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
                {
                    if(Menuu.unlockedUniform[id] != 0)
                    {
                        name = InstanceTracker.Menuu.GetUniformName(id);
                        desc = InstanceTracker.Menuu.GetUniformDesc(id);
                    }
                    unlock = InstanceTracker.Menuu.GetUniformUnlock(id);
                }
            }
            else
            {
                int equippedIndex = (Dresser.currentUI == Dresser.CurrentUI.AUG) ? Menuu.curAugment : Menuu.curUniform;
                if(equippedIndex != id)
                {
                    SetDescription(equippedIndex);
                }
                else
                {
                    string currentUIName = (Dresser.currentUI == Dresser.CurrentUI.AUG) ? "augment" : "uniform";
                    Dresser.logger.LogConsole("Unknown currently-equipped " + currentUIName + " with id: " + id, GadgetConsole.MessageSeverity.WARN);
                }
                return;
            }
            Dresser.name.text = name;
            Dresser.name2.text = name;
            Dresser.desc.text = desc;
            Dresser.desc2.text = desc;
            Dresser.unlock.text = unlock;
            Dresser.unlock2.text = unlock;
        }

        static void UpdateBoxes()
        {
            ICharacterFeatureRegistry characterFeatureRegistry;
            if(Dresser.currentUI == Dresser.CurrentUI.AUG)
            {
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
                if(CharacterAugmentRegistry.Singleton.Count() == 0)
                {
                    Dresser.pageBack.gameObject.SetActive(false);
                    Dresser.pageForward.gameObject.SetActive(false);
                }
                else
                {
                    Dresser.pageBack.gameObject.SetActive(true);
                    Dresser.pageForward.gameObject.SetActive(true);
                }
            }
            else if(Dresser.currentUI == Dresser.CurrentUI.UNIF)
            {
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
                if(CharacterUniformRegistry.Singleton.Count() == 0)
                {
                    Dresser.pageBack.gameObject.SetActive(false);
                    Dresser.pageForward.gameObject.SetActive(false);
                }
                else
                {
                    Dresser.pageBack.gameObject.SetActive(true);
                    Dresser.pageForward.gameObject.SetActive(true);
                }
            }
            else
            {
                return;
            }

            if(characterFeatureRegistry.GetCurrentPage() == 1)
            {
                if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                    Dresser.mMenuStuffSelect.material = (Material)Resources.Load("mat/mUpgrade");
                else // UNIF
                    Dresser.mMenuStuffSelect.material = (Material)Resources.Load("mat/mUniform");
            }
            else
            {
                if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                    Dresser.mMenuStuffSelect.material = (Material)Resources.Load("mat/mAugmentBack");
                else // UNIF
                    Dresser.mMenuStuffSelect.material = (Material)Resources.Load("mat/mUniformBack");
            }


            for(int i = 0; i < Dresser.boxes.childCount; i++)
            {
                Transform box = Dresser.boxes.GetChild(i);
                if(int.TryParse(box.name, out int slot))
                {
                    box.gameObject.SetActive(true);
                    Transform icon = box.GetChild(0);
                    Transform background = icon.GetChild(0);

                    int id;
                    if(characterFeatureRegistry.GetCurrentPage() == 1)
                        id = slot;
                    else
                        id = slot + characterFeatureRegistry.GetCurrentPage() * 24;

                    if(characterFeatureRegistry.TryGetEntryInterface(id, out _))
                    {
                        Material mat;
                        if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                            mat = CharacterAugmentRegistry.Singleton.GetEntry(id).SelectorIconMat;
                        else // UNIF
                            mat = CharacterUniformRegistry.Singleton.GetEntry(id).SelectorIconMat;
                        icon.GetComponent<Renderer>().material = mat;
                        icon.GetComponent<Renderer>().enabled = true;
                        background.GetComponent<Renderer>().enabled = true;
                        box.GetComponent<Renderer>().enabled = !characterFeatureRegistry.IsFeatureUnlocked(id);
                    }
                    else if(characterFeatureRegistry.GetCurrentPage() == 1) // vanilla
                    {
                        bool unlocked;
                        if(Dresser.currentUI == Dresser.CurrentUI.AUG)
                            unlocked = Menuu.unlockedAugment[id] != 0;
                        else // UNIF)
                            unlocked = Menuu.unlockedUniform[id] != 0;
                        icon.GetComponent<Renderer>().enabled = false;
                        background.GetComponent<Renderer>().enabled = false;
                        box.GetComponent<Renderer>().enabled = !unlocked;
                    }
                    else
                    {
                        box.gameObject.SetActive(false);
                    }
                }
            }
        }

        static void ChangeChar()
        {
            PreviewLabs.PlayerPrefs.SetInt(Menuu.curChar + "uniform", Menuu.curUniform);
            PreviewLabs.PlayerPrefs.SetInt(Menuu.curChar + "augment", Menuu.curAugment);

            GameScript.equippedIDs[6] = Menuu.curUniform;
            GameScript.equippedIDs[7] = Menuu.curAugment;
            MenuScript.playerAppearance.GetComponent<PlayerAppearance>().UA(GameScript.equippedIDs, 0, GameScript.dead);
            MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
            {
                GameScript.equippedIDs,
                0,
                GameScript.dead
            });
            InstanceTracker.GameScript.UpdateHP();
            InstanceTracker.GameScript.UpdateMana();
            Dresser.menu.CloseMenu();
        }
    }
}
