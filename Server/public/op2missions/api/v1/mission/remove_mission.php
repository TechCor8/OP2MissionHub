<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// remove_mission.php
// Removes a mission from the database.
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


// Fail out, the data we need was not passed
if (!isset($userID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');				exit; }
if (!isset($sessionToken))			{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');			exit; }
if (!isset($missionID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionID');				exit; }

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

	if ($row == false)					{ echo CreateErrorResponse(-3, 'You are not the author if this mission.');	exit; }
	if ($row[0] != $userID)				{ echo CreateErrorResponse(-3, 'You are not the author if this mission.');	exit; }
}

// Verify mission ID (for security)
$db->Prepare('SELECT MissionID FROM Missions WHERE MissionID=:MissionID');
$params = array(':MissionID' => $missionID);

$db->Execute($params);
	
$row = $db->FetchArray();

if ($row == false)					{ echo CreateErrorResponse(-2, 'Could not find mission.');	exit; }

// Use the value from the database
$missionID = $row[0];

$dirPath = "../../../download/$missionID";

// Delete mission files
$db->Prepare('SELECT FileName FROM MissionFiles WHERE MissionID=:MissionID');
$params = array(':MissionID' => $missionID);

$db->Execute($params);
	
while ($row = $db->FetchArray())
	unlink($dirPath . '/' . $row[0]);

// Delete mission directory
if (file_exists($dirPath))
{
	$dirFiles = scandir($dirPath);
	if ($dirFiles !== false)
	{
		$dirFiles = array_diff($dirFiles, array('.', '..'));
		if (count($dirFiles) == 0)
			rmdir($dirPath);
	}
}

// Remove mission 
$db->Prepare('DELETE FROM Missions WHERE MissionID=:MissionID');
$params = array(':MissionID' => $missionID);

$db->Execute($params);


echo CreateSuccessResponse(1);

?>