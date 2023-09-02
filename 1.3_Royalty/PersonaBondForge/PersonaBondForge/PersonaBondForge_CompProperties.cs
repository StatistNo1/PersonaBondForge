using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PersonaBondForge {
    [Verse.StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            new HarmonyLib.Harmony("StatistNo1.PersonaBondForge").Patch(PersonaBondForge_Patch.PersonaBondForge_MethodBase(),
                prefix: new HarmonyLib.HarmonyMethod(typeof(PersonaBondForge_Patch).GetMethod("MakeRecipeProductsQuality")));
        }
    }

    [RimWorld.DefOf]
    public static class DefOf {
        public static Verse.ThingDef PersonaBondForge_ThingDef_BondingBench;
        public static Verse.ThingDef PersonaBondForge_ThingDef_BondingCore;
    }
    public class PersonaBondForge_Patch {
        public static System.Reflection.MethodBase PersonaBondForge_MethodBase() {
            return HarmonyLib.AccessTools.Method(typeof(Verse.GenRecipe), "MakeRecipeProducts");
		}
        public static bool MakeRecipeProductsQuality(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver, Precept_ThingStyle precept = null) {
			System.Collections.Generic.List<Thing> productsToReturn = new List<Thing>();
			Building_WorkTable building_WorkTable = billGiver as Building_WorkTable;
			if (building_WorkTable?.def != DefOf.PersonaBondForge_ThingDef_BondingBench)
				return true;
			RimWorld.QualityCategory qc = RimWorld.QualityCategory.Awful;
			float efficiency = ((recipeDef.efficiencyStat != null) ? worker.GetStatValue(recipeDef.efficiencyStat) : 1f);
			if (recipeDef.workTableEfficiencyStat != null) {
				if (building_WorkTable != null) {
					efficiency *= building_WorkTable.GetStatValue(recipeDef.workTableEfficiencyStat);
				}
			}
			if (recipeDef.products != null) {
				for (int j = 0; j < recipeDef.products.Count; j++) {
					ThingDefCountClass thingDefCountClass = recipeDef.products[j];
					Thing thing = ThingMaker.MakeThing(stuff: (!thingDefCountClass.thingDef.MadeFromStuff) ? null : dominantIngredient.def, def: thingDefCountClass.thingDef);
					thing.stackCount = UnityEngine.Mathf.CeilToInt((float)thingDefCountClass.count * efficiency);
					if (dominantIngredient != null && recipeDef.useIngredientsForColor) {
						thing.SetColor(dominantIngredient.DrawColor, reportFailure: false);
					}
					CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
					if (compIngredients != null) {
						for (int l = 0; l < ingredients.Count; l++) {
							compIngredients.RegisterIngredient(ingredients[l].def);
						}
					}
					CompFoodPoisonable compFoodPoisonable = thing.TryGetComp<CompFoodPoisonable>();
					if (compFoodPoisonable != null) {
						if (Rand.Chance(worker.GetRoom()?.GetStat(RoomStatDefOf.FoodPoisonChance) ?? RoomStatDefOf.FoodPoisonChance.roomlessScore)) {
							compFoodPoisonable.SetPoisoned(FoodPoisonCause.FilthyKitchen);
						} else if (Rand.Chance(worker.GetStatValue(StatDefOf.FoodPoisonChance))) {
							compFoodPoisonable.SetPoisoned(FoodPoisonCause.IncompetentCook);
						}
					}
					foreach (Verse.Thing t in ingredients) {
						if (t.def != DefOf.PersonaBondForge_ThingDef_BondingCore)
							t.TryGetQuality(out qc);
					}
					Verse.Log.Message("Persona Bond Forge Fix: Attempting to create something." +
						"\nRecipeDef: " + recipeDef.defName +
						"\nProduct: " + recipeDef.products[0] +
						"\nQuality: " + qc +
						"\nWorktable: " + building_WorkTable +
						"\nBillgiver: " + billGiver.LabelShort);
					productsToReturn.Add(PostProcessProductQuality(qc, thing, recipeDef, worker, precept));
				}
			}
			__result = productsToReturn.AsEnumerable();
			return false;
		}
		public static Thing PostProcessProductQuality(RimWorld.QualityCategory qc, Thing product, RecipeDef recipeDef, Pawn worker, Precept_ThingStyle precept = null) {
			CompQuality compQuality = product.TryGetComp<CompQuality>();
			if (compQuality != null) {
				if (recipeDef.workSkill == null) {
					Log.Error(string.Concat(recipeDef, " needs workSkill because it creates a product with a quality."));
				}
				QualityCategory q = qc;
				compQuality.SetQuality(q, ArtGenerationContext.Colony);
				QualityUtility.SendCraftNotification(product, worker);
			}
			CompArt compArt = product.TryGetComp<CompArt>();
			if (compArt != null) {
				compArt.JustCreatedBy(worker);
				if (compQuality != null && (int)compQuality.Quality >= 4) {
					TaleRecorder.RecordTale(TaleDefOf.CraftedArt, worker, product);
				}
			}
			if (worker.Ideo != null) {
				product.StyleDef = worker.Ideo.GetStyleFor(product.def);
			}
			if (precept != null) {
				product.StyleSourcePrecept = precept;
			}
			if (product.def.Minifiable) {
				product = product.MakeMinified();
			}
			return product;
		}
	}
}
