<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// reset_password.php
// Changes a user's password.
// -2 = Bad Data
// -1 = Exception
//  1 = Success
//  2 = Invalid Token
//  3 = Password requires 8 different characters
//  4 = Password must contain at least 1 upper case, number, or symbol and at least 1 lower case.
//  5 = Password too similar to name
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/authentication/account_creator.php';


$userName			= $_GET['u']		?? $_POST['u']		?? null;
$newPassword		= $_GET['p']		?? $_POST['p']		?? null;
$token				= $_GET['token']	?? $_POST['token']	?? null;
$checkTokenOnly		= $_GET['check']	?? $_POST['check']	?? false;

// Fail out, the data we need was not passed
if (!isset($userName))		{ echo CreateErrorResponse(-2, 'Missing Parameter: u');		exit; }
if (!isset($token))			{ echo CreateErrorResponse(-2, 'Missing Parameter: token');	exit; }

if (!$checkTokenOnly AND !isset($newPassword))	{ echo CreateErrorResponse(-2, 'Missing Parameter: p');		exit; }

// Connect to database
$db = new AppDatabase();
$db->Connect();


// Get the ResetPasswordToken from the table
$db->Prepare('SELECT ResetPasswordToken, UNIX_TIMESTAMP() - UNIX_TIMESTAMP(ResetPasswordTime), DisplayName FROM Users WHERE UserName=:UserName');
$params = array(':UserName' => $userName);

$db->Execute($params);

$row = $db->FetchArray();
if (!$row)
{
	// User not found
	echo CreateErrorResponse(2, 'We\'re sorry, that token is invalid or has expired.');
	exit;
}

$resetPasswordToken = $row[0];
$timeSinceRequestInSec = $row[1];
$displayName = $row[2];


if (!isset($timeSinceRequestInSec) OR $timeSinceRequestInSec >= 86400) // Expires in 24 hours
{
	// Reset expired
	echo CreateErrorResponse(2, 'We\'re sorry, that token is invalid or has expired.');
	exit;
}

if (!isset($resetPasswordToken) OR !password_verify($token, $resetPasswordToken))
{
	// Bad token
	echo CreateErrorResponse(2, 'We\'re sorry, that token is invalid or has expired.');
	exit;
}

// Don't set password if this only a token check
if ($checkTokenOnly)
{
	echo CreateSuccessResponse(1, 'Token is valid');
	exit;
}

$accountCreator = new AccountCreator($db);

// 0 = Valid
// 1 = Needs different characters
// 2 = Needs Upper Number Symbol And Lower
// 3 = Password too similar to text
switch ($code = $accountCreator->IsInvalidPassword($newPassword, array($userName, $displayName)))
{
	case 1:		$error = 'Password must contain at least 8 different characters.';	break;
	case 2:		$error = 'Password must contain at least 1 upper case letter, number or symbol and 1 lower case letter.'; break;
	case 3:		$error = 'Password too similar to name.';	break;
}

if (isset($error))
{
	// User not found
	echo CreateErrorResponse($code+2, $error);
	exit;
}


$newPasswordHash = password_hash($newPassword, PASSWORD_DEFAULT);

// Set new password
$db->Prepare('UPDATE Users SET Password=:Password, ResetPasswordToken=NULL, ResetPasswordTime=NULL, EmailVerified=1 WHERE UserName=:UserName');
$params = array(':Password' => $newPasswordHash,
				':UserName' => $userName);

$db->Execute($params);

echo CreateSuccessResponse(1, $userName . '\'s password has been reset successfully.');



?>