<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// request_password_reset.php
// Sends a "reset password" email.
// -2 = Bad data
// -1 = Exception
//  1 = Success
//  2 = Invalid email
//  3 = Reset Limit | $timeSinceLastEmail
//  4 = Could not send email
// (1) = Email not registered
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/constants/app_constants.php';
require INCLUDE_PATH_PUBLIC . '/library/reset/reset_password_email.php';
require INCLUDE_PATH_PUBLIC . '/library/common/gen_uuid.php';


$email					= $_POST['Email']				?? null;

// Fail out, the data we need was not passed
if (!isset($email))				{ echo CreateErrorResponse(-2, 'Missing Parameter: Email');				exit; }

// Validate email
if (filter_var($email, FILTER_VALIDATE_EMAIL) == false)
{
	echo CreateErrorResponse(2, 'Invalid email address');
	exit;
}

$db = new AppDatabase();
$db->Connect();

// Verify email exists on our db and get username and a password token
$db->Prepare('SELECT UserID, UserName, UNIX_TIMESTAMP() - UNIX_TIMESTAMP(ResetPasswordTime) FROM Users WHERE Email=:Email');
$params = array(':Email' => $email);

$db->Execute($params);

$row = $db->FetchArray();
if (!$row)
{
	// Did not find account with that email. Tell them we succeeded anyway.
	echo CreateSuccessResponse(1);
	exit;
}

$userID = $row[0];
$userName = $row[1];
$timeSinceLastEmail = $row[2];

// Get the IP account limit
$constants = new AppConstants($db);
$passwordTimeDelay = $constants->GetConstant(AppConstants::FORGOT_PASSWORD_TIME_DELAY);

if (isset($timeSinceLastEmail) AND $timeSinceLastEmail < $passwordTimeDelay)
{
	$timeUntilNextEmail = $passwordTimeDelay - $timeSinceLastEmail;
	
	// Inside cooldown period between reset emails. Tell them we succeeded anyway.
	echo CreateSuccessResponse(1);
	//echo CreateResponse(3, array('timeUntilNextEmail' => $timeUntilNextEmail), 'Reset limit');
	exit;
}


$passwordToken = generate_password(38);
$passwordTokenHash = password_hash($passwordToken, PASSWORD_DEFAULT);

// Update reset password token on the db
$db->Prepare('UPDATE Users SET ResetPasswordToken = :ResetPasswordToken, ResetPasswordTime=NOW() where UserID = :UserID');
$params = array(':ResetPasswordToken' => $passwordTokenHash,
				':UserID' => $userID);

$db->Execute($params);


// Send the email
$sendMail = new ResetPasswordEmail($db);
$sendMail->Send($email, $userName, $passwordToken);

echo CreateSuccessResponse(1);

?>