using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Dresser.Patches
{
	[HarmonyPatch]
	[HarmonyGadget("Dresser")]
	public static class Patch_ChunkWorld_CreateBaseShip
	{
		public static MethodBase moveNext = typeof(ChunkWorld).GetNestedType("<CreateBaseShip>c__Iterator1", BindingFlags.NonPublic).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);

		[HarmonyTargetMethod]
		public static MethodBase TargetMethod()
        {
			return moveNext;
        }

		[HarmonyPrefix]
		public static void Prefix()
		{
			InstanceTracker.GameScript.chunkWorldObj.GetComponent<ChunkWorld>().gridSpecial[13, 14] = Dresser.dresser.GetID();
		}
	}
}
