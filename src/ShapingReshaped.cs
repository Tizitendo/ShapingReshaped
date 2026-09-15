using BepInEx;
using BepInEx.Configuration;
using Logger;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ShapingReshaped;

[BepInDependency(RecalculateStatsAPI.PluginGUID)]
[BepInDependency(ItemAPI.PluginGUID)]
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public sealed class ShapingReshaped : BaseUnityPlugin
{
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Onyx";
    public const string PluginName = "ShapingReshaped";
    public const string PluginVersion = "1.0.0";

	public static ShapingReshaped Instance;
	public static ConfigEntry<int> InitialSoulCost { get; set; }
	public static ConfigEntry<int> DeathSoulCost { get; set; }
	public static ConfigEntry<float> ShrineWeightMultiplier { get; set; }
	public static ItemDef ShapingPermanentSoulCost;
	public static InteractableSpawnCard shrineShapingCard;

	public void Awake()
    {
		Log.Init(Logger);
		Instance = SingletonHelper.Assign(Instance, this);
		Options.Init();

		AssetAsyncReferenceManager<GameObject>.LoadAsset(new(RoR2_DLC2.ShrineColossusAccess_prefab)).Completed += (x) =>
		{
			if (x.Result.TryGetComponent(out PurchaseInteraction purchaseInteraction))
			{
				purchaseInteraction.cost = InitialSoulCost.Value;
			}
		};

		AssetAsyncReferenceManager<InteractableSpawnCard>.LoadAsset(new(RoR2_DLC2.iscShrineColossusAccess_asset)).Completed += (x) =>
		{
			shrineShapingCard = x.Result;
		};

		ShapingPermanentSoulCost = ScriptableObject.CreateInstance<ItemDef>();
		ShapingPermanentSoulCost.name = "ShapingPermanentSoulCost";
		ShapingPermanentSoulCost.nameToken = "SHAPINGPERMANENTSOULCOST_NAME";
		ShapingPermanentSoulCost.pickupToken = "SHAPINGPERMANENTSOULCOST_PICKUP";
		ShapingPermanentSoulCost.descriptionToken = "SHAPINGPERMANENTSOULCOST_DESC";
		ShapingPermanentSoulCost.loreToken = "SHAPINGPERMANENTSOULCOST_LORE";
		ShapingPermanentSoulCost.hidden = true;
		ShapingPermanentSoulCost.tier = ItemTier.NoTier;
		ItemAPI.Add(new CustomItem(ShapingPermanentSoulCost, new ItemDisplayRuleDict(null)));

		On.RoR2.CharacterMaster.TryReviveOnBodyDeath += TryReviveOnBodyDeath;
		On.RoR2.SceneDirector.GenerateInteractableCardSelection += GenerateInteractableCardSelection;
		RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
	}

	private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender || !sender.inventory)
				return;
			
			if (sender.inventory.GetItemCountEffective(ShapingPermanentSoulCost.itemIndex) > 0)
			{
				args.baseCurseAdd += (float)(sender.inventory.GetItemCountEffective(ShapingPermanentSoulCost.itemIndex) * DeathSoulCost.Value) / 100;
			}
        }

	private static WeightedSelection<DirectorCard> GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, SceneDirector self)
	{
		WeightedSelection<DirectorCard> result = orig(self);
		for(int i = 0; i < result.Count; i++)
		{
			WeightedSelection<DirectorCard>.ChoiceInfo choice = result.GetChoice(i);
			if(choice.value.spawnCard == shrineShapingCard)
			{
				result.ModifyChoiceWeight(i, choice.weight * ShrineWeightMultiplier.Value);
			}
		}
		return result;
	}

    private bool TryReviveOnBodyDeath(On.RoR2.CharacterMaster.orig_TryReviveOnBodyDeath orig, CharacterMaster self, CharacterBody body)
    {
		if (body.HasBuff(DLC2Content.Buffs.ExtraLifeBuff))
		{
			body.inventory.GiveItemPermanent(ShapingPermanentSoulCost);
		}
        return orig(self, body);
    }
}