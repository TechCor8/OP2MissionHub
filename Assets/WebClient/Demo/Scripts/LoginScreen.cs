using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FlexAS.Demo
{
	public class LoginScreen : MonoBehaviour
	{
		[SerializeField] private CanvasGroup m_LoginCanvasGroup				= default;

		[SerializeField] private InputField m_InputUserName					= default;
		[SerializeField] private InputField m_InputPassword					= default;

		[SerializeField] private Toggle m_RememberMe						= default;


		private void Start()
		{
			// Get the current "remember me" state
			bool shouldRememberMe = CustomAuthenticator.rememberMe;

			m_RememberMe.isOn = shouldRememberMe;

			if (shouldRememberMe)
			{
				// Auto-fill credentials if "remember me" was used last session
				m_InputUserName.text = CustomAuthenticator.rememberMeUserName;
				m_InputPassword.text = "TOKEN";
			}

			// Allow user to interact with the login screen
			m_LoginCanvasGroup.interactable = true;

			m_InputUserName.ActivateInputField();
		}

		public void OnClick_Login()
		{
			// Don't allow user to interact during the login process
			m_LoginCanvasGroup.interactable = false;

			AppController.Login(OnLoginResponse, m_InputUserName.text, m_InputPassword.text, Application.version, m_RememberMe.isOn);
		}

		private void OnLoginResponse(LoginResponse response, ErrorResponse error)
		{
			if (error != null)
			{
				if (response.responseCode == LoginResponse.ResponseCode.DownForMaintenance && response.hasAdminAccess)
				{
					// Allow admin user the option to override the maintenance block
					ConfirmDialog.Create(OnOverrideDialogResponse, "Down For Maintenance", error.ToString(), "Override");
				}
				else if (response.responseCode == LoginResponse.ResponseCode.BadVersion)
				{
					// Allow user to update app version
					ConfirmDialog.Create(OnUpdateDialogResponse, "Update Required", error.ToString(), "Download");
				}
				else
				{
					// Other errors that always restore the login screen
					Debug.LogError(error.ToDeveloperMessage(true));
					InfoDialog.Create("", error.ToString(), OnCloseInfoDialog);					
				}
			}
			else
			{
				if (!response.emailVerified)
				{
					ConfirmDialog.Create(OnClick_ResendEmail, "Email Not Yet Verified", "Your email has not yet been verified. You must confirm your email address to upload content."
							+ "\n\nWould you like to resend the confirmation email?", "Resend");
				}
				else
				{
					// Success. Close login dialog.
					OnClick_Close();
				}
			}
		}

		private void OnCloseInfoDialog()
		{
			m_LoginCanvasGroup.interactable = true;
		}

		private void OnOverrideDialogResponse(bool didConfirm)
		{
			if (didConfirm)
			{
				// Override maintenance
				AppController.Login(OnLoginResponse, m_InputUserName.text, m_InputPassword.text, Application.version, m_RememberMe.isOn, true);
			}
			else
			{
				// Restore login
				m_LoginCanvasGroup.interactable = true;
			}
		}

		private void OnUpdateDialogResponse(bool didConfirm)
		{
			if (didConfirm)
			{
				// Send user to download page
				Application.OpenURL("https://github.com/TechCor8/OP2MissionEditor/releases");
				m_LoginCanvasGroup.interactable = true;
			}
			else
			{
				// Restore login
				m_LoginCanvasGroup.interactable = true;
			}
		}

		private void OnClick_ResendEmail(bool didConfirm)
		{
			if (didConfirm)
			{
				// Resend confirmation email
				AppController.localUser.ResendVerificationEmail(OnResendEmailResponse);
			}
			else
			{
				// Restore login
				m_LoginCanvasGroup.interactable = true;
			}
		}

		private void OnResendEmailResponse(UserAccount.ResendEmailResponseCode code, ErrorResponse error)
		{
			// Log the user out
			AppController.localUser.LogOut((ErrorResponse logoutError) =>
			{
				// Inform user about the email response
				switch (code)
				{
					case UserAccount.ResendEmailResponseCode.Success:
						InfoDialog.Create("Email Sent", "A confirmation email has been sent. Please check your inbox.", OnCloseInfoDialog);
						break;

					case UserAccount.ResendEmailResponseCode.AlreadyVerified:
						InfoDialog.Create("Already Verified", "Email address confirmed. Please log in again.", OnCloseInfoDialog);
						break;

					default:
						InfoDialog.Create("", error.ToString(), OnCloseInfoDialog);
						break;
				}
			});
		}

		public void OnClick_CreateAccount()
		{
			m_LoginCanvasGroup.interactable = false;

			CreateAccountDialog.Create(OnCreateAccountDialogResponse);
		}

		private void OnCreateAccountDialogResponse()
		{
			m_LoginCanvasGroup.interactable = true;
		}

		/// <summary>
		/// When the user clicks "Forgot Password", ask for their email address and fire off a reset password email.
		/// </summary>
		public void OnClick_ForgotPassword()
		{
			m_LoginCanvasGroup.interactable = false;

			InputDialog dlg = InputDialog.Create(OnForgotPasswordResponse, "Reset Password", "Enter your account email", "Enter email...");
			dlg.contentType = InputField.ContentType.EmailAddress;
			dlg.characterLimit = 255;
		}

		private void OnForgotPasswordResponse(string userInput)
		{
			if (string.IsNullOrEmpty(userInput))
			{
				m_LoginCanvasGroup.interactable = true;
				return;
			}

			// Reset password
			CustomAuthenticator authenticator = new CustomAuthenticator(AppController.appService);
			authenticator.RequestPasswordReset(OnPasswordRequestResponse, userInput);
		}

		private void OnPasswordRequestResponse(ResetPasswordResponse response, ErrorResponse error)
		{
			m_LoginCanvasGroup.interactable = true;

			if (error == null)
			{
				InfoDialog.Create("Request Sent!", "Reset password email has been sent!", OnCloseInfoDialog);
			}
			else
			{
				// Error occurred
				Debug.LogError(error.ToDeveloperMessage(true));
				InfoDialog.Create("", error.ToString(), OnCloseInfoDialog);
			}
		}

		public void OnClick_Close()
		{
			Destroy(gameObject);
		}

		// Update is called once per frame
		void Update()
		{
			// Trigger login on enter key
			if (m_LoginCanvasGroup.interactable && Input.GetKeyDown(KeyCode.Return))
				OnClick_Login();
		}

		/// <summary>
		/// Creates and presents the LoginScreen dialog to the user.
		/// </summary>
		/// <param name="onCloseCB">The callback fired when the dialog closes.</param>
		public static LoginScreen Create(System.Action onCloseCB=null)
		{
			GameObject goDialog = Instantiate(Resources.Load<GameObject>("Dialogs/CanvasLoginDialog"));
			LoginScreen dialog = goDialog.GetComponent<LoginScreen>();
			
			return dialog;
		}
	}
}
