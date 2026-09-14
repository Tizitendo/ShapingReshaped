using BepInEx.Bootstrap;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ShapingReshaped;

static class Options
{
	public static bool IsEnabled => Chainloader.PluginInfos.ContainsKey(RiskOfOptions.PluginInfo.PLUGIN_GUID);

	public static void Init()
	{
		ShapingReshaped.InitialSoulCost = ShapingReshaped.Instance.Config.Bind("General", "Initial Soul Cost", 20, "How much Soul (curse) you pay for activating the shaping shrine (only use intervals of 10)");
		ShapingReshaped.DeathSoulCost = ShapingReshaped.Instance.Config.Bind("General", "Death Soul Cost", 30, "How much permanent Soul (curse) you pay for dying after being revived with the shrine");
		ShapingReshaped.ShrineWeightMultiplier = ShapingReshaped.Instance.Config.Bind("General", "Shrine Weight Multiplier", 0.5f, "Shrine weight is multiplied by this value");
		if (Options.IsEnabled)
		{
			RiskOfOptionsConfig();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public static void RiskOfOptionsConfig() {
		const string MOD_GUID = ShapingReshaped.PluginGUID;
		const string MOD_NAME = ShapingReshaped.PluginName;

		ModSettingsManager.AddOption(new IntSliderOption(ShapingReshaped.InitialSoulCost), MOD_GUID, MOD_NAME);
		ModSettingsManager.AddOption(new IntSliderOption(ShapingReshaped.DeathSoulCost), MOD_GUID, MOD_NAME);
		ModSettingsManager.AddOption(new SliderOption(ShapingReshaped.ShrineWeightMultiplier, new SliderConfig
		{
			min = 0,
			max = 2
		}), MOD_GUID, MOD_NAME);

		ModSettingsManager.SetModDescription($"Options for {MOD_NAME}", MOD_GUID, MOD_NAME);

		FileInfo iconFile = null;
		DirectoryInfo dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
		do
		{
			FileInfo[] files = dir.GetFiles("icon.png", SearchOption.TopDirectoryOnly);
			if (files != null && files.Length > 0)
			{
				iconFile = files[0];
				break;
			}

			dir = dir.Parent;
		} while (dir != null && dir.Exists && !string.Equals(dir.Name, "plugins", StringComparison.OrdinalIgnoreCase));

		if (iconFile != null)
		{
			Texture2D iconTexture = new Texture2D(256, 256);
			if (iconTexture.LoadImage(File.ReadAllBytes(iconFile.FullName)))
			{
				Sprite iconSprite = Sprite.Create(iconTexture, new Rect(0f, 0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
				iconSprite.name = $"{MOD_NAME}Icon";

				ModSettingsManager.SetModIcon(iconSprite, MOD_GUID, MOD_NAME);
			}
		}
	}
}