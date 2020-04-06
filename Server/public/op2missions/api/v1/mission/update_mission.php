<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// update_mission.php
// Updates a mission on the database.
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
$missionName			= $_POST['MissionName']			?? null;
$missionDescription		= $_POST['MissionDescription']	?? null;
$gitHubLink				= $_POST['GitHubLink']			?? null;


// Fail out, the data we need was not passed
if (!isset($userID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');				exit; }
if (!isset($sessionToken))			{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');			exit; }
if (!isset($missionID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionID');				exit; }
if (!isset($missionName))			{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionName');			exit; }
if (!isset($missionDescription))	{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionDescription');	exit; }
if (!isset($gitHubLink))			{ echo CreateErrorResponse(-2, 'Missing Parameter: GitHubLink');			exit; }

$db = new AppDatabase();
$db->Connect();

// Check session
$session = new UserSession($db, $userID, $sessionToken);
if (!$session->IsValid())			{ echo CreateErrorResponse(-3, 'Invalid Session');							exit; }

// Check ban status
$user = new User($db, $userID);
if ($user->IsBanned())				{ echo CreateErrorResponse(-3, 'Invalid Session');							exit; }
$isAdmin = $user->IsAdmin();

if (!$isAdmin)
{
	// Verify that user is the mission author
	$db->Prepare('SELECT AuthorID FROM Missions WHERE MissionID=:MissionID');
	$params = array(':MissionID' => $missionID);

	$db->Execute($params);
		
	$row = $db->FetchArray();

	if ($row == false)					{ echo CreateErrorResponse(-3, 'Invalid Session');							exit; }
	if ($row[0] != $userID)				{ echo CreateErrorResponse(-3, 'Invalid Session');							exit; }
}

// Update mission 
$db->Prepare('UPDATE Missions SET '
			.' MissionName=:MissionName, MissionDescription=:MissionDescription, GitHubLink=:GitHubLink'
			.' WHERE MissionID=:MissionID');
$params = array(':MissionName' => $missionName,
				':MissionDescription' => $missionDescription,
				':GitHubLink' => $gitHubLink,
				':MissionID' => $missionID);

$db->Execute($params);


echo CreateSuccessResponse(1);

?>