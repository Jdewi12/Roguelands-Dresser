using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Dresser.Scripts
{
    class DresserUIScript : MonoBehaviour
    {
        private void OnEnable()
        {
            if (Dresser.dresserInstance != gameObject)
            {
                Dresser.menuDresser = transform.Find("menuDresser");
                Dresser.menuStuffSelect = transform.Find("menuStuffSelect");
                Dresser.mMenuStuffSelect = Dresser.menuStuffSelect.GetComponent<MeshRenderer>();
                Dresser.boxes = Dresser.menuStuffSelect.Find("boxes");
                Dresser.pageBack = Dresser.boxes.Find("bSelectorPageBack")?.GetComponent<ButtonMenu>();
                Dresser.pageForward = Dresser.boxes.Find("bSelectorPageForward")?.GetComponent<ButtonMenu>();
                Dresser.dresserInstance = gameObject;
                Dresser.hover = Dresser.boxes.Find("hover");
                Dresser.hover.transform.position = new Vector3(-10000, 0, Dresser.hover.transform.position.z);
                Dresser.chosen = Dresser.boxes.Find("chosen");
                Dresser.name = Dresser.menuStuffSelect.Find("NAME").GetComponent<TextMesh>();
                Dresser.name2 = Dresser.name.transform.GetChild(0).GetComponent<TextMesh>();
                Dresser.unlock = Dresser.menuStuffSelect.Find("UNLOCK").GetComponent<TextMesh>();
                Dresser.unlock2 = Dresser.unlock.transform.GetChild(0).GetComponent<TextMesh>();
                Dresser.desc = Dresser.menuStuffSelect.Find("DESC").GetComponent<TextMesh>();
                Dresser.desc2 = Dresser.desc.transform.GetChild(0).GetComponent<TextMesh>();
                //Dresser.menuTitle = Dresser.menuDresser.Find("TITLE").GetComponent<TextMesh>();
                //Dresser.menuTitle2 = Dresser.menuTitle.transform.GetChild(0).GetComponent<TextMesh>();
                Dresser.menuAug = Dresser.menuDresser.Find("AUG").GetChild(0).GetComponent<TextMesh>();
                Dresser.menuAug2 = Dresser.menuAug.transform.GetChild(0).GetComponent<TextMesh>();
                Dresser.menuUnif = Dresser.menuDresser.Find("UNIF").GetChild(0).GetComponent<TextMesh>();
                Dresser.menuUnif2 = Dresser.menuUnif.transform.GetChild(0).GetComponent<TextMesh>();
            }
            Dresser.currentUI = Dresser.CurrentUI.MAIN;
            Dresser.menuDresser.gameObject.SetActive(true);
            Dresser.menuStuffSelect.gameObject.SetActive(false);
            Dresser.menuAug.text = "Augment: " + InstanceTracker.Menuu.GetAugmentName(Menuu.curAugment);
            Dresser.menuAug2.text = Dresser.menuAug.text;
            Dresser.menuUnif.text = "Uniform: " + InstanceTracker.Menuu.GetUniformName(Menuu.curUniform);
            Dresser.menuUnif2.text = Dresser.menuUnif.text;
        }

        private void OnDisable()
        {
            Dresser.currentUI = Dresser.CurrentUI.CLOSED;
        }

        
    }
}
