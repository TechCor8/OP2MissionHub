using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace FlexAS
{
	public delegate void CreateAccountCallback(CreateAccountResponse response, ErrorResponse error);
	public delegate void LoginCallback(LoginResponse response, ErrorResponse error);
	public delegate void ResetPasswordCallback(ResetPasswordResponse response, ErrorResponse error);
	public delegate void LogoutCallback(ErrorResponse error);

	/// <summary>
	/// The authenticator for custom authentication.
	/// <para>Used for creating and logging into user accounts using the custom auth method.</para>
	/// </summary>
	public sealed class CustomAuthenticator
	{
		private AppService m_AppService;
		private MonoBehaviour m_CoroutineOwner;
		private Coroutine m_WebCoroutine;

		// Remember me settings for local machine
		public static bool rememberMe											{ get { return PlayerPrefs.GetInt("FlexAS_Login_RememberMe", 0) != 0;		} }
		public static string rememberMeUserName									{ get { return PlayerPrefs.GetString("FlexAS_Login_UserName");				} }
		

		/// <summary>
		/// Creates a custom authenticator.
		/// </summary>
		/// <param name="appService">The application to perform requests on.</param>
		public CustomAuthenticator(AppService appService)
		{
			if (appService == null || !appService.isInitialized)
				throw new System.Exception("AppService not initialized.");

			m_AppService = appService;
			m_CoroutineOwner = appService.coroutineOwner;
		}

		/// <summary>
		/// Creates a user account.
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		/// <param name="email">The email of the account to create.</param>
		/// <param name="userName">The username of the account to create.</param>
		/// <param name="displayName">The display name of the account to create.</param>
		/// <param name="password">The password of the account to create.</param>
		/// <param name="gameVersion">The version of the application.</param>
		public void CreateAccount(CreateAccountCallback cb, string email, string userName, string displayName, string password, string gameVersion)
		{
			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("Email", email);
			formData.Add("UserName", userName);
			formData.Add("DisplayName", displayName);
			formData.Add("Password", password);
			formData.Add("GameVersion", gameVersion);

			m_WebCoroutine = m_CoroutineOwner.StartCoroutine(OnCreateAccountResponse(m_AppService.apiUrl + "authentication/custom/create_account.php", formData, cb, gameVersion));
		}

		private IEnumerator OnCreateAccountResponse(string url, Dictionary<string, string> formData, CreateAccountCallback cb, string gameVersion)
		{
			using (UnityWebRequest request = WebConfig.Post(url, formData))
			{
				yield return request.SendWebRequest();

				m_WebCoroutine = null;

				ErrorResponse error = WebConfig.ProcessGenericErrors(request);

				if (error != null)
				{
					cb?.Invoke(new CreateAccountResponse(), error);
					yield break;
				}
				
				CreateAccountResponse response = CreateAccountResponse.FromJson(request.downloadHandler.text);

				// -2 | Bad Data
				// -1 | Internal Error
				// 1 | Valid
				// 2 | Email Bad Format
				// 3 | Email Already Taken
				// 4 | UserName Bad Format
				// 5 | UserName Already Taken
				// 6 | DisplayName Bad Format
				// 7 | DisplayName Already Taken
				// 8 | Password needs different characters
				// 9 | Password needs Upper Number Symbol And Lower
				// 10 | Password too similar to name
				// 11 | Bad Version | $foundVersion
				// 12 | Down for maintenance
				switch (response.code)
				{
					case 1:		cb?.Invoke(response, null);																														break;

					case 2:		cb?.Invoke(response, new ErrorResponse("Email format is invalid.", response.message, null, response.code));										break;
					case 3:		cb?.Invoke(response, new ErrorResponse("Email already in use.", response.message, null, response.code));										break;
					case 4:		cb?.Invoke(response, new ErrorResponse("User name must contain only letters and numbers.", response.message, null, response.code));				break;
					case 5:		cb?.Invoke(response, new ErrorResponse("User name already exists.", response.message, null, response.code));									break;
					case 6:		cb?.Invoke(response, new ErrorResponse("Display name must contain only letters and numbers.", response.message, null, response.code));			break;
					case 7:		cb?.Invoke(response, new ErrorResponse("Display name already exists.", response.message, null, response.code));									break;
					case 8:		cb?.Invoke(response, new ErrorResponse("Password must contain at least 8 different characters.", response.message, null, response.code));		break;
					case 9:		cb?.Invoke(response, new ErrorResponse("Password must contain at least 1 upper case letter, number or symbol and 1 lower case letter.", response.message, null, response.code));	break;
					case 10:	cb?.Invoke(response, new ErrorResponse("Password too similar to name.", response.message, null, response.code));								break;
					case 11:
						cb?.Invoke(response, new ErrorResponse("Your app version is out of date.\n\nCurrent Version: " + response.serverVersion + "\nYour Version: " + gameVersion,
							response.message, null, response.code));
						break;

					case 12:
						// Down for maintenance
						cb?.Invoke(response, new ErrorResponse(response.message, response.displayMessage, null, response.code));
						break;
				
					default:
						// Unknown code error
						cb?.Invoke(response, WebConfig.GetUnknownError(request));
						break;
				}
			}
		}

		/// <summary>
		/// Log in to the specified user account.
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		/// <param name="userName">The username of the account to log into.</param>
		/// <param name="password">The password of the account to log into.</param>
		/// <param name="gameVersion">The version of the application.</param>
		/// <param name="shouldRememberMe">Should this user be able to log in without a password next time.</param>
		/// <param name="adminOverride">Allow login while in maintenance mode. User must be an admin.</param>
		public void Login(LoginCallback cb, string userName, string password, string gameVersion, bool shouldRememberMe, bool adminOverride=false)
		{
			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserName", userName);
			formData.Add("Password", password);
			formData.Add("GameVersion", gameVersion);
			formData.Add("RememberMe", shouldRememberMe ? "1": "0");
			formData.Add("AdminOverride", adminOverride ? "1" : "0");

			m_WebCoroutine = m_CoroutineOwner.StartCoroutine(OnLoginResponse(m_AppService.apiUrl + "authentication/custom/authenticate.php", formData, cb, userName, gameVersion));
		}

		/// <summary>
		/// Log in using the stored "remember me" token.
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		/// <param name="userName">The username of the account to log into.</param>
		/// <param name="gameVersion">The version of the application.</param>
		/// <param name="shouldRememberMe">Should this user be able to log in without a password next time.</param>
		/// <param name="adminOverride">Allow login while in maintenance mode. User must be an admin.</param>
		public void LoginRememberMe(LoginCallback cb, string userName, string gameVersion, bool shouldRememberMe, bool adminOverride=false)
		{
			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			Dictionary<string, string> formData = new Dictionary<string, string>();
			
			if (PlayerPrefs.HasKey("FlexAS_Login_RememberMeToken"))
				formData.Add("RememberMeToken", PlayerPrefs.GetString("FlexAS_Login_RememberMeToken"));

			formData.Add("UserName", userName);
			formData.Add("Password", "TOKEN");
			formData.Add("GameVersion", gameVersion);
			formData.Add("RememberMe", shouldRememberMe ? "1": "0");
			formData.Add("AdminOverride", adminOverride ? "1" : "0");

			m_WebCoroutine = m_CoroutineOwner.StartCoroutine(OnLoginResponse(m_AppService.apiUrl + "authentication/custom/authenticate.php", formData, cb, userName, gameVersion));
		}

		private IEnumerator OnLoginResponse(string url, Dictionary<string, string> formData, LoginCallback cb, string userName, string gameVersion)
		{
			using (UnityWebRequest request = WebConfig.Post(url, formData))
			{
				yield return request.SendWebRequest();

				m_WebCoroutine = null;

				ErrorResponse error = WebConfig.ProcessGenericErrors(request);

				if (error != null)
				{
					cb?.Invoke(new LoginResponse(), error);
					yield break;
				}
				
				LoginResponse response = LoginResponse.FromJson(request.downloadHandler.text);

				switch (response.code)
				{
					case 1:
						// Save remember me credentials
						PlayerPrefs.SetInt("FlexAS_Login_RememberMe", !string.IsNullOrEmpty(response.rememberMeToken) ? 1 : 0);
						if (!string.IsNullOrEmpty(response.rememberMeToken))
						{
							PlayerPrefs.SetString("FlexAS_Login_UserName", userName);
							PlayerPrefs.SetString("FlexAS_Login_RememberMeToken", response.rememberMeToken);
						}
						PlayerPrefs.Save();

						cb?.Invoke(response, null);

						break;

					case 2:
						cb?.Invoke(response, new ErrorResponse("Your app version is out of date.\n\nCurrent Version: " + response.serverVersion + "\nYour Version: " + gameVersion,
							response.message, null, response.code));
						break;

					case 5:
						string accountRestoreTime = "";
						if (response.banRestoreDate > 0)
							accountRestoreTime = "\n\nAccount will be restored in " + DateUtility.GetFormattedTimeFromSeconds(response.banRestoreDate);
						cb?.Invoke(response, new ErrorResponse("This account has been banned.\n\nAdmin Message:\n\"" + response.banReason + "\"" + accountRestoreTime, response.message, null, response.code));
						break;

					case 3: cb?.Invoke(response, new ErrorResponse("Incorrect username or password", response.message, null, response.code));	break;
					case 4: cb?.Invoke(response, new ErrorResponse("Account Locked", response.message, null, response.code));					break;
					case 6:
						// Down for maintenance
						cb?.Invoke(response, new ErrorResponse(response.message, response.displayMessage, null, response.code));
						break;

					default:
						// Unknown code error
						cb?.Invoke(response, WebConfig.GetUnknownError(request));
						break;
				}
			}
		}

		/// <summary>
		/// Sends a password reset to the specified email address. Email address must be on file.
		/// <para>NOTE: User does not need to be logged in to perform this action.</para>
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		/// <param name="email">The email of the account to reset.</param>
		public void RequestPasswordReset(ResetPasswordCallback cb, string email)
		{
			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("Email", email);

			m_WebCoroutine = m_CoroutineOwner.StartCoroutine(OnPasswordRequestResponse(m_AppService.apiUrl + "reset/request_password_reset.php", formData, cb));
		}

		private IEnumerator OnPasswordRequestResponse(string url, Dictionary<string, string> formData, ResetPasswordCallback cb)
		{
			using (UnityWebRequest request = WebConfig.Post(url, formData))
			{
				yield return request.SendWebRequest();

				m_WebCoroutine = null;

				ErrorResponse error = WebConfig.ProcessGenericErrors(request);

				if (error != null)
				{
					cb?.Invoke(new ResetPasswordResponse(), error);
					yield break;
				}
				
				ResetPasswordResponse response = ResetPasswordResponse.FromJson(request.downloadHandler.text);

				switch (response.code)
				{
					case 1:
						cb?.Invoke(response, null);
						break;

					case 2:
						cb?.Invoke(response, new ErrorResponse("The email entered is not valid.", response.message, null, response.code));
						break;

					case 3:
						string time = DateUtility.GetFormattedTimeFromSeconds(response.timeSinceLastEmail);
						cb?.Invoke(response, new ErrorResponse("Reset limit reached.\n\nTry again in " + time + ".", response.message, null, response.code));
						break;

					default:
						// Unknown code error
						cb?.Invoke(response, WebConfig.GetUnknownError(request));
						break;
				}
			}
		}

		/// <summary>
		/// Terminates a user's session.
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		/// <param name="userID">The ID of the user to log out.</param>
		/// <param name="sessionToken">The session token of the user to log out.</param>
		public void LogOut(LogoutCallback cb, uint userID, string sessionToken)
		{
			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", userID.ToString(CultureInfo.InvariantCulture));
			formData.Add("SessionToken", sessionToken);

			m_WebCoroutine = m_CoroutineOwner.StartCoroutine(OnLogOutResponse(m_AppService.apiUrl + "authentication/logout.php", formData, cb));
		}

		private IEnumerator OnLogOutResponse(string url, Dictionary<string, string> formData, LogoutCallback cb)
		{
			using (UnityWebRequest request = WebConfig.Post(url, formData))
			{
				yield return request.SendWebRequest();

				m_WebCoroutine = null;

				ErrorResponse error = WebConfig.ProcessGenericErrors(request);

				if (error != null)
				{
					cb?.Invoke(error);
					yield break;
				}
				
				JsonResponse response = JsonResponse.FromJson(request.downloadHandler.text);

				switch (response.code)
				{
					case 1:		cb?.Invoke(null);									break;
					default:	cb?.Invoke(WebConfig.GetUnknownError(request));		break;
				}
			}
		}

		/// <summary>
		/// Stops an active request.
		/// </summary>
		public void Abort()
		{
			if (m_WebCoroutine != null)
				m_CoroutineOwner.StopCoroutine(m_WebCoroutine);

			m_WebCoroutine = null;
		}
	}
}
