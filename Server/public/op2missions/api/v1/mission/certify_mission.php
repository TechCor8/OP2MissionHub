<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// certify_mission.php
// Marks a mission as certified or uncertified on the database.
// -3 = Invalid Session
// -2 = Bad Data
// -1 = Failure
//  1 = Success
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';
require INCLUDE_PATH_PUBLIC . '/library/user/session.php';
require INCLUDE_PATH_PUBLIC . '/library/user/user.php';


$userID					= $_POST['UserID']				?? null;
$sessionToken			= $_POST['SessionToken']		?? null;
$missionID				= $_POST['MissionID']			?? null;
$certify				= $_POST['Certify']				?? null;


// Fail out, the data we need was not passed
if (!isset($userID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');				exit; }
if (!isset($sessionToken))			{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');			exit; }
if (!isset($missionID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionID');				exit; }
if (!isset($certify))				{ echo CreateErrorResponse(-2, 'Missing Parameter: Certify');				exit; }

$db = new AppDatabase();
$db->Connect();

// Check session
$session = new UserSession($db, $userID, $sessionToken);
if (!$session->IsValid())			{ echo CreateErrorResponse(-3, 'Invalid Session');							exit; }

// Check ban status
$user = new User($db, $userID);
if ($user->IsBanned())				{ echo CreateErrorResponse(-3, 'Invalid Session');							exit; }
if (!$user->IsAdmin())				{ echo CreateErrorResponse(-3, 'Invalid Session');							exit; }

// Update mission certification
if ($certify)
{
	// Certify
	$db->Prepare('UPDATE Missions SET '
				.' CertifyingAdminID=:CertifyingAdminID'
				.' WHERE MissionID=:MissionID');
	$params = array(':CertifyingAdminID' => $userID,
					':MissionID' => $missionID);
}
else
{
	// Uncertify
	$db->Prepare('UPDATE Missions SET '
				.' CertifyingAdminID=NULL'
				.' WHERE MissionID=:MissionID');
	$params = array(':MissionID' => $missionID);
}

$db->Execute($params);


echo CreateSuccessResponse(1);

?>