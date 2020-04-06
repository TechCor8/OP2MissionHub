using FlexAS.Demo;
using OP2MissionHub.Data;
using OP2MissionHub.Data.Json;
using OP2MissionHub.Dialogs;
using OP2MissionHub.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OP2MissionHub.Menu
{
	/// <summary>
	/// Pulls mission database and displays them in a filtered list.
	/// </summary>
	public class MissionListController : MonoBehaviour
	{
		private enum MissionFilter { Library, Standard, Custom, YourContent }

		[SerializeField] private StatusBarController m_StatusBar	= default;

		[SerializeField] private Toggle[] m_ToggleFilters			= default;
		[SerializeField] private Transform m_MissionContainer		= default;
		[SerializeField] private GameObject m_MissionListItemPrefab	= default;
		[SerializeField] private Text m_txtLoginToViewYourContent	= default;
		[SerializeField] private InputField m_InputSearchFilter		= default;

		//[SerializeField] private Button m_BtnRefresh				= default;

		private ReadOnlyCollection<MissionData> m_Missions;

		private MissionFilter m_MissionFilter;


		private void Start()
		{
			AppController.onLoginStatusChangedCB += RefreshFilter;

			Refresh();
		}

		public void Refresh()
		{
			// Disable refresh button
			//m_BtnRefresh.interactable = false;
			//StartCoroutine(WaitToEnableRefreshButton());

			// Fetch missions from database
			StartCoroutine(RequestMissionData());
		}

		private IEnumerator RequestMissionData()
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Fetching Missions");

			using (UnityWebRequest request = UnityWebRequest.Get(WebConfig.webAPI + "mission/get_missions.php"))
			{
				progressDialog.SetWebRequest(request);

				yield return request.SendWebRequest();

				string error = WebConfig.GetErrorString(request);

				if (string.IsNullOrEmpty(error))
				{
					// Success
					MissionResponse response = MissionResponse.FromJson(request.downloadHandler.text);
					m_Missions = response.missions;

					PopulateMissions(m_MissionFilter);
				}
				else
				{
					// Failure
					Debug.Log(error);
					InfoDialog.Create("", error.ToString());
				}
			}

			progressDialog.Close();
		}

		private void PopulateMissions(MissionFilter missionFilter)
		{
			ClearMissions();

			List<MissionData> missions;

			// Show local content even if it is no longer provided by the database
			if (missionFilter == MissionFilter.Library)
			{
				missions = new List<MissionData>(CacheDetails.missions);

				// Create database lookup table
				Dictionary<uint, MissionData> lookupTable = new Dictionary<uint, MissionData>();
				foreach (MissionData mission in m_Missions)
					lookupTable[mission.missionID] = mission;

				// Replace local mission data with the database data
				for (int i=0; i < missions.Count; ++i)
				{
					MissionData dbMissionData;
					if (!lookupTable.TryGetValue(missions[i].missionID, out dbMissionData))
						continue;

					missions[i] = dbMissionData;
				}
			}
			else
			{
				// Use the database data
				missions = new List<MissionData>(m_Missions);
			}

			foreach (MissionData mission in missions)
			{
				// Determine is mission is a standard mission
				bool hasDLL = mission.fileNames.FirstOrDefault((filename) => filename.IndexOf(".dll", System.StringComparison.OrdinalIgnoreCase) >= 0) != null;
				bool hasOPM = mission.fileNames.FirstOrDefault((filename) => filename.IndexOf(".opm", System.StringComparison.OrdinalIgnoreCase) >= 0) != null;
				bool isStandardMission = !hasDLL && hasOPM;

				//bool isInstalled = File.Exists(CachePath.GetMissionInstalledMetaFilePath(mission.missionID));
				bool isDownloaded = File.Exists(CachePath.GetMissionDetailsFilePath(mission.missionID));
				bool isYourContent = AppController.localUser.isLoggedIn && AppController.localUser.userID == mission.authorID;

				if (missionFilter == MissionFilter.Standard && !isStandardMission) continue;
				if (missionFilter == MissionFilter.Custom && isStandardMission) continue;
				if (missionFilter == MissionFilter.Library && !isDownloaded) continue;
				if (missionFilter == MissionFilter.YourContent && !isYourContent) continue;

				// Create mission item
				GameObject goItem = Instantiate(m_MissionListItemPrefab);
				goItem.GetComponent<MissionListItem>().onUpdatedMissionCB += Refresh;
				goItem.GetComponent<MissionListItem>().Initialize(mission);
				goItem.GetComponent<MissionListItem>().hasAuthorPermissions = AppController.localUser.isLoggedIn && AppController.localUser.userID == mission.authorID;
				goItem.GetComponent<MissionListItem>().hasAdminPermissions = AppController.localUser.isLoggedIn && AppController.localUser.isAdmin;

				goItem.transform.SetParent(m_MissionContainer);
			}

			if (missionFilter == MissionFilter.YourContent && AppController.localUser.isLoggedIn)
			{
				// Create blank mission item
				GameObject goItem = Instantiate(m_MissionListItemPrefab);
				goItem.GetComponent<MissionListItem>().onUpdatedMissionCB += Refresh;
				goItem.GetComponent<MissionListItem>().canEdit = true;
				goItem.GetComponent<MissionListItem>().hasAdminPermissions = AppController.localUser.isLoggedIn && AppController.localUser.isAdmin;

				goItem.transform.SetParent(m_MissionContainer);
			}

			OnValueChanged_SearchFilter();
		}

		private void ClearMissions()
		{
			foreach (Transform t in m_MissionContainer)
				Destroy(t.gameObject);
		}

		public void OnChange_Filter(bool isOn)
		{
			if (!isOn)
				return;

			// Find active filter
			for (int i=0; i < m_ToggleFilters.Length; ++i)
			{
				if (m_ToggleFilters[i].isOn)
					m_MissionFilter = (MissionFilter)i;
			}

			switch (m_MissionFilter)
			{
				case MissionFilter.Library:		m_StatusBar.SetMessage("These are missions you have downloaded or installed.");					break;
				case MissionFilter.Standard:	m_StatusBar.SetMessage("These are missions that were built with the OP2 mission editor.");		break;
				case MissionFilter.Custom:		m_StatusBar.SetMessage("These are missions that were compiled from custom source code.");		break;
				case MissionFilter.YourContent:	m_StatusBar.SetMessage("These are missions you created.");										break;
			}

			RefreshFilter();
		}

		private void RefreshFilter()
		{
			m_txtLoginToViewYourContent.gameObject.SetActive(m_MissionFilter == MissionFilter.YourContent && !AppController.localUser.isLoggedIn);

			PopulateMissions(m_MissionFilter);
		}

		public void OnValueChanged_SearchFilter()
		{
			// Toggle visibility of items based on search filter
			foreach (Transform t in m_MissionContainer)
			{
				MissionListItem item = t.GetComponent<MissionListItem>();
				if (item == null || item.missionData == null)
					continue;

				// Filter mission data by user search
				if (!string.IsNullOrEmpty(m_InputSearchFilter.text))
				{
					if (item.missionData.authorName.IndexOf(m_InputSearchFilter.text, System.StringComparison.OrdinalIgnoreCase) < 0
						&& item.missionData.certifyingAdminName.IndexOf(m_InputSearchFilter.text, System.StringComparison.OrdinalIgnoreCase) < 0
						&& item.missionData.missionName.IndexOf(m_InputSearchFilter.text, System.StringComparison.OrdinalIgnoreCase) < 0
						&& item.missionData.missionDescription.IndexOf(m_InputSearchFilter.text, System.StringComparison.OrdinalIgnoreCase) < 0
						&& item.missionData.fileNames.FirstOrDefault((fileName) => fileName.IndexOf(m_InputSearchFilter.text, System.StringComparison.OrdinalIgnoreCase) >= 0) == null
						&& item.details.IndexOf(m_InputSearchFilter.text, System.StringComparison.OrdinalIgnoreCase) < 0)
					{
						item.gameObject.SetActive(false);
						continue;
					}
				}

				item.gameObject.SetActive(true);
			}
		}

		//private IEnumerator WaitToEnableRefreshButton()
		//{
		//	yield return new WaitForSecondsRealtime(15.0f);

		//	m_BtnRefresh.interactable = true;
		//}

		private void OnDestroy()
		{
			AppController.onLoginStatusChangedCB -= RefreshFilter;
		}
	}
}
