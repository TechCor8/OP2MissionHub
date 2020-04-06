using UnityEngine;

namespace OP2MissionHub.Data.Json
{
	/// <summary>
	/// Contains server response data that comes back for every request.
	/// </summary>
	[System.Serializable]
	public class JsonResponse
	{
		[SerializeField] private int Code			= default;
		[SerializeField] private string Message		= default;

		public int code				{ get { return Code;		}	}
		public string message		{ get { return Message;		}	}


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
			catch (System.Exception ex)
			{
				Debug.LogException(ex);

				return new T();
			}
		}
	}
}
