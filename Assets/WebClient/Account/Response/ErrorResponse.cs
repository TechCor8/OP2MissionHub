using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace FlexAS
{
	public class ErrorResponse
	{
		public bool isNetworkError		{ get; private set; }
		public bool isHttpError			{ get; private set; }

		public string apiName			{ get; private set; }		// The apiName e.g. "authenticate.php"
		public long httpResponseCode	{ get; private set; }		// The http response code e.g. 404, 200
		public int jsonCode				{ get; private set; }		// The JSON response code

		public string errorMessage		{ get; private set; }		// User-friendly error message.
		public string errorDetails		{ get; private set; }


		public ErrorResponse(string message, string details, UnityWebRequest request=null, int jsonCode=0)
		{
			if (request != null)
			{
				isNetworkError = request.isNetworkError;
				isHttpError = request.isHttpError;

				apiName = Path.GetFileNameWithoutExtension(new System.UriBuilder(request.url).Path);
				httpResponseCode = request.responseCode;
			}
			this.jsonCode = jsonCode;

			errorMessage = message;
			errorDetails = details;
		}

		public string ToDeveloperMessage(bool includeDetails)
		{
			// Custom error messages don't have an apiName (null request)
			if (apiName == null)
				return errorDetails;

			if (isNetworkError)
					return "Network Error: " + errorDetails;

			// Handle HTTP errors and critical JSON errors
			string message;

			if (isHttpError)
				message = "HTTP Error: ";
			else
				message = "Internal Server Error: ";

			message += "\nAPI: " + apiName;
			message += "\nCode: " + (isHttpError ? httpResponseCode : jsonCode);
			message += "\nVersion: " + WebConfig.version;

			if (includeDetails)
				message += "\nDetails: " + errorDetails;

			return message;
		}

		public override string ToString()
		{
			return ToDeveloperMessage(false);//errorMessage;
		}
	}
}
