<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// remove_file.php
// Removes a mission file from the server.
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
$fileName				= $_POST['FileName']			?? null;


// Fail out, the data we need was not passed
if (!isset($userID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');				exit; }
if (!isset($sessionToken))			{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');			exit; }
if (!isset($missionID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionID');				exit; }
if (!isset($fileName))				{ echo CreateErrorResponse(-2, 'Missing Parameter: FileName');				exit; }

// Validate file name
if(preg_match('/[^a-z_\-0-9.]/i', $fileName))
{
	echo CreateErrorResponse(-2, 'Invalid FileName: ' . $fileName);
	exit;
}

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

// Verify mission ID and file name (for security)
$db->Prepare('SELECT MissionID, FileName FROM MissionFiles WHERE MissionID=:MissionID AND FileName=:FileName');
$params = array(':MissionID' => $missionID,
				':FileName' => $fileName);

$db->Execute($params);
	
$row = $db->FetchArray();

if ($row == false)					{ echo CreateErrorResponse(-2, 'Could not find mission file.');	exit; }

// Use the values from the database
$missionID = $row[0];
$fileName = $row[1];

// Remove file from web path
$webPath = "../../../download/$missionID/$fileName";
if (file_exists($webPath))
	unlink($webPath);

// Remove mission file
$db->Prepare('DELETE FROM MissionFiles WHERE MissionID=:MissionID AND FileName=:FileName');
$params = array(':MissionID' => $missionID,
				':FileName' => $fileName);

$db->Execute($params);


// Increment mission version
$db->Prepare('UPDATE Missions SET Version=Version+1 WHERE MissionID=:MissionID');
$params = array(':MissionID' => $missionID);

$db->Execute($params);


echo CreateSuccessResponse(1);

?>