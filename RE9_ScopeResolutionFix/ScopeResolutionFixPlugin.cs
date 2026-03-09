#nullable enable
using System;
using System.Collections.Generic;
using Hexa.NET.ImGui;
using REFrameworkNET.Callbacks;
using REFrameworkNET.Attributes;
using REFrameworkNET;
using REFrameworkNETPluginConfig;
using app;


namespace RE9_ScopeResolutionFix
{
	public class ScopeResolutionFixPlugin
	{
		#region PLUGIN_INFO

		/*PLUGIN INFO*/
		public const string PLUGIN_NAME = "RE9_ScopeResolutionFix";
		public const string COPYRIGHT = "";
		public const string COMPANY = "https://github.com/TonWonton/RE9_ScopeResolutionFix";

		public const string GUID = "RE9_ScopeResolutionFix";
		public const string VERSION = "1.0.0";

		public const string GUID_AND_V_VERSION = GUID + " v" + VERSION;

		#endregion



		/* VARIABLES */
		//Config
		private static Config _config = new Config(GUID);

		//Scope
		private static ConfigEntry<float> _scopeBaseZoomMultiplier = _config.Add("Scope base zoom multiplier", 2f);

		//References
		private static ScopeCameraControllerV3? _scopeCameraControllerV3;
		private static ADSCameraController? _adsCameraController;

		//Variables
		private static Dictionary<ADSCameraZoomData_Base.ZoomStepData, float> _zoomStepDataOriginalFOV = new Dictionary<ADSCameraZoomData_Base.ZoomStepData, float>();

		private static float _resetTextSize = 0f;
		public static float ResetTextSize
		{
			get
			{
				if (_resetTextSize != 0f) return _resetTextSize;
				else return _resetTextSize = ImGui.CalcTextSize("Reset").X;
			}
		}



		/* METHODS */
		private static bool TryGetOriginalFOVFromZoomStepData(ADSCameraZoomData_Base.ZoomStepData zoomStepData, out float originalFOV)
		{
			if (_zoomStepDataOriginalFOV.TryGetValue(zoomStepData, out float existingOriginalFOV))
			{
				originalFOV = existingOriginalFOV;
				return true;
			}
			else
			{
				originalFOV = zoomStepData._ZoomFov;
				if (originalFOV != 0f)
				{
					_zoomStepDataOriginalFOV[zoomStepData] = originalFOV;
					return true;
				}
			}

			return false;
		}

		private static float GetScopeFOV(float desiredFOV, int zoomStep)
		{
			float zoomDivisor = _scopeBaseZoomMultiplier.Value + 0.1f * zoomStep;
			return desiredFOV / zoomDivisor;
		}

		/* HOOKS */
		[MethodHook(typeof(ScopeCameraControllerV3), nameof(ScopeCameraControllerV3.setDisplayScope), MethodHookType.Pre)]
		public static PreHookResult PreScopeCameraSetupScopeInfo(Span<ulong> args)
		{
			if (_scopeCameraControllerV3 == null) _scopeCameraControllerV3 = ManagedObject.ToManagedObject(args[1]).TryAs<ScopeCameraControllerV3>();
			if (_scopeCameraControllerV3 != null)
			{
				ScopeCameraV3ParamUserData? paramUserData = _scopeCameraControllerV3._ParamUserData;
				if (paramUserData != null)
				{
					//Set the default image scale to 1x and set zoom rate to 0
					paramUserData._LensImageDefaultScale = 1f;
					paramUserData._LensImageZoomRate = 0f;
				}
			}

			//_isScope = true;
			return PreHookResult.Continue;
		}

