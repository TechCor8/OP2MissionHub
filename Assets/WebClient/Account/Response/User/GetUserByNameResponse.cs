using UnityEngine;

namespace FlexAS
{
	public class GetUserByNameResponse : JsonResponse
	{
		[SerializeField] private uint UserID		= default;

		public uint userID		{ get { return UserID;	} }

		
		public static new GetUserByNameResponse FromJson(string json)
		{
			return JsonResponse.FromJson<GetUserByNameResponse>(json);
		}
	}
}
