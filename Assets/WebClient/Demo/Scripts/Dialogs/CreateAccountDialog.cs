using UnityEngine;
using UnityEngine.UI;

namespace FlexAS.Demo
{
	public class CreateAccountDialog : MonoBehaviour
	{
		[SerializeField] private CanvasGroup m_CanvasGroup						= default;

		[SerializeField] private InputField m_InputUserName						= default;
		[SerializeField] private InputField m_InputDisplayName					= default;
		[SerializeField] private InputField m_InputEmail						= default;
		[SerializeField] private InputField m_InputPassword						= default;

		[SerializeField] private Text m_txtError								= default;

		public delegate void OnDialogCallback();

		protected OnDialogCallback m_OnDialogClosedCB;


		// Awake is called before the first frame update
		void Awake()
		{
			m_txtError.text = "";
		}

		// Update is called once per frame
		void Update()
		{
			// Trigger create account on enter key
			if (Input.GetKeyDown(KeyCode.Return))
				OnClick_CreateAccount();
		}

		public void OnClick_CreateAccount()
		{
			m_CanvasGroup.interactable = false;

			// Request new account
			CustomAuthenticator authenticator = new CustomAuthenticator(AppController.appService);
			authenticator.CreateAccount(OnCreateAccountResponse, m_InputEmail.text, m_InputUserName.text, m_InputDisplayName.text, m_InputPassword.text, Application.version);
		}

		private void OnCreateAccountResponse(CreateAccountResponse response, ErrorResponse error)
		{
			if (error == null)
			{
				// Success
				InfoDialog.Create("Check your email!",
					"Thanks for signing up!\n\nA confirmation email has been sent."
					+ " Please click the link in the email to activate your account.", OnClick_Close);
			}
			else
			{
				// Error occurred
				m_txtError.text = error.errorMessage;

				m_CanvasGroup.interactable = true;

				Debug.Log(error.ToDeveloperMessage(true));
			}
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);

			m_OnDialogClosedCB?.Invoke();
		}

		public static CreateAccountDialog Create(OnDialogCallback onCloseCB=null)
		{
			GameObject goDlg = Instantiate(Resources.Load<GameObject>("Dialogs/CanvasCreateAccount"));
			CreateAccountDialog dlg = goDlg.GetComponent<CreateAccountDialog>();
			dlg.m_OnDialogClosedCB = onCloseCB;

			return dlg;
		}
	}
}
