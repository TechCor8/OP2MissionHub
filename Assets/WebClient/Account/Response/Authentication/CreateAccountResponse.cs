using UnityEngine;

namespace FlexAS
{
	public class CreateAccountResponse : JsonResponse
	{
		public enum ResponseCode { Error, Success, EmailBadFormat, EmailTaken, UserNameBadFormat, UserNameTaken, DisplayNameBadFormat, DisplayNameTaken, PasswordNeedsDiffChars,
									PasswordNeedsUpperNumberSymbolLower, PasswordTooSimilarToName, BadVersion, DownForMaintenance, AuthorizationFailed, BadLicense };

		// JSON serialized fields
		
		// Code:
		// 2 - Bad game version
		[SerializeField] private string ServerVersion		= default;

		// Code:
		// 6 - Down For Maintenance
		[SerializeField] private string DisplayMessage		= default;
		[SerializeField] private bool HasAdminAccess		= default;

		// -2 | Bad Data
		// -1 | Internal Error
		// 1  | Valid
		// 2  | Email Bad Format
		// 3  | Email Already Taken
		// 4  | UserName Bad Format
		// 5  | UserName Already Taken
		// 6  | DisplayName Bad Format
		// 7  | DisplayName Already Taken
		// 8  | Password needs different characters
		// 9  | Password needs Upper Number Symbol And Lower
		// 10 | Password too similar to name
		// 11 | Bad Version | $foundVersion
		// 12 | Down for maintenance
		// 13 | Authorization Failed
		// 14 | Bad License

		// Code:
		// 2 - Bad game version
		public string serverVersion		{ get { return ServerVersion;		} }

		// Code:
		// 6 - Down For Maintenance
		public string displayMessage	{ get { return DisplayMessage;		} }
		public bool hasAdminAccess		{ get { return HasAdminAccess;		} }

		public ResponseCode responseCode	{ get { return (ResponseCode)code;	} }


		public static new CreateAccountResponse FromJson(string json)
		{
			return JsonResponse.FromJson<CreateAccountResponse>(json);
		}
	}
}
