using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace FlexAS
{
	/// <summary>
	/// Represents the user's connection and persistent data.
	/// </summary>
	public class UserAccount
	{
		public enum BanType { None=0, Transaction=2, Login=4 };

		public enum ResendEmailResponseCode { Error, Success, AlreadyVerified };

		private AppService m_AppService;
		private MonoBehaviour m_CoroutineOwner;
		private Coroutine m_WebCoroutine;

		// Accessors
		internal uint userID													{ get; set; }
		internal string sessionToken											{ get; set; }

		public string displayName												{ get; private set; }
		//private BanType banType												{ get; set; }
		public DateTime lastLoginTime											{ get; private set; }

		public bool isLoggedIn													{ get { return !string.IsNullOrEmpty(sessionToken);						} }
		//public bool isBannedFromTransactions									{ get { return banType >= BanType.Transaction;							} }

		public bool isAdmin														{ get; private set; }

		public delegate void FetchProfileCallback(ErrorResponse error);
		public delegate void ResendEmailCallback(ResendEmailResponseCode code, ErrorResponse error);


		/// <summary>
		/// Constructor for local users that are not logged in.
		/// </summary>
		/// <param name="appService">The application to perform requests on.</param>
		public UserAccount(AppService appService)
		{
			if (appService == null || !appService.isInitialized)
				throw new Exception("AppService not initialized.");

			m_AppService = appService;
			m_CoroutineOwner = appService.coroutineOwner;
		}

		/// <summary>
		/// Constructor for logged in / local users.
		/// </summary>
		/// <param name="appService">The application to perform requests on.</param>
		/// <param name="response">The login response.</param>
		public UserAccount(AppService appService, LoginResponse response)
		{
			if (appService == null || !appService.isInitialized)
				throw new Exception("AppService not initialized.");

			if (response == null)
				throw new ArgumentNullException("response");

			m_AppService = appService;

			// Store core data for player profile
			userID = response.userID;
			sessionToken = response.sessionToken;
			displayName = response.displayName;
			lastLoginTime = DateTime.Parse(response.lastLoginTime);
			isAdmin = response.isAdmin;
			
			m_CoroutineOwner = appService.coroutineOwner;
		}

		/// <summary>
		/// Constructor for remote users.
		/// </summary>
		/// <param name="appService">The application to perform requests on.</param>
		/// <param name="userID">The ID of the remote user.</param>
		public UserAccount(AppService appService, uint userID)
		{
			if (appService == null || !appService.isInitialized)
				throw new Exception("AppService not initialized.");

			this.userID = userID;

			m_CoroutineOwner = appService.coroutineOwner;
		}

		/// <summary>
		/// Fetches the user's profile.
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		public void FetchProfile(FetchProfileCallback cb)
		{
			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", userID.ToString(CultureInfo.InvariantCulture));
			formData.Add("SessionToken", sessionToken);

			m_WebCoroutine = m_CoroutineOwner.StartCoroutine(OnFetchProfile(m_AppService.apiUrl + "user/get_private_profile.php", formData, cb));
		}

		private IEnumerator OnFetchProfile(string url, Dictionary<string, string> formData, FetchProfileCallback cb)
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

				GetProfileResponse response = GetProfileResponse.FromJson(request.downloadHandler.text);

				switch (response.code)
				{
					case 1:
						// TODO: Fill out profile
						cb?.Invoke(null);
						break;

					default:
						cb?.Invoke(WebConfig.GetUnknownError(request));
						break;
				}
			}
		}

		/// <summary>
		/// Resends a confirmation email to the user.
		/// <para>NOTE: This only works for user's created with CustomAuthentication.</para>
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		public void ResendVerificationEmail(ResendEmailCallback cb)
		{
			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("UserID", userID.ToString(CultureInfo.InvariantCulture));
			formData.Add("SessionToken", sessionToken);

			m_WebCoroutine = m_CoroutineOwner.StartCoroutine(OnResendEmailResponse(m_AppService.apiUrl + "verification/resend_email.php", formData, cb));
		}

		private IEnumerator OnResendEmailResponse(string url, Dictionary<string, string> formData, ResendEmailCallback cb)
		{
			using (UnityWebRequest request = WebConfig.Post(url, formData))
			{
				yield return request.SendWebRequest();

				m_WebCoroutine = null;

				ErrorResponse error = WebConfig.ProcessGenericErrors(request);

				if (error != null)
				{
					cb?.Invoke(ResendEmailResponseCode.Error, error);
					yield break;
				}

				JsonResponse response = JsonResponse.FromJson(request.downloadHandler.text);

				ResendEmailResponseCode code = (ResendEmailResponseCode)response.code;

				switch (response.code)
				{
					case 1:		cb?.Invoke(code, null);																						break;
					case 2:		cb?.Invoke(code, new ErrorResponse("Email already verified.", response.message, null, response.code));		break;
					default:	cb?.Invoke(code, WebConfig.GetUnknownError(request));														break;
				}
			}
		}

		/// <summary>
		/// Terminates the user's session and releases their data.
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		public void LogOut(LogoutCallback cb)
		{
			if (string.IsNullOrEmpty(sessionToken))
			{
				Debug.LogWarning("Cannot log out on users that aren't logged in.");
				return;
			}

			if (m_WebCoroutine != null)
			{
				Debug.LogError("A request is already in progress. Please wait for the operation to complete.");
				return;
			}

			// Terminate session
			CustomAuthenticator authenticator = new CustomAuthenticator(m_AppService);
			authenticator.LogOut(cb, userID, sessionToken);


			userID = 0;
			sessionToken = "";
			displayName = null;
			//banType = BanType.None;
			lastLoginTime = DateTime.UtcNow.AddDays(-1);
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
