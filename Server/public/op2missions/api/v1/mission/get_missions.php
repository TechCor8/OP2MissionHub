<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// get_service.php
// Adds user feedback to database.
// -4 = Application Not Found
// -3 = Invalid Session
// -2 = Bad Data
// -1 = Failure
//  1 = Success
////////////////////////////////////////////////////////////////////////////////////////////////////
require '../config.php';
require INCLUDE_PATH_PUBLIC . '/library/common/response_format.php';
require INCLUDE_PATH_PUBLIC . '/library/common/exception_handler.php';
require INCLUDE_PATH_PUBLIC . '/library/common/dbconnect.php';


$db = new AppDatabase();
$db->Connect();


// Get Missions
$db->Prepare('SELECT M.MissionID, MissionName, MissionDescription, GitHubLink, AuthorID, U.DisplayName AuthorName, U2.DisplayName CertifyingAdminName, Version, (SELECT GROUP_CONCAT(FileName) FROM MissionFiles MF WHERE MF.MissionID = M.MissionID) AS FileNames'
			.' FROM Missions M'
			.' INNER JOIN Users U ON M.AuthorID = U.UserID'
			.' LEFT JOIN Users U2 ON M.CertifyingAdminID = U2.UserID');
			
$db->Execute();

$missions = array();

while ($row = $db->FetchDictionary())
{
	$row['FileNames'] = explode(',', $row['FileNames']);
	array_push($missions, $row);
}

$result = array('Missions' => $missions);

echo CreateResponse(1, $result);

?>