using System.Collections.Generic;
using System.IO;

namespace OP2MissionHub.Data
{
	/// <summary>
	/// Maintains installed file references.
	/// </summary>
	public class FileReference
	{
		private static Dictionary<string, int> m_FileReferences = new Dictionary<string, int>();


		/// <summary>
		/// Initializes installed file references based on the cached installed.txt for each mission directory.
		/// </summary>
		public static void Initialize()
		{
			// Parse cached mission directories
			foreach (string directory in Directory.EnumerateDirectories(CachePath.GetCachePath()))
			{
				uint missionID;
				if (!uint.TryParse(directory, out missionID))
					continue;

				// Register all installed files for mission
				string installedFilesPath = CachePath.GetMissionInstalledMetaFilePath(missionID);
				string[] installedFiles = File.ReadAllLines(installedFilesPath);

				foreach (string fileName in installedFiles)
					AddReference(fileName);
			}
		}

		public static void AddReference(string fileName)
		{
			int references;
			if (m_FileReferences.TryGetValue(fileName, out references))
				++references;

			m_FileReferences[fileName] = references;
		}

		public static void RemoveReference(string fileName)
		{
			int references;
			if (m_FileReferences.TryGetValue(fileName, out references))
				--references;
			
			m_FileReferences[fileName] = references;

			// Delete file
			if (m_FileReferences[fileName] <= 0)
				File.Delete(Path.Combine(UserPrefs.gameDirectory, fileName));
		}
	}
}
