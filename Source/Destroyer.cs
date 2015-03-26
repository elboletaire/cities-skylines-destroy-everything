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
				Destroyer.DestroyAllBuildings();
			}
		}
	}

	public static class Destroyer
	{
		public static bool destroyEverything = false;

		static bool startedDestroying = false;
		static ushort latestDestroyed = 0;

		public static void DestroyAllBuildings()
		{
			if (startedDestroying) {
				return;
			}
			startedDestroying = true;

			ChirpLog.Info("Pressed destroyAll button");

			SimulationManager simManager = Singleton<SimulationManager>.instance;
			BuildingManager buildManager = Singleton<BuildingManager>.instance;
			AudioGroup nullAudioGroup = new AudioGroup(0, new SavedFloat("bulldoze", Settings.gameSettingsFile, 0, false));

			ChirpLog.Info("Current length: " + buildManager.m_buildings.m_buffer.Length);
			for (ushort b_index = latestDestroyed; b_index < buildManager.m_buildings.m_buffer.Length; b_index += 1) {
				Building build = buildManager.m_buildings.m_buffer[b_index];

				if (build.m_flags == Building.Flags.None) {
					continue;
				}

				ChirpLog.Info("Current building has flags:");
				ChirpLog.Info(build.m_flags.ToString());

				BuildingInfo info = build.Info;
				if (info.m_buildingAI.CheckBulldozing(b_index, ref build) != ToolBase.ToolErrors.None) {
					ChirpLog.Info("There was an error checking bulldozing");
					ChirpLog.Info(info.m_buildingAI.CheckBulldozing(b_index, ref build).ToString());
					continue;
				}

				int amount = GetBuildingRefundAmount(simManager, b_index, ref build, ref info);
				if (amount != 0) {
					Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, amount, info.m_class);
				}
				ChirpLog.Info("Building refund amount is (theorically): " + amount);

				Vector3 position = build.m_position;
				float angle = build.m_angle;
				int length = build.m_length;

				buildManager.ReleaseBuilding(b_index);

				EffectInfo effect = Singleton<BuildingManager>.instance.m_properties.m_bulldozeEffect;

				if (effect == null) {
					ChirpLog.Info("There's no effect for this building!");
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

				latestDestroyed = b_index;
				break;
			}

			if (latestDestroyed >= buildManager.m_buildings.m_buffer.Length) {
				destroyEverything = false;
				ChirpLog.Info("Finished destroying everything!");
			}
			startedDestroying = false;
			ChirpLog.Flush();
		}

		public static void DestroyBuilding(ref SimulationManager simManager, ref BuildingManager buildManager, ref AudioGroup nullAudioGroup, ushort index, ref Building build)
		{
			if (build.m_flags == Building.Flags.None) {
				return;
			}

			BuildingInfo info = build.Info;
			if (info.m_buildingAI.CheckBulldozing(index, ref build) != ToolBase.ToolErrors.None) {
				return;
			}

			int amount = GetBuildingRefundAmount(simManager, index, ref build, ref info);

			if (amount != 0) {
				ChirpLog.Info("Building refund amount is (theorically): " + amount);
				Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, amount, info.m_class);
			}

			Vector3 position = build.m_position;
			float angle = build.m_angle;
			int length = build.m_length;

			buildManager.ReleaseBuilding(index);

			EffectInfo effect = Singleton<BuildingManager>.instance.m_properties.m_bulldozeEffect;
			if (effect == null) {
				ChirpLog.Warning("There was no bulldozeEffect :\\");
				return;
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

		private static int GetBuildingRefundAmount(SimulationManager simManager, ushort index, ref Building building, ref BuildingInfo info)
		{
			if (simManager.IsRecentBuildIndex(index)) {
				return info.m_buildingAI.GetRefundAmount(index, ref building);
			}
			return 0;
		}
	}
}

