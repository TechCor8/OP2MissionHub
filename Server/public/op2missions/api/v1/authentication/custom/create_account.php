<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// create_account.php
// Creates a user in the database.
// -2 | Bad Data
// -1 | Internal Error
// 1 | Valid
// 2 | Email Bad Format
// 3 | Email Already Taken
// 4 | UserName Bad Format
// 5 | UserName Already Taken
// 6 | DisplayName Bad Format
// 7 | DisplayName Already Taken
// 8 | Password needs different characters
// 9 | Password needs Upper Number Symbol And Lower
// 10 | Password too similar to name
// 11 | Bad Version | $foundVersion
// 12 | Down for maintenance
// 13 | Account limit reached
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/constants/app_constants.php';
require INCLUDE_PATH_PUBLIC . '/library/authentication/account_creator.php';
require INCLUDE_PATH_PUBLIC . '/library/authentication/auth_user.php';


$email					= $_POST['Email']				?? null;
$userName				= $_POST['UserName']			?? null;
$displayName			= $_POST['DisplayName']			?? null;
$password				= $_POST['Password']			?? null;
$appVersion				= $_POST['AppVersion']			?? null;

// Fail out, the data we need was not passed
if (!isset($email))				{ echo CreateErrorResponse(-2, 'Missing Parameter: Email'); 		exit; }
if (!isset($userName))			{ echo CreateErrorResponse(-2, 'Missing Parameter: UserName');		exit; }
if (!isset($displayName))		{ echo CreateErrorResponse(-2, 'Missing Parameter: DisplayName'); 	exit; }
if (!isset($password))			{ echo CreateErrorResponse(-2, 'Missing Parameter: Password');		exit; }

$db = new AppDatabase();
$db->Connect();

$authUser = new AuthUser($db);

// Get version info
$authUser->FetchAppData();

// If game is not allowing logins, prevent account creation and return message
if ($authUser->IsInMaintenanceMode())
{
	echo $authUser->GetMaintenanceResponse(12);
	exit;
}

// Check user version
if (!$authUser->HasValidVersion($appVersion))
{
	echo $authUser->GetBadVersionResponse(11);
	exit;
}

// Check IP Address rate limit (6 hour window)
$db->Prepare('SELECT COUNT(1) FROM Users WHERE CreatedIPAddress=:CreatedIPAddress AND CreationDate > DATE_SUB(NOW(), INTERVAL 6 HOUR)');
$params = array(':CreatedIPAddress' => $_SERVER['REMOTE_ADDR']);

$db->Execute($params);

$row = $db->FetchArray();
$count = $row[0];

// Don't bother checking the rate limit if there aren't any previous accounts by this IP address
if ($count > 0)
{
	// Get the IP account limit
	$constants = new AppConstants($db);
	$accountLimit = $db->GetConstant(AppConstants::CREATE_ACCOUNT_IP_LIMIT);
	
	if ($count >= $accountLimit)
	{
		echo CreateErrorResponse(13, 'Account limit reached.');
		exit;
	}
}

// 1 = Valid
// 2 = Email Bad Format
// 3 = Email Already Taken
// 4 = UserName Bad Format
// 5 = UserName Already Taken
// 6 = DisplayName Bad Format
// 7 = DisplayName Already Taken
// 8 = Password needs different characters
// 9 = Password needs Upper Number Symbol And Lower
// 10 = Password too similar to name
// -1 = Internal Error
$accountCreator = new AccountCreator($db);
$result = $accountCreator->CreateUser(1, $userName, $displayName, $password, $email);

if ($result === 1)
	echo CreateSuccessResponse($result);
else
	echo CreateErrorResponse($result, $accountCreator->GetErrorMessage());


?>