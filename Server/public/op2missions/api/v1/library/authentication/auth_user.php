<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// auth_user.php
// Contains generic functions for authenticating a user, common to all auth types.
// These functions should only be used in authenticating scripts.
////////////////////////////////////////////////////////////////////////////////////////////////////
require_once INCLUDE_PATH_PUBLIC . '/library/common/gen_uuid.php';
require_once INCLUDE_PATH_PUBLIC . '/library/constants/app_constants.php';


class AuthUser
{
	private $db = null;
	private $error = null;
	
	private $versionInfo = null;
	
	private $displayNameFromDB = null;
	private $userID = null;
	private $rememberMeTokenHash = null;
	private $passwordHash = null;
	private $loginAttempts = null;
	private $lastLoginAttempt = null;
	private $email = null;
	private $emailVerified = null;
	private $banTypeID = null;
	private $banDate = null;
	private $banRestoreDate = null;
	private $banReason = null;
	private $isAdmin = null;
	
	private $hasAdminAccess = false;
	
	
	public function __construct($db)
	{
		$this->db = $db;
	}
	
	public function GetErrorMessage()		{ return $this->error;						}
	
	// User data
	public function UserExists()			{ return isset($this->userID);				}
	public function GetDisplayName()		{ return $this->displayNameFromDB;			}
	public function GetUserID()				{ return $this->userID;						}
	public function GetRememberMeTokenHash(){ return $this->rememberMeTokenHash;		}
	public function GetPasswordHash()		{ return $this->passwordHash;				}
	public function GetLoginAttempts()		{ return $this->loginAttempts;				}
	public function GetLastLoginAttempt()	{ return $this->lastLoginAttempt;			}
	public function GetEmail()				{ return $this->email;						}
	public function GetEmailVerified()		{ return $this->emailVerified;				}
	public function GetBanTypeID()			{ return $this->banTypeID;					}
	public function GetBanDate()			{ return $this->banDate;					}
	public function GetBanRestoreDate()		{ return $this->banRestoreDate;				}
	public function GetBanReason()			{ return $this->banReason;					}
	
	// App data
	public function IsInMaintenanceMode()	{ return $this->versionInfo[AppConstants::MAINTENANCE_MODE];				}
	public function IsAllowingAdminAccess()	{ return $this->versionInfo[AppConstants::MAINTENANCE_MODE_LOGIN_ACCESS];	}
	
	
	
	public function FetchAppData()
	{
		// Get app version and maintenance status
		$appConstants = new AppConstants($this->db);
		
		$constants = $appConstants->GetConstants(array(AppConstants::APP_VERSION,
														AppConstants::MAINTENANCE_MODE,
														AppConstants::MAINTENANCE_MODE_LOGIN_ACCESS));
		
		
		$this->versionInfo = $constants;
		
		return true;
	}
	
	public function FetchUserData($authTypeID, $userName)
	{
		// If logins are completely disabled, early out
		if ($this->IsInMaintenanceMode() AND !$this->IsAllowingAdminAccess())
			return true;
		
		// Get the UserID and password hash from the table
		$this->db->Prepare('SELECT DisplayName, UserID, RememberMeToken, Password, FailedLoginAttempts, UNIX_TIMESTAMP() - UNIX_TIMESTAMP(LastLoginAttempt), Email, EmailVerified, BanTypeID, BanStartTime, GREATEST(UNIX_TIMESTAMP(BanEndTime) - UNIX_TIMESTAMP(),0), BanReason, IsAdmin'
						. ' FROM Users'
						. ' WHERE AuthTypeID=:AuthTypeID AND UserName=:UserName');
		$params = array(':AuthTypeID' => $authTypeID,
						':UserName' => $userName);

		$this->db->Execute($params);
		
		$row = $this->db->FetchArray();
		if ($row)
		{
			$this->displayNameFromDB = $row[0];
			$this->userID = $row[1];
			$this->rememberMeTokenHash = $row[2];
			$this->passwordHash = $row[3];
			$this->loginAttempts = $row[4];
			$this->lastLoginAttempt = $row[5];
			$this->email = $row[6];
			$this->emailVerified = $row[7];
			$this->banTypeID = $row[8];
			$this->banDate = $row[9];
			$this->banRestoreDate = $row[10];
			$this->banReason = $row[11];
			$this->isAdmin = $row[12];
			
			if (!$this->IsUserBanned() AND $this->IsAccountLocked())
			{
				// Report failed login on User row, but we won't update the login attempt time
				$this->db->Prepare('UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts+1 WHERE UserID=:UserID');
				$params = array(':UserID' => $this->userID);
				$this->db->Execute($params);
			}
			
			// If game is in maintenance mode, get admin login access for this user
			if ($this->IsInMaintenanceMode())
				$this->hasAdminAccess = $this->FetchAdminLoginAccess();
		}
		
		return true;
	}
	
