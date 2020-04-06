using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OP2MissionHub.Dialogs
{
	/// <summary>
	/// Presents a list of string items to the user for selection.
	/// </summary>
	public class ProgressDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtTitle					= default;
		[SerializeField] private Image m_ProgressBar				= default;

		private UnityWebRequest m_WebRequest;
		private bool m_IsUploadRequest;


		private void Awake()
		{
			enabled = false;
		}

		private void Initialize(string title)
		{
			m_txtTitle.text = title;
		}

		public void SetTitle(string title)
		{
			m_txtTitle.text = title;
		}

		public void SetProgress(float progress)
		{
			m_ProgressBar.fillAmount = progress;
		}

		public void SetWebRequest(UnityWebRequest request, bool isUploadRequest=false)
		{
			m_WebRequest = request;
			m_IsUploadRequest = isUploadRequest;
			enabled = true;
		}

		private void Update()
		{
			if (m_WebRequest != null)
			{
				if (m_IsUploadRequest)
					m_ProgressBar.fillAmount = m_WebRequest.uploadProgress;
				else
					m_ProgressBar.fillAmount = m_WebRequest.downloadProgress;
			}
		}

		public void Close()
		{
			enabled = false;
			m_WebRequest = null;

			Destroy(gameObject);
		}


		/// <summary>
		/// Creates and presents the progress dialog to the user.
		/// </summary>
		/// <param name="title">The title of the dialog box.</param>
		public static ProgressDialog Create(string title)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/ProgressDialog"));
			ProgressDialog dialog = goDialog.GetComponent<ProgressDialog>();
			dialog.Initialize(title);

			return dialog;
		}
	}
}
