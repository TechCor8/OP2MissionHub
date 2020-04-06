<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// authenticate.php
// Authenticates a user and returns his UserID
// -2 = Bad data
// 1 = Success | $userID | $sessionToken | $rememberMeToken | $emailVerified | $displayNameFromDB
// 2 = Bad app version | $appVersionFromDB
// 3 = Incorrect username or password
// 4 = Account locked
// 5 = User banned | $banDate | $banRestoreDate | $banReason
// 6 = Down for maintenance
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/authentication/auth_user.php';

// Remove comment to disable login for maintenance.
//echo CreateResponse(6, array('message' => 'Down for Maintenance', 'displayMessage' => 'We are currently down for maintenance.\n\nWe will be back as soon as possible.')); exit;

$userName				= $_POST['UserName']			?? null;
$password				= $_POST['Password']			?? null;
$appVersion				= $_POST['AppVersion']			?? null;
$rememberMe				= $_POST['RememberMe']			?? null;
$rememberMeTokenPosted	= $_POST['RememberMeToken']		?? null;
$adminOverride			= $_POST['AdminOverride']		?? null;

if (isset($rememberMe) == false) $rememberMe = 0;

// Fail out, the data we need was not passed
if (!isset($userName))			{ echo CreateErrorResponse(-2, 'Missing Parameter: UserName');		exit; }
if (!isset($password))			{ echo CreateErrorResponse(-2, 'Missing Parameter: Password');		exit; }


$db = new AppDatabase();
$db->Connect();

$authUser = new AuthUser($db);

// Get version info
$authUser->FetchAppData();

// If app is not allowing logins, prevent login and return message
if ($authUser->IsInMaintenanceMode() AND !$authUser->IsAllowingAdminAccess())
{
	echo $authUser->GetMaintenanceResponse();
	exit;
}

// Check user version
if (!$authUser->HasValidVersion($appVersion))
{
	echo $authUser->GetBadVersionResponse();
	exit;
}

// Get the user's data
$authUser->FetchUserData(1, $userName);

if (empty($authUser->GetPasswordHash()))
{
	echo CreateErrorResponse(3, 'Incorrect username or password');
	exit;
}

if ($authUser->IsUserBanned())
{
	echo $authUser->GetBannedResponse();
	exit;
}

if ($authUser->IsAccountLocked())
{
	echo $authUser->GetLockedResponse();
	exit;
}

// Check if password is correct
if (isset($rememberMeTokenPosted) AND password_verify($rememberMeTokenPosted, $authUser->GetRememberMeTokenHash())
			or password_verify($password, $authUser->GetPasswordHash()))
{
	// Finish login process
	echo $authUser->ExecuteSuccessfulLogin($rememberMe, $adminOverride);
}
else
{
	// Login failed
	$authUser->ExecuteFailedLogInAttempt();
	
	echo CreateErrorResponse(3, 'Incorrect username or password');
}



?>