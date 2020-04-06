using UnityEngine;

namespace OP2MissionHub
{
	public class UserPrefs
	{
		/// <summary>
		/// Called when user prefs change.
		/// </summary>
		public static event System.Action onChangedPrefsCB;

		/// <summary>
		/// The Outpost 2 game directory to install and uninstall missions.
		/// </summary>
		public static string gameDirectory
		{
			get { return PlayerPrefs.GetString("OP2Directory");														}
			set { PlayerPrefs.SetString("OP2Directory", value); PlayerPrefs.Save(); onChangedPrefsCB?.Invoke();		}
		}
	}
}
