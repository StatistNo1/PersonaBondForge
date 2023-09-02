using System.Linq;

namespace PersonaBondForge {
    [Verse.StaticConstructorOnStartup]
    static class Fill_PersonaBondForge_RecipeDef_RerollTraits {
        static Fill_PersonaBondForge_RecipeDef_RerollTraits() {
            foreach (Verse.ThingDef td in Verse.DefDatabase<Verse.ThingDef>.AllDefs.Where(x => x.comps.Any(y => y is RimWorld.CompProperties_BladelinkWeapon))) {
                DefOf.PersonaBondForge_RecipeDef_RerollTraits.ingredients[0].filter.SetAllow(td, true);
                DefOf.PersonaBondForge_RecipeDef_RerollTraits.fixedIngredientFilter.SetAllow(td, true);
            }
            HelperClass.updateDef(DefOf.PersonaBondForge_RecipeDef_RerollTraits);
        }
    }
}
