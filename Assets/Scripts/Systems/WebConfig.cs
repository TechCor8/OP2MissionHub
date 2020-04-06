using OP2MissionHub.Data.Json;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace OP2MissionHub.Systems
{
    public class WebConfig : MonoBehaviour
    {
		private const string defaultHost = "https://krealm.xyz/op2missions/";

		public static string webHost { get; private set; }

		public static string webAPI { get { return webHost + "api/v1/"; } }


		private void Awake()
		{
			// Load config file
			try
			{
				ReadConfig();
			}
			catch (System.Exception)
			{
				WriteDefaultConfig();
				webHost = defaultHost;
			}
		}

		private void ReadConfig()
		{
			string[] lines = File.ReadAllLines("hub.cfg");

			foreach (string line in lines)
			{
				if (line.Length < 1) continue;
				if (line[0] == '#') continue; // If first character is a '#', it is a comment line. Ignore it.

				int equalsIndex = line.IndexOf('=');
				if (equalsIndex >= 0)
				{
					// Assignment line
					string key = line.Substring(0, equalsIndex);
					string value = line.Substring(equalsIndex+1);

					switch (key)
					{
						case "host":	webHost = value;		break;
					}
				}
			}
		}

		private void WriteDefaultConfig()
		{
			string[] contents = new string[]
			{
				"#-------------------------------------------------",
				"# OP2 Mission Hub Configuration",
				"#-------------------------------------------------",
				"",
				"# Mission Host URL",
				"host=" + defaultHost
			};

			File.WriteAllLines("hub.cfg", contents);
		}

		/// <summary>
		/// Creates an error string based on a web request. If there are no errors, returns null.
		/// </summary>
		/// <param name="request">The request to get errors from.</param>
		public static string GetErrorString(UnityWebRequest request)
		{
			if (request.isNetworkError)
				return "There was a communication error: " + request.error;
			else if (request.isHttpError)
				return "There was an HTTP error: " + request.error;
			else
			{
				try
				{
					JsonResponse response = JsonResponse.FromJson(request.downloadHandler.text);

					switch (response.code)
					{
						case 0:		return "There was an unknown error: " + request.downloadHandler.text;
						case -1:	return "There was an internal server error: " +  response.message;
						case -2:	return "Bad data was sent to server: " + response.message;
						case -3:	return "Session has expired: " + response.message;
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogError(ex);
					return "There was an unknown error: " + request.downloadHandler.text;
				}
			}

			return null;
		}

		public static bool DidSessionExpire(UnityWebRequest request)
		{
			if (request.isNetworkError || request.isHttpError)
				return false;

			JsonResponse response = JsonResponse.FromJson(request.downloadHandler.text);
			return response.code == -3;
		}
    }
}