		[MethodHook(typeof(ADSCameraController), nameof(ADSCameraController.updateFOV), MethodHookType.Pre)]
		public static PreHookResult PreADSCameraControllerUpdateFOV(Span<ulong> args)
		{
			if (_adsCameraController == null) _adsCameraController = ManagedObject.ToManagedObject(args[1]).TryAs<ADSCameraController>();
			if (_adsCameraController != null)
			{
				//Scope zoom
				ADSCameraController.ADSZoomData? adsZoomData = _adsCameraController.CurrentZoomData;
				if (adsZoomData != null)
				{
					int zoomStepsCount = adsZoomData._ZoomSteps.Count;
					for (int i = 0; i < zoomStepsCount; i++)
					{
						ADSCameraZoomData_Base.ZoomStepData zoomStepData = adsZoomData._ZoomSteps[i];
						if (zoomStepData != null)
						{
							if (TryGetOriginalFOVFromZoomStepData(zoomStepData, out float originalFOV))
							{
								zoomStepData._ZoomFov = GetScopeFOV(originalFOV, i);
							}
						}
					}
				}
			}

			return PreHookResult.Continue;
		}



		/* EVENT HANDLING */
		private static void OnSettingsChanged()
		{
			_config.SaveToJson();
		}



		/* PLUGIN LOAD */
		[PluginEntryPoint]
		private static void Load()
		{
			RegisterConfigEvents();
			_config.LoadFromJson();
			Log.Info("Loaded " + VERSION);
		}

		[PluginExitPoint]
		private static void Unload()
		{
			UnregisterConfigEvents();
			Log.Info("Unloaded " + VERSION);
		}

		private static void RegisterConfigEvents()
		{
			foreach (ConfigEntryBase configEntry in _config.Values)
			{
				configEntry.ValueChanged += OnSettingsChanged;
			}
		}

		private static void UnregisterConfigEvents()
		{
			foreach (ConfigEntryBase configEntry in _config.Values)
			{
				configEntry.ValueChanged -= OnSettingsChanged;
			}
		}


		/* PLUGIN GENERATED UI */

		[Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
		public static void PreImGuiDrawUI()
		{
			if (API.IsDrawingUI() && ImGui.TreeNode(GUID_AND_V_VERSION))
			{
				int labelNr = 0;

				//Scope
				ImGui.Text("SCOPE");
				ImGui.Separator();
				_scopeBaseZoomMultiplier.DrawDragFloat(0.01f, 0f, 10f); _scopeBaseZoomMultiplier.DrawResetButtonSameLine(ref labelNr);

				//End
				ImGui.TreePop();
			}
		}
	}

	public static class Log
	{
		private const string PREFIX = "[" + ScopeResolutionFixPlugin.GUID + "] ";
		public static void Info(string message)
		{
			API.LogInfo(PREFIX + message);
		}

		public static void Warning(string message)
		{
			API.LogWarning(PREFIX + message);
		}

		public static void Error(string message)
		{
			API.LogError(PREFIX + message);
		}
	}

	internal static class ImGuiExtensions
	{
		private static Dictionary<int, string> _resetButtonLabels = new Dictionary<int, string>();

		private static string TryGetResetButtonLabel(ref int labelNr)
		{
			if (_resetButtonLabels.TryGetValue(labelNr, out string? label) == false)
			{
				label = "Reset##" + labelNr;
				_resetButtonLabels[labelNr] = label;
			}

			labelNr++;
			return label;
		}

		public static bool DrawCheckbox(this ConfigEntry<bool> configEntry)
		{
			bool changed = ImGui.Checkbox(configEntry.Key, ref configEntry.RefValue);
			if (changed) configEntry.NotifyValueChanged();

			return changed;
		}

		public static bool DrawDragFloat(this ConfigEntry<float> configEntry, float vSpeed, float vMin, float vMax)
		{
			bool changed = ImGui.DragFloat(configEntry.Key, ref configEntry.RefValue, vSpeed, vMin, vMax);
			if (changed) configEntry.NotifyValueChanged();

			return changed;
		}

		public static bool DrawResetButtonSameLine(this ConfigEntryBase configEntry, ref int labelNr)
		{
			float buttonWidth = ScopeResolutionFixPlugin.ResetTextSize + ImGui.GetStyle().FramePadding.X * 2;

			ImGui.SameLine(ImGui.GetContentRegionAvail().X - buttonWidth);
			bool reset = ImGui.Button(TryGetResetButtonLabel(ref labelNr));
			if (reset) configEntry.Reset();

			return reset;
		}
	}
}