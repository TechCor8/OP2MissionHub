using UnityEngine;

namespace FlexAS
{
	public class ResetPasswordResponse : JsonResponse
	{
		public enum ResponseCode { Error, Success, InvalidEmail, ResetLimit };

		[SerializeField] private uint TimeSinceLastEmail		= default;

		// All Codes:
		// -2 = Bad data
		// -1 = Update Failure
		// 1 = Success
		// 2 = Invalid email
		// 3 = Reset Limit
		// 4 = Could not send email

		// Code:
		// 3 - Reset Limit
		public uint timeSinceLastEmail		{ get { return TimeSinceLastEmail;	} }

		public ResponseCode responseCode	{ get { return (ResponseCode)code;	} }


		public static new ResetPasswordResponse FromJson(string json)
		{
			return JsonResponse.FromJson<ResetPasswordResponse>(json);
		}
	}
}
