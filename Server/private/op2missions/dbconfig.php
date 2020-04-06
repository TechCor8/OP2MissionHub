<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// dbconfig.php
// This file stores the db configuration and connects to the database.
////////////////////////////////////////////////////////////////////////////////////////////////////

// Makes a backup of the database
function BackupDB()
{
	$dbhost='mysqlhost.com';
	$dbusername='user';
	$dbpassword='pass';
	$database='op2missions';
	
	$curDate = date('Y-m-d_H-i-s');
	
	$destDir = dirname(__FILE__) . '/backup';
	
	shell_exec("mysqldump --opt --single-transaction --user=$dbusername --password=$dbpassword --host=$dbhost $database | gzip > $destDir/dump-$curDate.sql.gz");
	
	return $output;
}

function ConnectDB()
{
	// DB configuration
	$dbhost='mysqlhost.com';
	$dbusername='user';
	$dbpassword='pass';
	$database='op2missions';
	
	try
	{
		$pdo = new PDO("mysql:host=$dbhost;dbname=$database", $dbusername, $dbpassword);
		
		$pdo->setAttribute(PDO::ATTR_EMULATE_PREPARES, false);
		$pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
		
		return $pdo;
	}
	catch (PDOException $ex)
	{
		return false;
	}
}

?>