	private function FetchAdminLoginAccess()
	{
		// Check if user has admin login access
		$this->db->Prepare('SELECT MaintenanceLogin FROM UserAdmins WHERE UserID=:UserID');
		$params = array(':UserID' => $this->userID);
		$this->db->Execute($params);
		
		$row = $this->db->FetchArray();
		
		// If no row, this user is not an admin
		if (!$row)
			return false;
		
		// If MaintenanceLogin is not true, this admin does not have maintenance access
		if (!$row[0])
			return false;
		
		return true;
	}
	
	public function HasValidVersion($appVersion)
	{
		// Allow bypassing of version check
		if (!isset($appVersion))
			return true;
		
		return $this->versionInfo[AppConstants::APP_VERSION] === $appVersion;
	}
	
	public function IsUserBanned()
	{
		return isset($this->banDate) AND (isset($this->banRestoreDate) == FALSE OR $this->banRestoreDate > 0) AND $this->banTypeID >= 4;
	}
	
	public function IsAccountLocked()
	{
		return $this->loginAttempts > 4 and $this->lastLoginAttempt < 60;
	}
	
	public function GetBadVersionResponse($responseCode=2)
	{
		$foundVersion = $this->versionInfo[AppConstants::APP_VERSION];
		
		$data = array('ServerVersion' => $foundVersion);
		
		return CreateResponse($responseCode, $data, 'Bad app version');
	}
	
	public function GetLockedResponse()
	{
		return CreateErrorResponse(4, 'Account locked');
	}
	
	public function GetBannedResponse()
	{
		$data = array('BanDate' => $this->banDate,
					'BanRestoreDate' => $this->banRestoreDate,
					'BanReason' => $this->banReason);
		
		return CreateResponse(5, $data, 'User banned');
	}
	
	public function GetMaintenanceResponse($responseCode=6)
	{
		$data = array('HasAdminAccess' => $this->hasAdminAccess ? 1 : 0);
					
		return CreateResponse($responseCode, $data, 'Down for Maintenance');
	}
	
	public function ExecuteSuccessfulLogin($rememberMe, $adminOverride)
	{
		// If maintenance mode is active, user must have admin access rights to continue
		if ($this->IsInMaintenanceMode())
		{
			// If user doesn't have admin access OR admin override is not set by user, show "down for maintenance"
			if (!$this->hasAdminAccess OR !isset($adminOverride))
				return $this->GetMaintenanceResponse();
		}
		
		$sessionToken = generate_password(38);
		//$sessionToken = password_hash($sessionToken, PASSWORD_DEFAULT);
		
		// Generate new 'remember me' hash, or stop remembering
		if ($rememberMe == 0)
			$rememberMeToken = null;
		else
		{
			$rememberMeToken = generate_password(38);
			$rememberMeTokenHash = password_hash($rememberMeToken, PASSWORD_DEFAULT);
		}
		
		// Get last login time
		// Fetch user row
		$this->db->Prepare('SELECT SessionLastUpdated'
						. ' FROM Users'
						. ' WHERE Users.UserID=:UserID');
		$params = array(':UserID' => $this->userID);

		if (!$this->db->Execute($params))
		{
			echo CreateErrorResponse(-1, 'Failed to execute Users selection.');
			exit;
		}

		if ($row = $this->db->FetchArray())
		{
			$lastLoginTime = $row[0];
		}
		else
		{
			return CreateErrorResponse(-1, 'User not found.');
		}
	
		// Report successful login on User row
		$this->db->Prepare('UPDATE Users SET SessionToken=:SessionToken, SessionLastUpdated=NOW(), RememberMeToken=:RememberMeToken, FailedLoginAttempts=0, LastLoginAttempt=NOW(), LastLoginTime=NOW() where UserID = :UserID');
		$params = array(':SessionToken' => $sessionToken,
						':RememberMeToken' => $rememberMeTokenHash,
						':UserID' => $this->userID);
		if (!$this->db->Execute($params))
			return CreateErrorResponse(2, $this->GetErrorMessage());
		
		// Return success response
		$data = array('UserID' => $this->userID,
						'SessionToken' => $sessionToken,
						'RememberMeToken' => $rememberMeToken,
						'EmailVerified' => $this->emailVerified,
						'DisplayName' => $this->displayNameFromDB,
						'LastLoginTime' => $lastLoginTime,
						'IsAdmin' => $this->isAdmin);
		
		return CreateResponse(1, $data);
	}
	
	public function ExecuteFailedLogInAttempt()
	{
		if ($this->loginAttempts > 4 and $this->lastLoginAttempt >= 240)
		{
			// Reset failed login attempts if the timer lapsed.
			$this->db->Prepare('UPDATE Users SET FailedLoginAttempts=1, LastLoginAttempt=NOW() where UserID=:UserID');
			$params = array(':UserID' => $this->userID);
			$this->db->Execute($params);
		}
		else
		{
			// Report failed login on User row
			$this->db->Prepare('UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts+1, LastLoginAttempt=NOW() WHERE UserID=:UserID');
			$params = array(':UserID' => $this->userID);
			$this->db->Execute($params);
		}
	}
}

?>