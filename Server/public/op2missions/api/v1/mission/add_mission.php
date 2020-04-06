<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// add_mission.php
// Adds a mission to the database.
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
$missionName			= $_POST['MissionName']			?? null;
$missionDescription		= $_POST['MissionDescription']	?? null;
$gitHubLink				= $_POST['GitHubLink']			?? null;


// Fail out, the data we need was not passed
if (!isset($userID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');				exit; }
if (!isset($sessionToken))			{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');			exit; }
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

// Limit missions to 10 per day (global)
$db->Prepare('SELECT COUNT(1) FROM Missions WHERE DATE(CreatedDate) = CURDATE()');
$db->Execute();

$row = $db->FetchArray();

if ($row[0] > 10)
{
	echo CreateErrorResponse(-1, 'Global daily mission count exceeded.');
	exit;
}

// Insert mission 
$db->Prepare('INSERT INTO Missions'
			.' (MissionName, MissionDescription, GitHubLink, AuthorID) VALUES '
			.' (:MissionName, :MissionDescription, :GitHubLink, :AuthorID)');
$params = array(':MissionName' => $missionName,
				':MissionDescription' => $missionDescription,
				':GitHubLink' => $gitHubLink,
				':AuthorID' => $userID);

$db->Execute($params);


echo CreateSuccessResponse(1);

?>