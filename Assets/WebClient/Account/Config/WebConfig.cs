using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FlexAS
{
	/// <summary>
	/// Contains common web operations and configurations.
	/// <para>Contains the web url for reaching the master server for determining an application's service Url.</para>
	/// </summary>
	internal sealed class WebConfig
	{
		/// <summary>
		/// Connection host. Used to find service host.
		/// </summary>
		internal static string webHost			{ get { return "https://krealm.xyz/op2missions/";					} }

		/// <summary>
		/// API Url
		/// </summary>
		internal static string apiUrl			{ get { return webHost + apiSubdirectory;							} }

		/// <summary>
		/// API subdirectory
		/// </summary>
		internal static string apiSubdirectory	{ get { return "api/v" + version + "/";								} }

		/// <summary>
		/// API Version
		/// </summary>
		internal static int version				{ get { return 1;													} }


		// Creates a post request
		internal static UnityWebRequest Post(string url, Dictionary<string, string> postVars)
		{
			UnityWebRequest request = UnityWebRequest.Post(url, postVars);

			request.chunkedTransfer = false;

			return request;
		}

		// Creates a get request
		internal static UnityWebRequest Get(string url)
		{
			return UnityWebRequest.Get(url);
		}

		/// <summary>
		/// Creates an error response based on a web request. If there are no errors, returns null.
		/// </summary>
		/// <param name="request">The request to get errors from.</param>
		/// <returns>The error object.</returns>
		internal static ErrorResponse ProcessGenericErrors(UnityWebRequest request)
		{
			if (request.isNetworkError)
				return new ErrorResponse("There was a communication error.", request.error, request);
			else if (request.isHttpError)
				return new ErrorResponse("There was an HTTP error.", request.error, request);
			else
			{
				try
				{
					JsonResponse response = JsonResponse.FromJson(request.downloadHandler.text);

					switch (response.code)
					{
						case 0:		return new ErrorResponse("There was an unknown error.", request.downloadHandler.text, request, response.code);
						case -1:	return new ErrorResponse("There was an internal server error.", response.message, request, response.code);
						case -2:	return new ErrorResponse("Bad data was sent to server.", response.message, request, response.code);
						case -3:	return new ErrorResponse("Session has expired.", response.message, request, response.code);
						case -4:	return new ErrorResponse("Application not found.", response.message, request, response.code);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
					Debug.LogError(request.downloadHandler.text);
					return new ErrorResponse("There was an unknown error.", request.downloadHandler.text, request);
				}
			}

			return null;
		}

		/// <summary>
		/// Gets an "unknown error" object for a request.
		/// Used after external code has processed the request response and received an unknown result.
		/// </summary>
		/// <param name="request">The request used to generate the error.</param>
		/// <returns>The generated error object.</returns>
		internal static ErrorResponse GetUnknownError(UnityWebRequest request)
		{
			JsonResponse response = JsonResponse.FromJson(request.downloadHandler.text);

			return new ErrorResponse("There was an unknown error.", response.message, request, response.code);
		}
	}
}
