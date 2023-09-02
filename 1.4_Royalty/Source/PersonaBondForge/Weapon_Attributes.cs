using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PersonaBondForge {
	public class ThingDef_Weapon : ThingDef {
		public ThingDef baseWeapon;
		public string baseWeaponDefName;

		public ThingDef_Weapon() {
			label = "PersonaBondForge_TempLabel" + HelperClass.BondedThingDefs.Count() + "A";
			HelperClass.BondedThingDefs.Add(this);
		}
        public override void PostLoad() {
			base.PostLoad();
			if (defName == "UnnamedDef") defName = "PBF_ThingDef_Weapon_" + baseWeaponDefName;
		}
    }
	public static class Weapon_Attributes {
		public static void Adapt_RecipeDef(ThingDef_Weapon bondedWeapon) {
			RecipeDef recipeDef             = DefDatabase<RecipeDef>.GetNamed("Make_" + bondedWeapon.defName);
			recipeDef.label					= "RecipeMake".Translate(bondedWeapon.label);
			recipeDef.jobString             = "RecipeMakeJobString".Translate(bondedWeapon.label);
			recipeDef.description           = "RecipeMakeDescription".Translate(bondedWeapon.label);
			bondedWeapon.costList.Add(new ThingDefCountClass(bondedWeapon.baseWeapon, 1));
			recipeDef.descriptionHyperlinks.Add(new DefHyperlink(bondedWeapon));
			recipeDef.descriptionHyperlinks.Add(DefOf.PersonaBondForge_ThingDef_BondingCore);
			RimWorld.RecipeDefGenerator.SetIngredients(recipeDef, bondedWeapon);
			HelperClass.updateDef(recipeDef);
		}
		public static void Adapt_ThingDef(ThingDef_Weapon bondedWeapon) {
			bondedWeapon.baseWeapon = DefDatabase<ThingDef>.GetNamed(bondedWeapon.baseWeaponDefName);
			Set_Attributes_ThingDef(bondedWeapon);
			HelperClass.updateDef(bondedWeapon);
		}
		public static void Set_Attributes_ThingDef(ThingDef_Weapon bondedWeapon) {
			bondedWeapon.recipeMaker.workSpeedStat = RimWorld.StatDefOf.GeneralLaborSpeed;
			bondedWeapon.equippedAngleOffset    = bondedWeapon.baseWeapon.equippedAngleOffset;
			bondedWeapon.drawGUIOverlay         = bondedWeapon.baseWeapon.drawGUIOverlay;
			bondedWeapon.alwaysHaulable         = bondedWeapon.baseWeapon.alwaysHaulable;
			bondedWeapon.equipmentType          = bondedWeapon.baseWeapon.equipmentType;
			bondedWeapon.selectable             = bondedWeapon.baseWeapon.selectable;
			bondedWeapon.soundInteract          = bondedWeapon.baseWeapon.soundInteract;
			bondedWeapon.thingClass             = bondedWeapon.baseWeapon.thingClass;
			bondedWeapon.tickerType             = bondedWeapon.baseWeapon.tickerType;
			bondedWeapon.BaseMarketValue        = bondedWeapon.baseWeapon.BaseMarketValue + DefOf.PersonaBondForge_ThingDef_BondingCore.BaseMarketValue;
			bondedWeapon.pathCost               = bondedWeapon.pathCost == 0 ? bondedWeapon.baseWeapon.pathCost : bondedWeapon.pathCost;
			bondedWeapon.allowedArchonexusCount = bondedWeapon.allowedArchonexusCount == 0 ? bondedWeapon.baseWeapon.allowedArchonexusCount : bondedWeapon.allowedArchonexusCount;
			bondedWeapon.relicChance            = bondedWeapon.relicChance == 0 ? bondedWeapon.baseWeapon.relicChance : bondedWeapon.relicChance;
			bondedWeapon.label                  = "OnboardPersonaLabel".Translate(bondedWeapon.baseWeapon.label);
			bondedWeapon.description            = "OnboardPersonaDescription".Translate(bondedWeapon.baseWeapon.description);
			bondedWeapon.weaponClasses          = bondedWeapon.weaponClasses ?? bondedWeapon.baseWeapon.weaponClasses;
			bondedWeapon.techLevel              = bondedWeapon.techLevel == RimWorld.TechLevel.Undefined ? bondedWeapon.baseWeapon.techLevel : bondedWeapon.techLevel;
			bondedWeapon.tools                  = bondedWeapon.tools ?? bondedWeapon.baseWeapon.tools;
			bondedWeapon.graphicData            = bondedWeapon.graphicData ?? bondedWeapon.baseWeapon.graphicData;
			bondedWeapon.graphic                = bondedWeapon.graphic.path.Equals("UI/Misc/BadTexture") ? bondedWeapon.baseWeapon.graphic : bondedWeapon.graphic;
			bondedWeapon.thingCategories        = new List<ThingCategoryDef>(bondedWeapon.baseWeapon.thingCategories);
			bondedWeapon.thingCategories = bondedWeapon.thingCategories.Count == 0 ? new List<ThingCategoryDef>(bondedWeapon.baseWeapon.thingCategories) : bondedWeapon.thingCategories;
			bondedWeapon.PostLoad(); //To set bondedWeapon.uiIcon
			if (bondedWeapon.Verbs.Count() == 0 && bondedWeapon.baseWeapon.Verbs.Count() > 0) bondedWeapon.Verbs.AddRange(bondedWeapon.baseWeapon.Verbs);
			//Adds StatModifier from the base weapon to the bonded weapon if those were not set for the bonded weapon
			List<RimWorld.StatDef> sd = new List<RimWorld.StatDef>(bondedWeapon.statBases.Select( e => e.stat));
			foreach (RimWorld.StatModifier sm in bondedWeapon.baseWeapon.statBases) if (!sd.Contains(sm.stat)) bondedWeapon.statBases.Add(sm);
		}
	}
}
