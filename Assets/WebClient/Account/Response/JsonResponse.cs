using System;
using UnityEngine;

namespace FlexAS
{
	/// <summary>
	/// Standardized response from the web server.
	/// All json web objects should inherit from this class.
	/// 
	/// <para>"Code" is guaranteed to be returned.</para>
	/// <para>"Message" is NOT guaranteed to be returned.</para>
	/// 
	/// <para> Standard Codes: </para>
	/// <para> 0	= Bad Response / Failed to parse response. The server NEVER returns 0. </para>
	/// <para> 1	= Success. Derived response object should be valid. </para>
	/// <para> >1	= Contextual Success. Operating as expected. Possible user error codes. Derived response object may or may not contain data. </para>
	/// <para> -1	= Internal server error. Typically a failed database operation. </para>
	/// <para> -2	= Bad data. Bad data was sent to the server. Typically a client-side programming error. </para>
	/// <para> -3	= Invalid Session. User session token is invalid or expired. </para>
	/// <para> -4	= Application Not Found. Application ID or API Key is invalid.</para>
	/// </summary>
	public class JsonResponse
	{
		// JSON serialized fields
		[SerializeField] private int Code				= default;
		[SerializeField] private string Message			= default;

		// Accessors
		public int code				{ get { return Code;		} }
		public string message		{ get { return Message;		} }


		public static JsonResponse FromJson(string json)
		{
			return JsonUtility.FromJson<JsonResponse>(json);
		}

		public static T FromJson<T>(string json) where T : JsonResponse, new()
		{
			try
			{
				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);

				return new T();
			}
		}

		public string ToJson()
		{
			return JsonUtility.ToJson(this);
		}
	}
}
