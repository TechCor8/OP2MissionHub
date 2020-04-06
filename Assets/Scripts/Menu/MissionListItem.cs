using DotNetMissionSDK;
using DotNetMissionSDK.Json;
using FlexAS.Demo;
using OP2MissionHub.Data;
using OP2MissionHub.Data.Json;
using OP2MissionHub.Dialogs;
using OP2MissionHub.Systems;
using OP2MissionHub.UserInterface;
using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OP2MissionHub.Menu
{
	public class MissionListItem : MonoBehaviour
	{
		[SerializeField] private InputField m_InputTitle			= default;
		[SerializeField] private InputField m_InputDescription		= default;
		[SerializeField] private Text m_txtGitHub					= default;
		[SerializeField] private InputField m_InputDetails			= default;
		[SerializeField] private GameObject m_Trusted				= default;
		[SerializeField] private GameObject m_Untrusted				= default;
		[SerializeField] private MinimapRenderer m_MinimapRenderer	= default;

		[SerializeField] private Button m_BtnDownload				= default;
		[SerializeField] private Button m_BtnDelete					= default;
		[SerializeField] private Button m_BtnInstall				= default;
		[SerializeField] private Button m_BtnUninstall				= default;

		[SerializeField] private Button m_BtnCertify				= default;
		[SerializeField] private Button m_BtnEdit					= default;
		[SerializeField] private Button m_BtnRemoveMission			= default;
		[SerializeField] private Button m_BtnUploadFile				= default;
		[SerializeField] private Button m_BtnDeleteFile				= default;

		[SerializeField] private Text m_txtDifferentVersion			= default;

		private const string GitHubDefaultText = "GitHub Link";

		public MissionData missionData { get; private set; }
		private string m_SDKVersion;
		private int m_LocalVersion;
		private bool m_IsStandardMission;

		private bool m_HasAdminPermissions;
		public bool hasAdminPermissions
		{
			get { return m_HasAdminPermissions; }
			set
			{
				m_HasAdminPermissions = value;
				m_BtnCertify.gameObject.SetActive(value);
				m_BtnCertify.interactable = missionData != null;
				if (value) hasAuthorPermissions = value;
			}
		}

		private bool m_HasAuthorPermissions;
		public bool hasAuthorPermissions
		{
			get { return m_HasAuthorPermissions; }
			set
			{
				m_HasAuthorPermissions = value;
				m_BtnEdit.gameObject.SetActive(value);
				m_BtnRemoveMission.gameObject.SetActive(value);
				m_BtnRemoveMission.interactable = missionData != null;
				m_BtnUploadFile.gameObject.SetActive(value);
				m_BtnDeleteFile.gameObject.SetActive(value);
			}
		}

		private bool m_CanEdit;
		public bool canEdit
		{
			get { return m_CanEdit; }
			set
			{
				m_CanEdit = value;
				m_InputTitle.interactable = value;
				m_InputDescription.interactable = value;
				m_BtnEdit.GetComponentInChildren<Text>().text = value ? "Save" : "Edit";
				if (value) hasAuthorPermissions = value;
			}
		}

		public string sdkVersion
		{
			get
			{
				if (!string.IsNullOrEmpty(m_SDKVersion))
					return m_SDKVersion;

				m_SDKVersion = CachePath.GetSDKVersion(missionData);
				return m_SDKVersion;
			}
		}

		public string details { get { return m_InputDetails.text; } }

		public System.Action onUpdatedMissionCB;


		private void Awake()
		{
			hasAdminPermissions = false;
			hasAuthorPermissions = false;
			canEdit = false;

			m_Trusted.SetActive(false);
			m_Untrusted.SetActive(false);

			m_BtnCertify.gameObject.SetActive(false);
			m_BtnRemoveMission.gameObject.SetActive(false);
			m_BtnUploadFile.gameObject.SetActive(false);
			m_BtnUploadFile.interactable = false;
			m_BtnDeleteFile.gameObject.SetActive(false);
			m_BtnDeleteFile.interactable = false;
			m_BtnEdit.gameObject.SetActive(false);

			m_BtnDelete.gameObject.SetActive(false);
			m_BtnUninstall.gameObject.SetActive(false);
			m_BtnDownload.interactable = false;
			m_BtnInstall.interactable = false;

			m_InputDetails.text = " ";
			m_txtDifferentVersion.text = "";
			m_txtGitHub.text = GitHubDefaultText;
		}

		public void Initialize(MissionData mission)
		{
			missionData = mission;

			m_InputTitle.text = mission.missionName;
			m_InputDescription.text = mission.missionDescription;
			m_txtGitHub.text = mission.gitHubLink;
			
			// DLL or OPM
			string dllName = mission.fileNames.FirstOrDefault((filename) => filename.IndexOf(".dll", System.StringComparison.OrdinalIgnoreCase) >= 0);
			string opmName = mission.fileNames.FirstOrDefault((filename) => filename.IndexOf(".opm", System.StringComparison.OrdinalIgnoreCase) >= 0);
			m_IsStandardMission = string.IsNullOrEmpty(dllName) && !string.IsNullOrEmpty(opmName);
			string mainFileName = m_IsStandardMission ? opmName : dllName;
			bool isStub = string.IsNullOrEmpty(mainFileName);

			// Mission type string
			string missionType = "Unknown";
			if (isStub) missionType = "Stub";
			else if (mainFileName.StartsWith("c")) missionType = "Colony";
			else if (mainFileName.StartsWith("a")) missionType = "Auto Demo";
			else if (mainFileName.StartsWith("t")) missionType = "Tutorial";
			else if (mainFileName.StartsWith("mu")) missionType = "Multiplayer Land Rush";
			else if (mainFileName.StartsWith("mf")) missionType = "Multiplayer Space Race";
			else if (mainFileName.StartsWith("mr")) missionType = "Multiplayer Resource Race";
			else if (mainFileName.StartsWith("mm")) missionType = "Multiplayer Midas";
			else if (mainFileName.StartsWith("ml")) missionType = "Multiplayer Last One Standing";

			// Certification image
			bool isCertified = !string.IsNullOrEmpty(mission.certifyingAdminName) || m_IsStandardMission || isStub;
			m_Trusted.SetActive(isCertified);
			m_Untrusted.SetActive(!isCertified);

			m_BtnCertify.GetComponentInChildren<Text>().text = string.IsNullOrEmpty(mission.certifyingAdminName) ? "Certify" : "Uncertify";

			// Certification string
			string certifiedString = mission.certifyingAdminName;
			if ((m_IsStandardMission || isStub) && string.IsNullOrEmpty(certifiedString))
				certifiedString = "Not Required";
			else if (!m_IsStandardMission && string.IsNullOrEmpty(certifiedString))
				certifiedString = "<color=yellow>Not Certified</color>";

			// Mission Details
			string details = m_IsStandardMission ? "Standard Mission" : "Custom Mission";
			details += "\nAuthor: " + mission.authorName;
			details += "\nCertified by: " + certifiedString;
			details += "\n";
			details += "\nType: " + missionType;
			//details += "\nPlayers: 6" + missionType;
			//details += "\nUnit Only: True" + missionType;
			details += "\n";
			details += "\nFiles:";
			details += "\n" + string.Join(", ", mission.fileNames);

			m_InputDetails.text = details;

			// We can upload files if the mission exists. This button won't be visible if the user does not have permission.
			m_BtnUploadFile.interactable = true;
			m_BtnDeleteFile.interactable = true;

			// Read local mission version
			if (File.Exists(CachePath.GetMissionDetailsFilePath(missionData.missionID)))
			{
				string json = File.ReadAllText(CachePath.GetMissionDetailsFilePath(missionData.missionID));
				MissionData localData = JsonUtility.FromJson<MissionData>(json);
				m_LocalVersion = localData.version;
			}
			else
			{
				// No local version exists, so use the current version
				m_LocalVersion = missionData.version;
			}

			// Show "New version available" text
			if (m_LocalVersion < missionData.version)
				m_txtDifferentVersion.text = "New version available.\nDelete to redownload.";
			else if (m_LocalVersion > missionData.version)
				m_txtDifferentVersion.text = "Older version available.\nDelete to redownload.";
			else
				m_txtDifferentVersion.text = "";

			// Show proper download state
			bool isCached = File.Exists(CachePath.GetMissionDetailsFilePath(missionData.missionID));
			m_BtnDownload.gameObject.SetActive(!isCached);
			m_BtnDelete.gameObject.SetActive(isCached);
			m_BtnDownload.interactable = true;
			
			// Show proper install state
			bool isInstalled = File.Exists(CachePath.GetMissionInstalledMetaFilePath(missionData.missionID));
			m_BtnInstall.gameObject.SetActive(!isInstalled);
			m_BtnUninstall.gameObject.SetActive(isInstalled);
			m_BtnInstall.interactable = isCached;	// You can only install if the mission is cached
			m_BtnUninstall.interactable = true;     // If installed, you can always uninstall. This button is only visible in that case.
			m_BtnDelete.interactable = !isInstalled;
			
		}

		public void OnClick_GitHubLink()
		{
			if (!canEdit)
				Application.OpenURL(missionData.gitHubLink);
			else
			{
				InputDialog.Create(OnInput_GitHubLink, "GitHub Link", "Enter the link to your source code.", "Enter Link...");
			}
		}

		private void OnInput_GitHubLink(string text)
		{
			if (!string.IsNullOrEmpty(text))
				m_txtGitHub.text = text;
		}

		public void OnClick_Edit()
		{
			if (!canEdit)
				canEdit = true;
			else
			{
				canEdit = false;

				// Save
				if (missionData == null)
				{
					// Add mission
					StartCoroutine(RequestAddMission());
				}
				else
				{
					// Update mission
					StartCoroutine(RequestUpdateMission());
				}
			}
		}

		private IEnumerator RequestAddMission()
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Creating Mission");

			// Perform request
			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", AppController.localUser.userID.ToString());
			formData.Add("SessionToken", AppController.localUser.sessionToken);
			formData.Add("MissionName", m_InputTitle.text);
			formData.Add("MissionDescription", m_InputDescription.text);
			formData.Add("GitHubLink", m_txtGitHub.text);

			using (UnityWebRequest request = UnityWebRequest.Post(WebConfig.webAPI + "mission/add_mission.php", formData))
			{
				progressDialog.SetWebRequest(request);

				yield return request.SendWebRequest();

				string error = WebConfig.GetErrorString(request);

				if (string.IsNullOrEmpty(error))
				{
					// Success
					onUpdatedMissionCB?.Invoke();
				}
				else
				{
					// Failure
					Debug.Log(error);
					InfoDialog.Create("", error.ToString());

					if (WebConfig.DidSessionExpire(request))
						AppController.LogOut();
				}
			}

			progressDialog.Close();
		}

		private IEnumerator RequestUpdateMission()
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Updating Mission");

			// Perform request
			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", AppController.localUser.userID.ToString());
			formData.Add("SessionToken", AppController.localUser.sessionToken);
			formData.Add("MissionID", missionData.missionID.ToString());
			formData.Add("MissionName", m_InputTitle.text);
			formData.Add("MissionDescription", m_InputDescription.text);
			formData.Add("GitHubLink", m_txtGitHub.text);

			using (UnityWebRequest request = UnityWebRequest.Post(WebConfig.webAPI + "mission/update_mission.php", formData))
			{
				progressDialog.SetWebRequest(request);

				yield return request.SendWebRequest();

				string error = WebConfig.GetErrorString(request);

				if (string.IsNullOrEmpty(error))
				{
					// Success
					onUpdatedMissionCB?.Invoke();
				}
				else
				{
					// Failure
					Debug.Log(error);
					InfoDialog.Create("", error.ToString());

					if (WebConfig.DidSessionExpire(request))
						AppController.LogOut();
				}
			}

			progressDialog.Close();
		}

		public void OnClick_Certify()
		{
			if (string.IsNullOrEmpty(missionData.certifyingAdminName))
			{
				// Certify this mission
				StartCoroutine(RequestCertifyMission(true));
			}
			else
			{
				// Uncertify this mission
				StartCoroutine(RequestCertifyMission(false));
			}
		}

		private IEnumerator RequestCertifyMission(bool certify)
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Updating Mission");

			// Perform request
			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", AppController.localUser.userID.ToString());
			formData.Add("SessionToken", AppController.localUser.sessionToken);
			formData.Add("MissionID", missionData.missionID.ToString());
			formData.Add("Certify", certify ? 1.ToString() : 0.ToString());

			using (UnityWebRequest request = UnityWebRequest.Post(WebConfig.webAPI + "mission/certify_mission.php", formData))
			{
				progressDialog.SetWebRequest(request);

				yield return request.SendWebRequest();

				string error = WebConfig.GetErrorString(request);

				if (string.IsNullOrEmpty(error))
				{
					// Success
					onUpdatedMissionCB?.Invoke();
				}
				else
				{
					// Failure
					Debug.Log(error);
					InfoDialog.Create("", error.ToString());

					if (WebConfig.DidSessionExpire(request))
						AppController.LogOut();
				}
			}

			progressDialog.Close();
		}

		public void OnClick_RemoveMission()
		{
			ConfirmDialog.Create(OnConfirm_RemoveMission, "", "Are you sure you want to permanently remove this mission from the server?", "Remove");
		}

		private void OnConfirm_RemoveMission(bool didConfirm)
		{
			if (didConfirm)
				StartCoroutine(RequestRemoveMission());
		}

		private IEnumerator RequestRemoveMission()
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Removing Mission");

			// Perform request
			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", AppController.localUser.userID.ToString());
			formData.Add("SessionToken", AppController.localUser.sessionToken);
			formData.Add("MissionID", missionData.missionID.ToString());
			
			using (UnityWebRequest request = UnityWebRequest.Post(WebConfig.webAPI + "mission/remove_mission.php", formData))
			{
				progressDialog.SetWebRequest(request);

				yield return request.SendWebRequest();

				string error = WebConfig.GetErrorString(request);

				if (string.IsNullOrEmpty(error))
				{
					// Success
					onUpdatedMissionCB?.Invoke();
				}
				else
				{
					// Failure
					Debug.Log(error);
					InfoDialog.Create("", error.ToString());

					if (WebConfig.DidSessionExpire(request))
						AppController.LogOut();
				}
			}

			progressDialog.Close();
		}

		public void OnClick_UploadFile()
		{
			FileBrowser.ShowLoadDialog(OnSelected_UploadFilePath, null, false, UserPrefs.gameDirectory, "Upload File", "Upload");
		}

		private void OnSelected_UploadFilePath(string path)
		{
			// Require DLL uploads to specify a GitHub link
			if (Path.GetExtension(path).ToLowerInvariant().Contains("dll"))
			{
				if (m_txtGitHub.text == GitHubDefaultText)
				{
					InfoDialog.Create("GitHub Link Required", "A GitHub link is required to certify your DLL.");
					return;
				}
			}

			if (!string.IsNullOrEmpty(path))
				StartCoroutine(RequestUploadFile(path));
		}

		private IEnumerator RequestUploadFile(string filePath)
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Uploading File");

			// Perform request
			List<IMultipartFormSection> multipartData = new List<IMultipartFormSection>();
			multipartData.Add(new MultipartFormDataSection("UserID", AppController.localUser.userID.ToString(), "form-data"));
			multipartData.Add(new MultipartFormDataSection("SessionToken", AppController.localUser.sessionToken, "form-data"));
			multipartData.Add(new MultipartFormDataSection("MissionID", missionData.missionID.ToString(), "form-data"));
			multipartData.Add(new MultipartFormFileSection("MissionFile", File.ReadAllBytes(filePath), Path.GetFileName(filePath), "file"));

			using (UnityWebRequest request = UnityWebRequest.Post(WebConfig.webAPI + "mission/add_file.php", multipartData))
			{
				progressDialog.SetWebRequest(request, true);

				yield return request.SendWebRequest();

				string error = WebConfig.GetErrorString(request);

				if (string.IsNullOrEmpty(error))
				{
					// Success
					onUpdatedMissionCB?.Invoke();
				}
				else
				{
					// Failure
					Debug.Log(error);
					InfoDialog.Create("", error.ToString());

					if (WebConfig.DidSessionExpire(request))
						AppController.LogOut();
				}
			}

			progressDialog.Close();
		}

		public void OnClick_DeleteFile()
		{
			ListSelectDialog.Create(missionData.fileNames, "Delete File", "Delete", OnSelected_DeleteFileName);
		}

		private void OnSelected_DeleteFileName(string fileName)
		{
			StartCoroutine(RequestDeleteFile(fileName));
		}

		private IEnumerator RequestDeleteFile(string fileName)
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Deleting File");

			// Perform request
			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", AppController.localUser.userID.ToString());
			formData.Add("SessionToken", AppController.localUser.sessionToken);
			formData.Add("MissionID", missionData.missionID.ToString());
			formData.Add("FileName", fileName);
			
			using (UnityWebRequest request = UnityWebRequest.Post(WebConfig.webAPI + "mission/remove_file.php", formData))
			{
				progressDialog.SetWebRequest(request);

				yield return request.SendWebRequest();

				string error = WebConfig.GetErrorString(request);

				if (string.IsNullOrEmpty(error))
				{
					// Success
					onUpdatedMissionCB?.Invoke();
				}
				else
				{
					// Failure
					Debug.Log(error);
					InfoDialog.Create("", error.ToString());

					if (WebConfig.DidSessionExpire(request))
						AppController.LogOut();
				}
			}

			progressDialog.Close();
		}

		public void OnClick_Download()
		{
			// Download mission to cache
			StartCoroutine(RequestDownloadMission());
		}

		private IEnumerator RequestDownloadMission()
		{
			ProgressDialog progressDialog = ProgressDialog.Create("Downloading Mission");

			foreach (string fileName in missionData.fileNames)
			{
				// Perform request
				string url = WebConfig.webHost + "download/" + missionData.missionID + "/" + fileName;
				string destPath = Path.Combine(CachePath.GetMissionDirectory(missionData.missionID), fileName);

				using (UnityWebRequest request = UnityWebRequest.Get(url))
				{
					progressDialog.SetTitle("Downloading " + fileName);
					progressDialog.SetWebRequest(request);

					yield return request.SendWebRequest();

					if (!DidDownloadSucceed(request, true))
					{
						progressDialog.Close();
						OnClick_Delete();
						yield break;
					}

					WriteFile(destPath, request.downloadHandler.data);
				}
			}

			// Get SDK path
			if (!string.IsNullOrEmpty(sdkVersion))
			{
				// Download SDK if it does not exist
				if (!File.Exists(CachePath.GetSDKFilePath(sdkVersion)))
				{
					string url = "https://github.com/TechCor8/OP2DotNetMissionSDK/releases/download/" + sdkVersion + "/" + CachePath.GetSDKFileName(sdkVersion);
					string destPath = CachePath.GetSDKFilePath(sdkVersion);

					using (UnityWebRequest request = UnityWebRequest.Get(url))
					{
						progressDialog.SetTitle("Downloading " + CachePath.GetSDKFileName(sdkVersion));
						progressDialog.SetWebRequest(request);

						yield return request.SendWebRequest();

						if (!DidDownloadSucceed(request, true))
						{
							progressDialog.Close();
							OnClick_Delete();
							yield break;
						}

						WriteFile(destPath, request.downloadHandler.data);
					}
				}

				// Download SDK Interop if it does not exist or is from an older SDK
				if (!File.Exists(CachePath.GetInteropFilePath()) || CachePath.IsNewerVersion(sdkVersion, CacheDetails.interopVersion))
				{
					string url = "https://github.com/TechCor8/OP2DotNetMissionSDK/releases/download/" + sdkVersion + "/" + CachePath.DotNetInteropFileName;
					string destPath = CachePath.GetInteropFilePath();
					
					using (UnityWebRequest request = UnityWebRequest.Get(url))
					{
						progressDialog.SetTitle("Downloading " + CachePath.DotNetInteropFileName);
						progressDialog.SetWebRequest(request);

						yield return request.SendWebRequest();

						if (!DidDownloadSucceed(request, true))
						{
							progressDialog.Close();
							OnClick_Delete();
							yield break;
						}

						WriteFile(destPath, request.downloadHandler.data);
					}
				}
			}

			progressDialog.Close();

			// Mission fully downloaded.
			// Write mission details
			string detailsJson = JsonUtility.ToJson(missionData);
			File.WriteAllText(CachePath.GetMissionDetailsFilePath(missionData.missionID), detailsJson);

			// Add mission details reference
			CacheDetails.AddMissionData(missionData);

			// Set buttons
			m_BtnDownload.gameObject.SetActive(false);
			m_BtnDelete.gameObject.SetActive(true);
			m_BtnInstall.interactable = true;
		}

		private bool DidDownloadSucceed(UnityWebRequest request, bool showError)
		{
			string error = null;
			if (request.isNetworkError)
				error = "There was a communication error: " + request.error;
			else if (request.isHttpError)
				error = "There was an HTTP error: " + request.error;

			if (!string.IsNullOrEmpty(error))
			{
				// Failure
				Debug.Log(error);
				if (showError)
					InfoDialog.Create("", error.ToString());

				return false;
			}

			return true;
		}

		private void WriteFile(string path, byte[] data)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, data);
		}

		public void OnClick_Delete()
		{
			// Remove details reference
			CacheDetails.RemoveMissionData(missionData.missionID);

			// Delete mission from cache
			if (Directory.Exists(CachePath.GetMissionDirectory(missionData.missionID)))
				Directory.Delete(CachePath.GetMissionDirectory(missionData.missionID), true);

			// Set buttons
			m_BtnDownload.gameObject.SetActive(true);
			m_BtnDelete.gameObject.SetActive(false);
			m_BtnInstall.interactable = false;
		}

		public void OnClick_Install()
		{
			// Get the cached mission details
			string json = File.ReadAllText(CachePath.GetMissionDetailsFilePath(missionData.missionID));
			MissionData localData = JsonUtility.FromJson<MissionData>(json);
			string sdkVersion = CachePath.GetSDKVersion(localData);
			
			// Do not allow installation if mission files already exist
			foreach (string fileName in localData.fileNames)
			{
				string filePath = Path.Combine(UserPrefs.gameDirectory, fileName);

				if (File.Exists(filePath))
				{
					InfoDialog.Create("Installation Failed", "The file '" + fileName + "' already exists in your game directory. Please remove it to continue.");
					return;
				}
			}

			List<string> installedFiles = new List<string>();

			// Need to export plugin for standard mission OPM file
			if (m_IsStandardMission)
			{
				string opmFileName = localData.fileNames.FirstOrDefault((string fileName) => Path.GetExtension(fileName).ToLowerInvariant().Contains("opm"));
				string opmFilePath = Path.Combine(CachePath.GetMissionDirectory(localData.missionID), opmFileName);

				string pluginFileName = Path.ChangeExtension(opmFileName, ".dll");
				string pluginPath = Path.Combine(UserPrefs.gameDirectory, pluginFileName);

				// Don't allow install if the plugin will overwrite another DLL of the same name
				if (File.Exists(pluginPath))
				{
					InfoDialog.Create("Install Failed", "There is already a plugin named " + pluginFileName);
					return;
				}

				// Export plugin
				MissionRoot root = MissionReader.GetMissionData(opmFilePath);
				PluginExporter.ExportPlugin(pluginPath, root.sdkVersion, root.levelDetails);

				FileReference.AddReference(pluginFileName);

				installedFiles.Add(pluginFileName);
			}

			// Install mission from cache into game folder
			foreach (string fileName in localData.fileNames)
			{
				string filePath = Path.Combine(CachePath.GetMissionDirectory(localData.missionID), fileName);

				InstallFile(fileName, filePath);

				installedFiles.Add(fileName);
			}

			// Install SDK
			if (!string.IsNullOrEmpty(sdkVersion))
			{
				InstallFile(CachePath.GetSDKFileName(sdkVersion), CachePath.GetSDKFilePath(sdkVersion));
				InstallFile(CachePath.DotNetInteropFileName, CachePath.GetInteropFilePath(), true);

				installedFiles.Add(CachePath.GetSDKFileName(sdkVersion));
				installedFiles.Add(CachePath.DotNetInteropFileName);
			}

			// Write installed files to cache
			using (FileStream fs = new FileStream(CachePath.GetMissionInstalledMetaFilePath(localData.missionID), FileMode.Create, FileAccess.Write, FileShare.Read))
			using (StreamWriter writer = new StreamWriter(fs))
			{
				foreach (string fileName in installedFiles)
					writer.WriteLine(fileName);
			}

			// Set buttons
			m_BtnInstall.gameObject.SetActive(false);
			m_BtnUninstall.gameObject.SetActive(true);
			m_BtnDelete.interactable = false;
		}

		private void InstallFile(string fileName, string srcPath, bool overwrite=false)
		{
			FileReference.AddReference(fileName);

			string destPath = Path.Combine(UserPrefs.gameDirectory, fileName);

			if (!overwrite && File.Exists(destPath))
				return;

			File.Copy(srcPath, destPath);
		}

		public void OnClick_Uninstall()
		{
			string installedFilesPath = CachePath.GetMissionInstalledMetaFilePath(missionData.missionID);

			// Read installed files from cache
			string[] installedFiles = File.ReadAllLines(installedFilesPath);

			// Delete mission files from game folder
			foreach (string fileName in installedFiles)
				UninstallFile(fileName);

			// Uninstall SDK
			if (!string.IsNullOrEmpty(sdkVersion))
			{
				UninstallFile(CachePath.GetSDKFileName(sdkVersion));
				UninstallFile(CachePath.DotNetInteropFileName);
			}

			// Delete meta data
			File.Delete(installedFilesPath);

			// Set buttons
			m_BtnInstall.gameObject.SetActive(true);
			m_BtnUninstall.gameObject.SetActive(false);
			m_BtnDelete.interactable = true;
		}

		private void UninstallFile(string fileName)
		{
			FileReference.RemoveReference(fileName);
		}
	}
}
