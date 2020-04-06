<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// resend_email.php
// Resends the confirmation email to a user.
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/user/session.php';
require INCLUDE_PATH_PUBLIC . '/library/verification/verification_email.php';
require INCLUDE_PATH_PUBLIC . '/library/common/gen_uuid.php';


$userID				= $_POST['UserID']			?? null;
$sessionToken		= $_POST['SessionToken']	?? null;


// Fail out, the data we need was not passed
if (!isset($userID))		{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');		exit; }
if (!isset($sessionToken))	{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');	exit; }

// Connect to database
$db = new AppDatabase();
$db->Connect();

// Check session
$session = new UserSession($db, $userID, $sessionToken);
if (!$session->IsValid())	{ echo CreateErrorResponse(-3, 'Invalid Session');					exit; }


// Get info about the user and a new email token
$db->Prepare('SELECT UserName, Email, EmailVerified FROM Users WHERE UserID=:UserID');

$params = array(':UserID' => $userID);

$db->Execute($params);

$row = $db->FetchArray();
if (!$row)
{
	echo CreateErrorResponse(-1, 'User not found.');
	exit;
}

$userName = $row[0];
$email = $row[1];
$emailVerified = $row[2];
$emailToken = generate_password();


// Has the email already been verified?
if ($emailVerified != 0)
{
	echo CreateSuccessResponse(2, 'User already verified.');
	exit;
}

$emailTokenHash = password_hash($emailToken, PASSWORD_DEFAULT);

// Update email token on the db
$db->Prepare('UPDATE Users SET EmailToken = :EmailToken WHERE UserID = :UserID');
$params = array(':EmailToken' => $emailTokenHash,
				':UserID' => $userID);

$db->Execute($params);

// Send the email
$sendMail = new VerificationEmail($db);
$sendMail->Send($email, $userName, $emailToken);


echo CreateSuccessResponse(1);

?>