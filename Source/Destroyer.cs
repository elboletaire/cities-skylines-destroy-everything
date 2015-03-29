namespace DestroyEverythingYouCan
{
	using ColossalFramework;
	using ICities;
	using UnityEngine;

	public class DestroyEveryThingYouCanThread : ThreadingExtensionBase
	{
		public override void OnCreated(IThreading threading)
		{
			Destroyer.nullAudioGroup = new AudioGroup(0, new SavedFloat("Bulldoze", Settings.gameSettingsFile, 0, false));
			Destroyer.simManager = Singleton<SimulationManager>.instance;
			Destroyer.buildManager = Singleton<BuildingManager>.instance;
		}
	
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

		public static bool startedDestroying = false;

		public static AudioGroup nullAudioGroup;
		public static SimulationManager simManager;
		public static BuildingManager buildManager;

		static ushort currentBuffer = 0;

		public static void DestroyAll()
		{
			if (startedDestroying || !destroyEverything) {
				return;
			}
			startedDestroying = true;

			for (ushort i = currentBuffer; i <= buildManager.m_buildings.m_buffer.Length; i += 1) {
				if (i >= buildManager.m_buildings.m_buffer.Length) {
					destroyEverything = false;
					currentBuffer = 0;
					Debug.Log("Finished destroying everything!");
					break;
				}

				if (!DestroyBuilding(i)) {
					continue;
				}
				currentBuffer = i;
				break;
			}
			startedDestroying = false;
		}

		public static bool DestroyBuilding(ushort index)
		{
			Building building = buildManager.m_buildings.m_buffer[index];

			if (building.m_flags == Building.Flags.None) {
				return false;
			}
//			Debug.Log("Building flags are: " + build.m_flags);

			BuildingInfo info = building.Info;
			// We don't care about burning buildings
//			if (info.m_buildingAI.CheckBulldozing(index, ref build) != ToolBase.ToolErrors.None) {
//				Debug.Log(info.m_buildingAI.CheckBulldozing(index, ref build).ToString());
//				return false;
//			}

			// For now we're gonna remove just buildings. Later we'll remove everything :B
			if (info.m_placementStyle != ItemClass.Placement.Automatic) {
//				Debug.Log(info.m_class.ToString());
				return false;
			}

			int amount = info.m_buildingAI.GetRefundAmount(index, ref building);
			if (amount != 0) {
				Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, amount, info.m_class);
			}

			Vector3 position = building.m_position;
			float angle = building.m_angle;
			int length = building.m_length;

			buildManager.ReleaseBuilding(index);

			EffectInfo effect = Singleton<BuildingManager>.instance.m_properties.m_bulldozeEffect;

			if (effect == null) {
				return false;
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
//			var name = new InstanceID();
//			Debug.Log("Current building name is " + buildManager.GetBuildingName(index, name));
			Debug.Log("Building should be destroyed now...");

			return true;
		}
	}
}

