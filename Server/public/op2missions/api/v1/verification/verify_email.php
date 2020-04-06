<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// verify_email.php
// Verifies a user's email token.
// -2 = Bad Data
// -1 = Exception
//  1 = Success
//	2 = Invalid Token
//	3 = Already Registered
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';


$userName	= $_GET['u']		?? null;
$token		= $_GET['token']	?? null;
$redirect	= $_GET['redirect']	?? false;

// Fail out, the data we need was not passed
if (!isset($userName))		{ echo CreateErrorResponse(-2, 'Missing Parameter: u');		exit; }
if (!isset($token))			{ echo CreateErrorResponse(-2, 'Missing Parameter: token');	exit; }

// Connect to database
$db = new AppDatabase();
$db->Connect();

$redirectQuery = '?u='.rawurlencode($userName).'&code=';

// Get redirect url
/*if ($redirect)
{
	// TODO: Get redirect URL
	//$redirectUrl;

	// Exception handler needs to send to redirect page
	$lamda = function($exception) use($redirectUrl)
	{
		error_log($exception);
		
		header("Location: $redirectUrl".$redirectQuery.'-1');
	}
	
	set_exception_handler($lamda);
}*/

// Get the EmailToken from the table
$db->Prepare('SELECT EmailToken, EmailVerified FROM Users WHERE UserName=:UserName');
$params = array(':UserName' => $userName);

$db->Execute($params);

$row = $db->FetchArray();
if (!$row)
{
	// User not found
	if ($redirect)
		header("Location: $redirectUrl".$redirectQuery.'2');
	else
		echo CreateErrorResponse(2, 'We\'re sorry, that token is invalid or has expired.');
	
	exit;
}

$userEmailToken = $row[0];
$emailVerified = $row[1];

// Check for bad token
if (!isset($userEmailToken) OR !password_verify($token, $userEmailToken))
{
	if ($redirect)
		header("Location: $redirectUrl".$redirectQuery.'2');
	else
		echo CreateErrorResponse(2, 'We\'re sorry, that token is invalid or has expired.');
	
	exit;
}

if ($emailVerified != 0)
{
	if ($redirect)
		header("Location: $redirectUrl".$redirectQuery.'3');
	else
		echo CreateErrorResponse(3, 'Account already registered!');
	
	exit;
}	

// Set email as verified
$db->Prepare('UPDATE Users SET EmailVerified=1 WHERE UserName=:UserName');
$params = array(':UserName' => $userName);

$db->Execute($params);

if ($redirect)
	header("Location: $redirectUrl".$redirectQuery.'1');
else
	echo CreateSuccessResponse(1, $userName . '\'s account registered successfully!');

?>