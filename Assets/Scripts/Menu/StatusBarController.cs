using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionHub.Menu
{
	/// <summary>
	/// Controls text display on the bottom bar on the screen.
	/// </summary>
	public class StatusBarController : MonoBehaviour
	{
		[SerializeField] private Text m_txtStatus				= default;


		private void Awake()
		{
			Application.logMessageReceived += OnLogMessageReceived;
		}

		private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			m_txtStatus.text = condition;
		}

		public void SetMessage(string message)
		{
			m_txtStatus.text = message;
		}
	}
}
