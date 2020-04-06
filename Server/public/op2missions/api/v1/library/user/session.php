<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// user_session.php
// Verifies user sessions.
////////////////////////////////////////////////////////////////////////////////////////////////////

class UserSession
{
	private $db = null;
	private $userID = null;
	private $sessionToken = null;
	private $isValid = false;
	
	public function IsValid()			{ return $this->isValid;		}
	
	
	public function __construct($db, $userID, $sessionToken)
	{
		$this->db = $db;
		$this->userID = $userID;
		$this->sessionToken = $sessionToken;
		
		$this->isValid = $this->Validate();
	}
	
	private function Validate()
	{
		if (!isset($this->sessionToken))
			return false;
		
		// Validate session ID
		$this->db->Prepare('SELECT SessionToken, UNIX_TIMESTAMP() - UNIX_TIMESTAMP(SessionLastUpdated) FROM Users WHERE UserID=:UserID');
		$params = array(':UserID' => $this->userID);
		
		$this->db->Execute($params);

		$row = $this->db->FetchArray();
		if (!$row)
			return false;
		
		$rowSessionToken = $row[0];
		$sessionLastUpdated = $row[1];
		
		
		// 86400 is 24 hours.
		if ($sessionLastUpdated < 86400 and $rowSessionToken === $this->sessionToken)
		{
			// Update session
			$this->db->Prepare('UPDATE Users SET SessionLastUpdated=NOW() WHERE UserID=:UserID');
			$params = array(':UserID' => $this->userID);
			
			$this->db->Execute($params);

			return true;
		}
		
		return false;
	}
}

?>