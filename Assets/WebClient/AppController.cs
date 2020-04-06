using FlexAS;
using UnityEngine;

/// <summary>
/// This is a simple wrapper for the Account Services initialization and login procedure. Place on the scene to use.
/// <para> Steps: </para>
/// <para> 1. Initialize			- Starts Account Services. </para>
/// <para> 2. Login					- Logs into Account Services and creates localUser. </para>
/// <para> 3. FetchPostLoginData	- Fetches App data and downloads asset bundles. If user is logged in, fetches user profile. </para>
/// </summary>
public class AppController : MonoBehaviour
{
	private static AppController m_Instance;

	public static AppService appService				{ get; private set; }

	public static UserAccount localUser				{ get; private set; }

	public delegate void UpdateProgressCallback(string status, float progress);

	public static event System.Action onLoginStatusChangedCB;


	private void Awake()
	{
		if (m_Instance != null)
			throw new System.Exception("Only one instance of AppService is allowed!");

		m_Instance = this;

		DontDestroyOnLoad(gameObject);
	}

	/// <summary>
	/// Initializes account services for use.
	/// <para>NOTE: Must be called before using any service.</para>
	/// </summary>
	private void Start()
	{
		if (m_Instance == null)
			throw new System.NullReferenceException("AppService has not been instantiated on the scene!");

		appService = new AppService(OP2MissionHub.Systems.WebConfig.webAPI, m_Instance);

		localUser = new UserAccount(appService);
	}

	/// <summary>
	/// Executes log in for the specified user.
	/// </summary>
	/// <param name="cb">The result of the web request.</param>
	/// <param name="userName">The username of the account to log into.</param>
	/// <param name="password">The password of the account to log into, or "TOKEN" to use "remember me".</param>
	/// <param name="gameVersion">The version of the application.</param>
	/// <param name="shouldRememberMe">Should this user be able to log in without a password next time.</param>
	/// <param name="adminOverride">Allow login while in maintenance mode. User must be an admin.</param>
	public static void Login(LoginCallback cb, string userName, string password, string gameVersion, bool shouldRememberMe, bool adminOverride=false)
	{
		if (m_Instance == null)
			throw new System.NullReferenceException("AppService has not been instantiated on the scene!");

		CustomAuthenticator authenticator = new CustomAuthenticator(appService);

		// "TOKEN" is safe to use since real passwords must be at least 8 characters.
		if (password == "TOKEN")
			authenticator.LoginRememberMe((LoginResponse r, ErrorResponse e) => OnLoginResponse(cb, r, e), userName, gameVersion, shouldRememberMe, adminOverride);
		else
			authenticator.Login((LoginResponse r, ErrorResponse e) => OnLoginResponse(cb, r, e), userName, password, gameVersion, shouldRememberMe, adminOverride);
	}

	private static void OnLoginResponse(LoginCallback cb, LoginResponse response, ErrorResponse error)
	{
		// Store login as local user
		if (response.responseCode == LoginResponse.ResponseCode.Success)
		{
			localUser = new UserAccount(appService, response);

			appService.SetExceptionUser(localUser);

			onLoginStatusChangedCB?.Invoke();
		}
		
		cb(response, error);
	}

	/// <summary>
	/// Fetches user data.
	/// </summary>
	/// <param name="cb">The result of the web request.</param>
	/// <param name="progressCB">A callback to update the current progress of the request.</param>
	/*public static void FetchPostLoginData(UserAccount.FetchProfileCallback cb, UpdateProgressCallback progressCB)
	{
		if (m_Instance == null)
			throw new System.NullReferenceException("AppService has not been instantiated on the scene!");

		if (cb == null)
			throw new System.ArgumentNullException("cb");

		progressCB?.Invoke("Getting Profile", 1);

		// Fetch profile
		localUser.FetchProfile(cb);
	}*/

	public static void LogOut()
	{
		if (!localUser.isLoggedIn)
			return;

		localUser.LogOut(null);

		onLoginStatusChangedCB?.Invoke();
	}
}
