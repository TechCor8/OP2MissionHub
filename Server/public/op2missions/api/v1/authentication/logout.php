<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// logout.php
// Terminates a user's session
// -3 = Invalid Session
// -2 = Bad Data
// -1 = Failure
//  1 = Success
//  2 = Rejected
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/user/session.php';


$userID			= $_POST['UserID']			?? null;
$sessionToken	= $_POST['SessionToken']	?? null;


// Fail out, the data we need was not passed
if (!isset($userID))			{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');		exit; }
if (!isset($sessionToken))		{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');	exit; }

$db = new AppDatabase();
$db->Connect();

// Check session
$session = new UserSession($db, $userID, $sessionToken);
if (!$session->IsValid())		{ echo CreateErrorResponse(-3, 'Invalid Session');					exit; }

// Expire session
$db->Prepare('UPDATE Users SET SessionToken=NULL, LogOutTime=NOW() WHERE UserID=:UserID');
$params = array(':UserID' => $userID);
				
$db->Execute($params);

echo CreateSuccessResponse(1);

?>