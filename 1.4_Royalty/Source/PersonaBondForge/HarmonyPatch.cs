using System.Linq;
using Verse;
using RimWorld;

namespace PersonaBondForge {
	public class PersonaBondForge_Patch {
		public static System.Reflection.MethodBase PersonaBondForge_MethodBase() {
			return HarmonyLib.AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts");
		}
		public static bool MakeRecipeProductsQuality(ref System.Collections.Generic.IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, System.Collections.Generic.List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver, Precept_ThingStyle precept = null) {
			System.Collections.Generic.List<Thing> productsToReturn = new System.Collections.Generic.List<Thing>();
			Building_WorkTable building_WorkTable = billGiver as Building_WorkTable;
			if (building_WorkTable?.def != DefOf.PersonaBondForge_ThingDef_BondingBench)
				return true;
			QualityCategory qc = RimWorld.QualityCategory.Awful;
			float efficiency = ((recipeDef.efficiencyStat != null) ? worker.GetStatValue(recipeDef.efficiencyStat) : 1f);
			if (recipeDef.workTableEfficiencyStat != null) {
				if (building_WorkTable != null) {
					efficiency *= building_WorkTable.GetStatValue(recipeDef.workTableEfficiencyStat);
				}
			}
			Pawn bondedPawn = null;
			if (recipeDef.products != null) {
				for (int j = 0; j < recipeDef.products.Count; j++) {
					ThingDefCountClass thingDefCountClass = new ThingDefCountClass();
					if (recipeDef == DefOf.PersonaBondForge_RecipeDef_RerollTraits) {
						thingDefCountClass.thingDef = ingredients[0].def;
						thingDefCountClass.count = 1;
						bondedPawn = ingredients.First(x => x.def != ThingDefOf.Luciferium).TryGetComp<CompBiocodable>().CodedPawn;
					} else
						thingDefCountClass = recipeDef.products[j];
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
						if (t.def != DefOf.PersonaBondForge_ThingDef_BondingCore && t.def != RimWorld.ThingDefOf.Luciferium)
							t.TryGetQuality(out qc);
					}
					if (Prefs.LogVerbose)
						Verse.Log.Message("Persona Bond Forge: Attempting to create something." +
							"\nRecipeDef: " + recipeDef.defName +
							"\nProduct: " + recipeDef.products[0] +
							"\nQuality: " + qc +
							"\nWorktable: " + building_WorkTable +
							"\nBillgiver: " + billGiver.LabelShort);
					productsToReturn.Add(PostProcessProductQuality(qc, thing, recipeDef, worker, precept));
				}
			}
			if (recipeDef == DefOf.PersonaBondForge_RecipeDef_RerollTraits && bondedPawn != null)
				productsToReturn[0].TryGetComp<CompBiocodable>().Notify_Equipped(bondedPawn);
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
