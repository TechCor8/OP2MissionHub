<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// user.php
// Contains user functions.
////////////////////////////////////////////////////////////////////////////////////////////////////

class User
{
	private $db = null;
	private $userID = null;
	
	
	public function __construct($db, $userID)
	{
		$this->db = $db;
		$this->userID = $userID;
	}
	
	public function IsAdmin()
	{
		$this->db->Prepare('SELECT IsAdmin FROM Users WHERE UserID=:UserID');
		$params = array(':UserID' => $this->userID);
		
		$this->db->Execute($params);
			
		$row = $this->db->FetchArray();
		
		if ($row == false)
			return false;
		
		return $row[0];
	}
	
	// Returns the BanTypeID or FALSE if the user is not banned.
	public function IsBanned()
	{
		$this->db->Prepare('SELECT BanTypeID FROM Users WHERE UserID=:UserID AND BanStartTime IS NOT NULL AND BanStartTime < NOW() AND (BanEndTime IS NULL OR NOW() < BanEndTime)');
		$params = array(':UserID' => $this->userID);
		
		$this->db->Execute($params);
			
		$row = $this->db->FetchArray();
		
		if ($row == false)
			return false;
		
		return $row[0];
	}
	
	public function BanUser($banTypeID, $endTime, $banReason, $appendBanReason)
	{
		// Check ban status
		$this->db->Prepare('SELECT BanTypeID, BanEndTime, BanReason FROM Users WHERE UserID=:UserID');
		$params = array(':UserID' => $this->userID);
		
		$this->db->Execute($params);
		
		$row = $this->db->FetchArray();
		
		if ($row == false)
		{
			//$this->error = 'User does not exist.';
			return false;
		}
		
		$oldBanType = $row[0];
		$oldBanEndTime = $row[1];
		$oldBanReason = $row[2];
		
		// Make sure new ban is not less than old ban, and that old ban is still in effect.
		if (isset($oldBanType) AND $banTypeID < $oldBanType AND (!isset($oldBanEndTime) OR new DateTime($oldBanEndTime) > new DateTime()))
		{
			// Cannot issue a lesser ban.
			//$this->error = 'Cannot issue a weaker ban. Unban this user first.';
			return false;
		}
		
		// Execute ban
		if ($appendBanReason)
			$banReason = $oldBanReason . $banReason;
		
		$this->db->Prepare('UPDATE Users SET BanTypeID=:BanTypeID, BanStartTime=NOW(), BanEndTime=:BanEndTime, BanCount=BanCount+1, BanReason=:BanReason'
					.' WHERE UserID=:UserID');

		$params = array(':BanTypeID' => $banTypeID,
						':BanEndTime' => $endTime,
						':BanReason' => $banReason,
						':UserID' => $this->userID);
					
		$this->db->Execute($params);
		
		return true;
	}
	
	public function UnbanUser($banReason, $appendBanReason)
	{
		$query = ':BanReason';
		if ($appendBanReason)
			$query = 'CONCAT(BanReason, :BanReason)';
		
		$this->db->Prepare('UPDATE Users SET BanTypeID=NULL, BanStartTime=NULL, BanEndTime=NULL, BanReason='.$query
					.' WHERE UserID=:UserID');

		$params = array(':BanReason' => $banReason,
						':UserID' => $this->userID);
					
		$this->db->Execute($params);
		
		return true;
	}
	
	public function KickUser()
	{
		$this->db->Prepare('UPDATE Users SET SessionID=NULL WHERE UserID=:UserID');

		$params = array(':UserID' => $this->userID);
					
		$this->db->Execute($params);
		
		return true;
	}
}

?>