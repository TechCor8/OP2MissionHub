using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FlexAS
{
	/// <summary>
	/// The connection to the remote application.
	/// </summary>
	public sealed class AppService
	{
		//private ExceptionReporter m_ExceptionReporter;

		internal string apiUrl					{ get; private set; }

		public bool isInitialized				{ get; private set; }

		internal MonoBehaviour coroutineOwner	{ get; private set; }

		public delegate void GetUserCallback(UserAccount user, ErrorResponse error);


		/// <summary>
		/// Initializes account services.
		/// <para>Must be called before performing any operations in account services.</para>
		/// </summary>
		/// <param name="coroutineOwner">The default owner of web request coroutines. A persistent object is recommended.</param>
		/// <param name="reportExceptions">Set to true to report exceptions to the server.</param>
		public AppService(string apiUrl, MonoBehaviour coroutineOwner, bool reportExceptions=true)
		{
			if (coroutineOwner == null)
				throw new System.ArgumentNullException("coroutineOwner");

			this.coroutineOwner = coroutineOwner;

			// Set initialized flag and apiUrl
			isInitialized = true;
			this.apiUrl = apiUrl;

			// Enable exception reporter
			//if (reportExceptions)
			//	m_ExceptionReporter = new ExceptionReporter(this);
		}

		/// <summary>
		/// Gets a user by their display name.
		/// </summary>
		/// <param name="cb">The result of the web request.</param>
		/// <param name="displayName">The display name of the user to get.</param>
		public void GetUserByName(GetUserCallback cb, string displayName)
		{
			if (!isInitialized)
				throw new System.Exception("Account Services not initialized.");

			// Query server for app authorization and web URL
			Dictionary<string, string> formData = new Dictionary<string, string>();
			formData.Add("DisplayName", displayName);

			coroutineOwner.StartCoroutine(OnGetUserByNameResponse(apiUrl + "user/get_user_by_name.php", formData, cb));
		}

		private IEnumerator OnGetUserByNameResponse(string url, Dictionary<string, string> formData, GetUserCallback cb)
		{
			using (UnityWebRequest request = WebConfig.Post(url, formData))
			{
				yield return request.SendWebRequest();

				ErrorResponse error = WebConfig.ProcessGenericErrors(request);

				if (error != null)
				{
					cb?.Invoke(null, error);
					yield break;
				}
				
				GetUserByNameResponse response = GetUserByNameResponse.FromJson(request.downloadHandler.text);

				switch (response.code)
				{
					case 1:
						cb?.Invoke(new UserAccount(this, response.userID), null);
						break;
					
					default:
						// Unknown code error
						cb?.Invoke(null, WebConfig.GetUnknownError(request));
						break;
				}
			}
		}

		/// <summary>
		/// Sets the user associated with reported exceptions.
		/// <para>NOTE: User must be logged in.</para>
		/// </summary>
		/// <param name="userAccount">The user account to associate with exceptions.</param>
		public void SetExceptionUser(UserAccount userAccount)
		{
			//if (m_ExceptionReporter == null)
			//	return;
			
			//m_ExceptionReporter.SetUserAccount(userAccount);
		}
	}
}
