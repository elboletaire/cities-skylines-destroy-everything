using ColossalFramework;
using ICities;
using UnityEngine;
using ChirpLogger;

namespace DestroyEverythingYouCan
{
	public class DestroyEveryThingYouCanThread : ThreadingExtensionBase
	{
		public override void OnAfterSimulationTick()
		{
			if (Destroyer.destroyEverything) {
				Destroyer.DestroyAll();
			}
		}
	}

	public static class Destroyer
	{
		public static bool destroyEverything = false;

		static bool startedDestroying = false;

		public static void DestroyAll()
		{
			if (startedDestroying) {
				return;
			}
			startedDestroying = true;

			ChirpLog.Info("Pressed destroyAll button");

			SimulationManager simManager = Singleton<SimulationManager>.instance;
			BuildingManager buildManager = Singleton<BuildingManager>.instance;
			AudioGroup nullAudioGroup = new AudioGroup(0, new SavedFloat("bulldoze", Settings.gameSettingsFile, 0, false));

			for (ushort i = 0; i < buildManager.m_buildings.m_buffer.Length; i += 1) {
				ChirpLog.Info("Gonna check building with index " + i);
				if (i >= buildManager.m_buildings.m_buffer.Length) {
					continue;
				}
				Building build = buildManager.m_buildings.m_buffer[i];

				if (build.m_flags == Building.Flags.None) {
					continue;
				}

				BuildingInfo info = build.Info;
				if (info.m_buildingAI.CheckBulldozing(i, ref build) != ToolBase.ToolErrors.None) {
					continue;
				}

				int amount = GetBuildingRefundAmount(simManager, i, ref build, ref info);
				ChirpLog.Info("Building refund amount is (theorically): " + amount);
				if (amount != 0) {
					Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, amount, info.m_class);
				}

				Vector3 position = build.m_position;
				float angle = build.m_angle;
				int length = build.m_length;

				ChirpLog.Info("Building should be destroyed now...");
				buildManager.ReleaseBuilding(i);

				ChirpLog.Info("Building type: " + build.GetType().ToString());

				EffectInfo effect = Singleton<BuildingManager>.instance.m_properties.m_bulldozeEffect;

				if (effect == null) {
					continue;
				}

				var instance = new InstanceID();
				var spawnArea = new EffectInfo.SpawnArea(
					Matrix4x4.TRS(
						Building.CalculateMeshPosition(info, position, angle, length),
						Building.CalculateMeshRotation(angle),
						Vector3.one
					),
					info.m_lodMeshData
				);
				Singleton<EffectManager>.instance.DispatchEffect(effect, instance, spawnArea, Vector3.zero, 0.0f, 1f, nullAudioGroup);
			}
			startedDestroying = false;
			destroyEverything = false;
			ChirpLog.Info("Finished destroying everything!");
			ChirpLog.Flush();
		}

		private static int GetBuildingRefundAmount(SimulationManager simManager, ushort index, ref Building building, ref BuildingInfo info)
		{
			if (simManager.IsRecentBuildIndex(index)) {
				return info.m_buildingAI.GetRefundAmount(index, ref building);
			}
			return 0;
		}
	}
}

