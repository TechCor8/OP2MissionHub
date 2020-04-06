using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using OP2MissionHub.Data.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OP2MissionHub.Data
{
	public class CachePath
	{
		public const string DotNetInteropFileName = "DotNetInterop.dll";


		public static bool IsNewerVersion(string requestedVersion, string currentVersion)
		{
			if (string.IsNullOrEmpty(currentVersion)) return true;

			List<string> requestedDots = new List<string>(requestedVersion.Split('.'));
			List<string> currentDots = new List<string>(currentVersion.Split('.'));

			// Make sure number of version components is the same
			while (requestedDots.Count < currentDots.Count)
				requestedDots.Add("0");
			while (currentDots.Count < requestedDots.Count)
				currentDots.Add("0");

			for (int i=0; i < requestedDots.Count; ++i)
			{
				int requestedDot;
				int currentDot;
				if (!int.TryParse(requestedDots[i], out requestedDot))	requestedDot = 0;
				if (!int.TryParse(currentDots[i], out currentDot))		currentDot = 0;

				if (requestedDot > currentDot)
					return true;
				else if (requestedDot < currentDot)
					return false;
			}

			// Versions are the same
			return false;
		}

		public static string GetCachePath()
		{
			if (Application.isEditor)
				return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Cache");

			return Path.Combine(Application.dataPath, "Cache");
		}

		public static string GetSDKFilePath(string sdkVersion)
		{
			string sdkFileName = GetSDKFileName(sdkVersion);
			if (!string.IsNullOrEmpty(sdkFileName))
				return Path.Combine(GetSDKDirectory(), sdkFileName);

			return null;
		}

		public static string GetSDKDirectory()
		{
			return Path.Combine(GetCachePath(), "DotNetMissionSDK");
		}

		public static string GetSDKFileName(string sdkVersion)
		{
			if (!string.IsNullOrEmpty(sdkVersion))
				return "DotNetMissionSDK_v" + sdkVersion.Replace(".", "_") + ".dll";

			return null;
		}

		public static string GetInteropFilePath()
		{
			return Path.Combine(GetSDKDirectory(), DotNetInteropFileName);
		}

		// Returns the SDK version specified in the .OPM file. Otherwise, returns null
		public static string GetSDKVersion(MissionData missionData)
		{
			string missionPath = Path.Combine(GetCachePath(), missionData.missionID.ToString());

			// DLL or OPM
			string dllName = missionData.fileNames.FirstOrDefault((filename) => filename.IndexOf(".dll", System.StringComparison.OrdinalIgnoreCase) >= 0);
			string opmName = missionData.fileNames.FirstOrDefault((filename) => filename.IndexOf(".opm", System.StringComparison.OrdinalIgnoreCase) >= 0);
			bool isStandardMission = string.IsNullOrEmpty(dllName) && !string.IsNullOrEmpty(opmName);
			
			if (isStandardMission)
			{
				// Get the SDK version from the mission file
				try
				{
					string filePath = Path.Combine(missionPath, opmName);
					MissionRoot root = MissionReader.GetMissionData(filePath);
					return root.sdkVersion;
				}
				catch (System.Exception ex)
				{
					Debug.LogError(ex);
				}
			}

			return null;
		}

		public static string GetMissionInstalledMetaFilePath(uint missionID)
		{
			return Path.Combine(GetMissionDirectory(missionID), "installed.txt");
		}

		public static string GetMissionDetailsFilePath(uint missionID)
		{
			return Path.Combine(GetMissionDirectory(missionID), "details.txt");
		}

		public static string GetMissionDirectory(uint missionID)
		{
			return Path.Combine(GetCachePath(), missionID.ToString());
		}
	}
}
