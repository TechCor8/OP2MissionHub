using OP2MissionHub.Data.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace OP2MissionHub.Data
{
	/// <summary>
	/// Tracks mission details in the cache.
	/// </summary>
	public class CacheDetails
	{
		private static List<MissionData> m_MissionList = new List<MissionData>();
		private static Dictionary<string, int> m_SDKReferences = new Dictionary<string, int>();

		public static ReadOnlyCollection<MissionData> missions	{ get { return m_MissionList.AsReadOnly();		} }
		public static string interopVersion						{ get; private set;								}


		/// <summary>
		/// Reads MissionData for each mission directory stored in the cached details.txt.
		/// </summary>
		public static void Initialize()
		{
			// Parse cached mission directories
			foreach (string directory in Directory.EnumerateDirectories(CachePath.GetCachePath()))
			{
				uint missionID;
				if (!uint.TryParse(new DirectoryInfo(directory).Name, out missionID))
					continue;

				// Register all details for mission
				string detailsPath = CachePath.GetMissionDetailsFilePath(missionID);
				if (!File.Exists(detailsPath))
				{
					// Folder is corrupted, delete it
					Directory.Delete(directory, true);
					continue;
				}
				string json = File.ReadAllText(detailsPath);
				MissionData localData = JsonUtility.FromJson<MissionData>(json);

				AddMissionData(localData);
			}
		}

		/// <summary>
		/// Registers mission data with cache details.
		/// Adds a reference to the mission's sdk version.
		/// </summary>
		public static void AddMissionData(MissionData missionData)
		{
			m_MissionList.Add(missionData);

			// Add reference to SDK version
			string sdkVersion = CachePath.GetSDKVersion(missionData);
			if (string.IsNullOrEmpty(sdkVersion))
				return;

			// Cached DotNetInterop is always the newest cached sdkVersion
			if (CachePath.IsNewerVersion(sdkVersion, interopVersion))
				interopVersion = sdkVersion;

			AddReference(sdkVersion);
			AddReference("DotNetInterop");
		}

		private static void AddReference(string key)
		{
			int references;
			if (m_SDKReferences.TryGetValue(key, out references))
				++references;

			m_SDKReferences[key] = references;
		}

		/// <summary>
		/// Unregisters mission data from cache details.
		/// If the mission's SDK version is no longer referenced, it will be deleted.
		/// </summary>
		public static void RemoveMissionData(uint missionID)
		{
			// Find and remove the mission
			int index = m_MissionList.FindIndex((mission) => mission.missionID == missionID);
			if (index < 0)
				return;

			MissionData missionData = m_MissionList[index];

			m_MissionList.RemoveAt(index);

			// Remove reference to SDK version
			string sdkVersion = CachePath.GetSDKVersion(missionData);
			if (string.IsNullOrEmpty(sdkVersion))
				return;

			if (RemoveReference(sdkVersion))		File.Delete(CachePath.GetSDKFilePath(sdkVersion));
			if (RemoveReference("DotNetInterop")) { File.Delete(CachePath.GetInteropFilePath()); interopVersion = null; }
		}

		private static bool RemoveReference(string key)
		{
			int references;
			if (m_SDKReferences.TryGetValue(key, out references))
				--references;
			
			m_SDKReferences[key] = references;

			// Should delete file
			return m_SDKReferences[key] <= 0;
		}
	}
}
