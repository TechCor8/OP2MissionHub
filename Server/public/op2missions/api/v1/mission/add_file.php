<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// add_file.php
// Adds a mission file to the server.
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
$fileName				= $_FILES['MissionFile']['name'];
$fileSize				= $_FILES['MissionFile']['size'];


// Fail out, the data we need was not passed
if (!isset($userID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: UserID');				exit; }
if (!isset($sessionToken))			{ echo CreateErrorResponse(-2, 'Missing Parameter: SessionToken');			exit; }
if (!isset($missionID))				{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionID');				exit; }
if (!isset($fileName))				{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionFile');			exit; }
if (!isset($fileSize))				{ echo CreateErrorResponse(-2, 'Missing Parameter: MissionFile size');		exit; }
if ($fileSize > 5000000)			{ echo CreateErrorResponse(-2, 'File size exceeds limit.');					exit; }

// Validate file name
if(preg_match('/[^a-z_\-0-9.]/i', $fileName))
{
	echo CreateErrorResponse(-2, 'Invalid file name: ' . $fileName);
	exit;
}

// Don't allow 'parent directory' in the file name
if (strpos($fileName, '..') !== false)
{
	echo CreateErrorResponse(-2, 'Invalid file name: ' . $fileName);
	exit;
}

// Validate extension
$extension = strtolower(substr($fileName, -4));
if($extension !== '.dll' AND $extension !== '.opm' AND $extension !== '.txt' AND $extension !== '.map')
{
	echo CreateErrorResponse(-2, 'File extension must be dll, opm, txt, or map.');
	exit;
}

if (!isset($_FILES['MissionFile']['error']) OR $_FILES['MissionFile']['error'] != UPLOAD_ERR_OK)
{
	echo CreateErrorResponse(-2, 'Bad File Data');
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

	if ($row == false)				{ echo CreateErrorResponse(-3, 'You are not the author if this mission.');	exit; }
	if ($row[0] != $userID)			{ echo CreateErrorResponse(-3, 'You are not the author if this mission.');	exit; }
	
	// Limit mission file count
	$db->Prepare('SELECT COUNT(1) FROM MissionFiles WHERE MissionID=:MissionID');
	$params = array(':MissionID' => $missionID);

	$db->Execute($params);
		
	$row = $db->FetchArray();

	if ($row[0] >= 10)				{ echo CreateErrorResponse(-2, 'File limit exceeded.');						exit; }
}

$db->BeginTransaction();

// Verify that file name is unique
$db->Prepare('SELECT 1 FROM MissionFiles WHERE FileName=:FileName');
$params = array(':FileName' => $fileName);

$db->Execute($params);
	
$row = $db->FetchArray();

if ($row)							{ echo CreateErrorResponse(-2, 'Your file name must be unique.');	exit; }

// Add mission file
$db->Prepare('INSERT INTO MissionFiles (MissionID, FileName) VALUES '
			.' (:MissionID, :FileName)');
$params = array(':MissionID' => $missionID,
				':FileName' => $fileName);

$db->Execute($params);


// Increment mission version, Reset certification
$db->Prepare('UPDATE Missions SET Version=Version+1, CertifyingAdminID=NULL WHERE MissionID=:MissionID');
$params = array(':MissionID' => $missionID);

$db->Execute($params);


// Find file destination
$webPath = "../../../download/$missionID/$fileName";

// Create mission directory, if it does not exist
if (!is_dir("../../../download/$missionID"))
	mkdir("../../../download/$missionID");

// Move file to web path
$tmp_name = $_FILES['MissionFile']['tmp_name'] ?? null;
if (isset($tmp_name))
	move_uploaded_file($tmp_name, $webPath);
else
{
	$db->Rollback();
	echo CreateErrorResponse(-1, 'Failed to move file.');
	exit;
}

$db->Commit();


echo CreateSuccessResponse(1);

?>