using UnityEngine;

namespace FlexAS
{
	public class LoginResponse : JsonResponse
	{
		public enum ResponseCode { Error, Success, BadVersion, BadCredentials, AccountLocked, UserBanned, DownForMaintenance, NeedAccount, BadLicense };

		// JSON serialized fields
		
		// Code:
		// 2 - Bad game version
		[SerializeField] private string ServerVersion			= default;

		// Code:
		// 5 - User banned
		[SerializeField] private string BanDate					= default;
		[SerializeField] private uint BanRestoreDate			= default;
		[SerializeField] private string BanReason				= default;

		// Code:
		// 6 - Down For Maintenance
		[SerializeField] private string DisplayMessage			= default;
		[SerializeField] private bool HasAdminAccess			= default;

		// Code:
		// 1 - Success
		[SerializeField] private uint UserID					= default;
		[SerializeField] private string SessionToken			= default;
		[SerializeField] private string RememberMeToken			= default;
		[SerializeField] private bool EmailVerified				= default;
		[SerializeField] private string DisplayName				= default;
		[SerializeField] private string LastLoginTime			= default;
		[SerializeField] private bool IsAdmin					= default;

		// All Codes:
		// -2 = Bad data
		// 1 = Success
		// 2 = Bad game version
		// 3 = Incorrect username or password
		// 4 = Account locked
		// 5 = User banned
		// 6 = Down for maintenance
		// 7 = Need Account (3rd party auth provider)
		// 8 = Bad License (3rd party auth provider)

		// Code:
		// 2 - Bad game version
		public string serverVersion		{ get { return ServerVersion;		} }

		// Code:
		// 5 - User banned
		public string banDate			{ get { return BanDate;				} }
		public uint banRestoreDate		{ get { return BanRestoreDate;		} }
		public string banReason			{ get { return BanReason;			} }

		// Code:
		// 6 - Down For Maintenance
		public string displayMessage	{ get { return DisplayMessage;		} }
		public bool hasAdminAccess		{ get { return HasAdminAccess;		} }

		// Code:
		// 1 - Success
		public uint userID				{ get { return UserID;				} }
		public string sessionToken		{ get { return SessionToken;		} }
		public string rememberMeToken	{ get { return RememberMeToken;		} }
		public bool emailVerified		{ get { return EmailVerified;		} }
		public string displayName		{ get { return DisplayName;			} }
		public string lastLoginTime		{ get { return LastLoginTime;		} }
		public bool isAdmin				{ get { return IsAdmin;				} }

		public ResponseCode responseCode	{ get { return (ResponseCode)code;	} }


		public static new LoginResponse FromJson(string json)
		{
			return JsonResponse.FromJson<LoginResponse>(json);
		}
	}
}
