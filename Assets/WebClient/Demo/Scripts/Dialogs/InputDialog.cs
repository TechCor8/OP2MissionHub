using UnityEngine;
using UnityEngine.UI;

namespace FlexAS.Demo
{
	public class InputDialog : MonoBehaviour
	{
		[SerializeField] private Text m_txtTitle							= default;
		[SerializeField] private Text m_txtBody								= default;
		[SerializeField] private Text m_txtPlaceholder						= default;

		[SerializeField] private InputField m_InputText						= default;

		public delegate void OnDialogCallback(string userInput);

		protected OnDialogCallback m_OnDialogClosedCB;

		public InputField.ContentType contentType	{ get { return m_InputText.contentType;		} set { m_InputText.contentType = value;	} }
		public int characterLimit					{ get { return m_InputText.characterLimit;	} set { m_InputText.characterLimit = value;	} }


		// Use this for initialization
		public void Initialize(OnDialogCallback cb, string title, string body, string placeholder)
		{
			m_OnDialogClosedCB = cb ?? throw new System.ArgumentNullException("cb");
			m_txtTitle.text = title;
			m_txtBody.text = body;
			m_txtPlaceholder.text = placeholder;
		}

		public void OnClick_Submit()
		{
			m_OnDialogClosedCB?.Invoke(m_InputText.text);

			Destroy(gameObject);
		}

		public void OnClick_Cancel()
		{
			m_OnDialogClosedCB?.Invoke(null);

			Destroy(gameObject);
		}

		// Update is called once per frame
		private void Update()
		{
			// Trigger submit on enter key
			if (Input.GetKeyDown(KeyCode.Return))
				OnClick_Submit();
		}

		public static InputDialog Create(OnDialogCallback cb, string title, string body, string placeholder)
		{
			GameObject goDlg = Instantiate(Resources.Load<GameObject>("Dialogs/CanvasInputDialog"));
			InputDialog dlg = goDlg.GetComponent<InputDialog>();

			dlg.Initialize(cb, title, body, placeholder);

			return dlg;
		}
	}
}
