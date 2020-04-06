using OP2MissionHub.Data;
using OP2MissionHub.Dialogs;
using OP2MissionHub.Systems;
using UnityEngine;

namespace OP2MissionHub.Scenes
{
	public class MainController : MonoBehaviour
	{
		private void Awake()
		{
			ConsoleLog.Initialize();
			TextureManager.Initialize();
			FileReference.Initialize();
			CacheDetails.Initialize();

			// If game directory hasn't been set, Open "Locate Outpost2" dialog to force user to select one
			if (string.IsNullOrEmpty(UserPrefs.gameDirectory))
				PreferencesDialog.Create();

			Debug.Log("Hub initialized.");
		}

		private void OnDestroy()
		{
			TextureManager.Release();
		}
	}
}
