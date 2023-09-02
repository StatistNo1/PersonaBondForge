namespace PersonaBondForge {
    [Verse.StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            new HarmonyLib.Harmony("StatistNo1.personabond").Patch(PersonaBondForge_Patch.PersonaBondForge_MethodBase(),
                prefix: new HarmonyLib.HarmonyMethod(typeof(PersonaBondForge_Patch).GetMethod("MakeRecipeProductsQuality")));
			AddBondedVersions(HelperClass.BondedThingDefs);
		}
		public static void AddBondedVersions(System.Collections.Generic.List<ThingDef_Weapon> allRangedWeapons) {
			foreach (ThingDef_Weapon bondedWeapon in allRangedWeapons) {
				Weapon_Attributes.Adapt_ThingDef(bondedWeapon);
                Weapon_Attributes.Adapt_RecipeDef(bondedWeapon);
			}
			HelperClass.updateDef(DefOf.PersonaBondForge_ThingDef_BondingBench);
		}
	}
	[RimWorld.DefOf]
	public static class DefOf {
		public static Verse.ThingDef PersonaBondForge_ThingDef_BondingBench;
		public static Verse.ThingDef PersonaBondForge_ThingDef_BondingCore;
		public static Verse.RecipeDef PersonaBondForge_RecipeDef_RerollTraits;
	}
	public static class HelperClass {
		public static System.Collections.Generic.List<ThingDef_Weapon> BondedThingDefs = new System.Collections.Generic.List<ThingDef_Weapon>();
		public static void updateDef(Verse.Def defToUpdate) {
			defToUpdate.ClearCachedData();
			defToUpdate.ResolveReferences();
		}
	}
}
