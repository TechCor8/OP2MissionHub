using OP2MissionHub.Dialogs;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionHub.Menu
{
	/// <summary>
	/// Handles click events for the menu bar at the top of the screen.
	/// </summary>
	public class MainMenuController : MonoBehaviour
	{
		[SerializeField] private CanvasGroup m_CanvasGroup		= default;

		[SerializeField] private Text m_txtLogin				= default;

		public bool interactable { get { return m_CanvasGroup.interactable; } set { m_CanvasGroup.interactable = value; } }


		private void Awake()
		{
			AppController.onLoginStatusChangedCB += RefreshLoginText;
		}

		private void RefreshLoginText()
		{
			m_txtLogin.text = AppController.localUser.isLoggedIn ? "Log Out" : "Log In";
		}

		public void OnClick_Preferences()
		{
			PreferencesDialog.Create();
		}

		public void OnClick_Login()
		{
			if (AppController.localUser.isLoggedIn)
				AppController.LogOut();
			else
			{
				// Open login screen for user to log in
				FlexAS.Demo.LoginScreen.Create();
			}
		}

		public void OnClick_RunOutpost2()
		{
			string path = Path.Combine(UserPrefs.gameDirectory, "Outpost2.exe");

			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(path);
			startInfo.WorkingDirectory = UserPrefs.gameDirectory;
			System.Diagnostics.Process.Start(startInfo);
		}

		public void OnClick_AboutMissionHub()
		{
			AboutDialog.Create();
		}

		private void OnDestroy()
		{
			AppController.onLoginStatusChangedCB -= RefreshLoginText;
		}
	}
}
