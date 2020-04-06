<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// app_constants.php
// Contains application constants.
////////////////////////////////////////////////////////////////////////////////////////////////////


class AppConstants
{
	private $db = null;
	private $appID = null;
	private $error = null;
	
	// Constant IDs
	public const APP_VERSION							= 1;
	public const MAINTENANCE_MODE						= 2;
	public const MAINTENANCE_MODE_LOGIN_ACCESS			= 3;
	public const MAX_DAILY_ACTIVE_USERS					= 4;
	public const DAYS_UNTIL_USER_DELETED				= 5;
	public const CREATE_ACCOUNT_IP_LIMIT				= 6;
	public const FORGOT_PASSWORD_TIME_DELAY				= 7;
	public const ASSET_DOWNLOAD_URL						= 8;
	public const APP_NAME								= 9;
	public const APP_URL								= 10;
	public const PUBLISHER_NAME							= 11;
	public const PUBLISHER_URL							= 12;
	public const PUBLISHER_EMAIL						= 13;
	
	
	public function __construct($db)
	{
		$this->db = $db;
	}
	
	public function GetErrorMessage()
	{
		return $this->error;
	}
	
	
	// Returns a single constant's value.
	public function GetConstant($constantID)
	{
		$this->db->Prepare('SELECT ConstantValue FROM AppConstants WHERE AppConstantID=:AppConstantID');

		$params = array(':AppConstantID' => $constantID);
					
		$this->db->Execute($params);
		
		$row = $this->db->FetchArray();
		if ($row === false)
			throw new Exception("AppConstantID: $constantID not found!");
		
		return $row[0];
	}
	
	// Returns multiple constants in an associative array
	public function GetConstants($arrConstantIDs)
	{
		$len = count($arrConstantIDs);
		if ($len == 0)
			return array();
		
		// Generate parameters
		$params = array();
		
		for ($i=0; $i < $len; ++$i)
			$params[":AppConstantID$i"] = $arrConstantIDs[$i];
		
		$whereClause = implode(',', array_keys($params));
		
		// Perform query
		$this->db->Prepare("SELECT AppConstantID, ConstantValue FROM AppConstants WHERE AppConstantID IN ($whereClause)");

		$this->db->Execute($params);
		
		// Compile result
		$result = array();
		
		while ($row = $this->db->FetchArray())
			$result[$row[0]] = $row[1];
		
		// Check for missing constants
		if (count($result) !== $len)
		{
			foreach ($arrConstantIDs as $id)
			{
				if (!array_key_exists($id, $result))
					throw new Exception("AppConstantID: $id not found!");
			}
		}
		
		return $result;
	}
	
	// Updates a constant's value
	public function SetConstant($constantID, $value, $overrideLock)
	{
		$lockQuery = '';
		if (!$overrideLock)
			$lockQuery = ' AND AdminLock=0';
		
		$this->db->Prepare('UPDATE AppConstants SET ConstantValue=:Value WHERE AppConstantID=:AppConstantID'.$lockQuery);

		$params = array(':AppConstantID' => $AppConstantID,
						':Value' => $value);
					
		$this->db->Execute($params);
		
		return true;
	}
	
	// Creates default constants (for new applications)
	public function InsertDefaultConstants()
	{
		$arrConstants = array(								// Value	// AdminLock
		self::APP_VERSION							=> array('1.0',		1),
		self::MAINTENANCE_MODE						=> array('1',		1),
		self::MAINTENANCE_MODE_LOGIN_ACCESS			=> array('1',		1),
		self::MAX_DAILY_ACTIVE_USERS				=> array('5',		1),
		self::DAYS_UNTIL_USER_DELETED				=> array('365',		1),
		self::CREATE_ACCOUNT_IP_LIMIT				=> array('4',		1),
		self::FORGOT_PASSWORD_TIME_DELAY			=> array('7200',	1),
		self::ASSET_DOWNLOAD_URL					=> array('',		0),
		self::APP_NAME								=> array('Untitled',0),
		self::APP_URL								=> array('',		0),
		self::PUBLISHER_NAME						=> array('',		0),
		self::PUBLISHER_URL							=> array('',		0),
		self::PUBLISHER_EMAIL						=> array('',		0),
		);
		
		$this->db->Prepare('INSERT INTO AppConstants (AppConstantID, ConstantValue, AdminLock)'
							.' VALUES (:AppConstantID, :ConstantValue, :AdminLock)');
		
		foreach ($arrConstants as $key => $value)
		{
			$params = array(':AppConstantID' => $key,
							':Value' => $value[0],
							':AdminLock' => $value[1]);
						
			$this->db->Execute($params);
		}
		
		return true;
	}
}




?>