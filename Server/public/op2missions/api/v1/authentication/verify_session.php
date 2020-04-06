<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// verify_session.php
// Verifies a user's session and userID.
// -3 = Invalid Session
// -2 = Bad Data
// -1 = Exception
//  1 = Success
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/user/session.php';


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


echo CreateSuccessResponse(1);

?>