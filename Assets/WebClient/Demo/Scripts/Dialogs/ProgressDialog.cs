using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.Dialogs
{
	public class ProgressDialog : MonoBehaviour
	{
		[SerializeField] private Button m_btnClose;
		[SerializeField] private Image m_ProgressCircle;
		[SerializeField] private Text m_txtProgress;
		[SerializeField] private Text m_txtStatus;

		private System.Action m_OnCancelCB;

		public float progress
		{
			get
			{
				return 1 - m_ProgressCircle.fillAmount;
			}
			set
			{
				m_txtProgress.text = Mathf.FloorToInt(value * 100) + "%";
				m_ProgressCircle.fillAmount = 1 - value;
			}
		}

		public string status
		{
			get { return m_txtStatus.text;	}
			set { m_txtStatus.text = value;	}
		}

		public bool closeInteractable
		{
			get { return m_btnClose.gameObject.activeSelf;	}
			set { m_btnClose.gameObject.SetActive(value);	}
		}


		// Use this for initialization
		public void Initialize(System.Action cb, string status)
		{
			m_OnCancelCB = cb;

			m_txtStatus.text = status;
			progress = 0;
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnCancelCB?.Invoke();
		}

		public static ProgressDialog Create(System.Action cb, string status)
		{
			GameObject goDlg = Instantiate(Resources.Load<GameObject>("Dialogs/Progress"));
			ProgressDialog dlg = goDlg.GetComponent<ProgressDialog>();

			dlg.Initialize(cb, status);

			return dlg;
		}
	}
}
