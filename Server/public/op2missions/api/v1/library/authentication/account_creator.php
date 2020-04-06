<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// account_creator.php
// Contains functions for creating a user account.
////////////////////////////////////////////////////////////////////////////////////////////////////
require INCLUDE_PATH_PUBLIC . '/library/verification/verification_email.php';


class AccountCreator
{
	private $db = null;
	private $error = null;
	
	
	public function __construct($db)
	{
		$this->db = $db;
	}
	
	public function GetErrorMessage()		{ return $this->error;						}
	
	// 0 = Valid
	// 1 = Bad Format
	// 2 = Address already taken
	private function IsInvalidEmail($email)
	{
		if (filter_var($email, FILTER_VALIDATE_EMAIL) == false)
			return 1;
		
		// Search for the email in the Users table
		$this->db->Prepare('SELECT UserID FROM Users WHERE Email=:Email');
		$params = array(':Email' => $email);

		$this->db->Execute($params);
		
		$row = $this->db->FetchArray();
		if ($row)
			return 2;
		
		return 0;
	}

	// 0 = Valid
	// 1 = Bad Format
	// 2 = Name already taken or banned
	private function IsInvalidUserName($userName)
	{
		// Make sure name length is valid
		if (strlen($userName) < 8 OR strlen($userName) > 32)
			return 1;
		
		// Name must be alphanumeric
		if (!preg_match('/^[a-zA-Z0-9]+$/', $userName))
			return 1;
		
		// Search for the name in the banned list
		$this->db->Prepare('SELECT 1 FROM BannedNames WHERE BannedName=:UserName');
		$params = array(':UserName' => $userName);

		$this->db->Execute($params);

		if ($this->db->FetchArray())
			return 2;

		// Search for the user name in the table
		$this->db->Prepare('SELECT UserID FROM Users WHERE UserName=:UserName');
		$params = array(':UserName' => $userName);

		$this->db->Execute($params);

		if ($this->db->FetchArray())
			return 2;
		
		return 0;
	}

	// 0 = Valid
	// 1 = Bad Format
	// 2 = Name already taken or banned
	private function IsInvalidDisplayName($displayName)
	{
		// Make sure name length is valid
		if (strlen($displayName) < 8 OR strlen($displayName) > 32)
			return 1;
		
		// Name must be alphanumeric
		if (!preg_match('/^[a-zA-Z0-9]+$/', $displayName))
			return 1;
		
		// Search for the name in the banned list
		$this->db->Prepare('SELECT 1 FROM BannedNames WHERE BannedName=:DisplayName');
		$params = array(':DisplayName' => $displayName);

		$this->db->Execute($params);

		if ($this->db->FetchArray())
			return 2;

		// Search for the user name in the table
		$this->db->Prepare('SELECT UserID FROM Users WHERE DisplayName=:DisplayName');
		$params = array(':DisplayName' => $displayName);

		$this->db->Execute($params);

		if ($this->db->FetchArray())
			return 2;
		
		return 0;
	}
	
	private function DoesPasswordHaveDifferentCharacters($password)
	{
		$passwordLength = strlen($password);
		
		if ($passwordLength < 8)
			return false;
		
		// At least 8 different characters
		$arr = array();
		for ($i=0; $i < $passwordLength; ++$i)
		{
			$break = false;
			for ($j=0; $j < count($arr); ++$j)
			{
				if ($arr[$j] === $password[$i])
				{
					$break = true;
					break;
				}
			}
			if ($break)
				continue;
			
			// New character
			array_push($arr, $password[$i]);
			
			if (count($arr) >= 8)
				return true;
		}
		
		return false;
	}
	
	private function DoesPasswordContainUpperNumberOrSymbolAndLower($password)
	{
		$hasLowerLetter = preg_match('/[a-z]/', $password);
		$hasUpperLetter = preg_match('/[A-Z]/', $password);
		$hasDigit = preg_match('/\d/', $password);
		$hasSpecial = preg_match('/[^a-zA-Z\d]/', $password);
		
		// At least 1 symbol, capital, or number
		return $hasLowerLetter AND ($hasUpperLetter OR $hasDigit OR $hasSpecial);
	}
	
	private function IsPasswordSimilarToString($password, $arrStrings)
	{
		for ($i=0; $i < count($arrStrings); ++$i)
		{
			$badMatch = $arrStrings[$i];
			
			$lowerBad = strtolower($badMatch);
			$lowerPass = strtolower($password);
			
			similar_text($lowerPass, $lowerBad, $percentage);
			
			if ($percentage > 60)
				return true;
		}
		
		return false;
	}

	// 0 = Valid
	// 1 = Needs different characters
	// 2 = Needs Upper Number Symbol And Lower
	// 3 = Password too similar to text
	public function IsInvalidPassword($password, $arrBadMatches)
	{
		if (!$this->DoesPasswordHaveDifferentCharacters($password))
			return 1;
			
		if (!$this->DoesPasswordContainUpperNumberOrSymbolAndLower($password))
			return 2;
	
		if ($this->IsPasswordSimilarToString($password, $arrBadMatches))
			return 3;
		
		return 0;
	}
	
	private function ProcessEmailCode($code)
	{
		switch ($code)
		{
		case 1:	$this->error = 'Email Bad Format';			return 2;
		case 2:	$this->error = 'Email Already Taken';		return 3;
		case 3:		return -1;
		}
		
		return 0;
	}
	
	private function ProcessUserNameCode($code)
	{
		switch ($code)
		{
		case 1:	$this->error = 'User Name Bad Format';		return 4;
		case 2:	$this->error = 'User Name Already Taken';	return 5;
		case 3:		return -1;
		}
		
		return 0;
	}
	
	private function ProcessDisplayNameCode($code)
	{
		switch ($code)
		{
		case 1:	$this->error = 'Display Name Bad Format';	return 6;
		case 2:	$this->error = 'Display Name Already Taken';return 7;
		case 3:		return -1;
		}
		
		return 0;
	}
	
	private function ProcessPasswordCode($code)
	{
		switch ($code)
		{
			case 1:
				$this->error = 'Password needs different characters';
				return 8;
				
			case 2:
				$this->error = 'Password needs Upper Number Symbol And Lower';
				return 9;
				
			case 3:
				$this->error = 'Password too similar to name';
				return 10;
		}
		
		return 0;
	}
	
	private function ProcessCreationCode($code)
	{
		if ($code === 3)
			return -1;
		
		return 0;
	}

	// 1 = Valid
	// 2 = Email Bad Format
	// 3 = Email Already Taken
	// 4 = UserName Bad Format
	// 5 = UserName Already Taken
	// 6 = DisplayName Bad Format
	// 7 = DisplayName Already Taken
	// 8 = Password needs different characters
	// 9 = Password needs Upper Number Symbol And Lower
	// 10 = Password too similar to name
	public function CreateUser($authType, $userName, $displayName, $password, $email)
	{
		// Only validate user name for custom auth type. Other auth type usernames can't be denied.
		if ($authType === 1)
			$userNameToValidate = $userName;
		
		if (($code = $this->ValidateUserInput($userNameToValidate, $displayName, $password, $email)) !== 0)
			return $code;
		
		if (($code = $this->PrivCreateAccount($authType, $userName, $displayName, $password, $email)) !== 0)
			return $this->ProcessCreationCode($code);
		
		return 1;
	}
	
	private function ValidateUserInput($userName, $displayName, $password, $email)
	{
		// Validate user input
		if (isset($email))
		{
			if (($code = $this->IsInvalidEmail($email)) !== 0)
				return $this->ProcessEmailCode($code);
		}
		
		if (isset($userName))
		{
			if (($code = $this->IsInvalidUserName($userName)) !== 0)
				return $this->ProcessUserNameCode($code);
		}
		
		if (($code = $this->IsInvalidDisplayName($displayName)) !== 0)
			return $this->ProcessDisplayNameCode($code);
		
		if (isset($password))
		{
			if (($code = $this->IsInvalidPassword($password, array($userName, $displayName))) !== 0)
				return $this->ProcessPasswordCode($code);
		}
		
		return 0;
	}
	
	// 0 = Created account
	private function PrivCreateAccount($authType, $userName, $displayName, $password, $email)
	{
		// Create user
		$emailToken = generate_password();
		$sessionToken = generate_password();
		$passwordHash = password_hash($password, PASSWORD_DEFAULT);
		$emailTokenHash = password_hash($emailToken, PASSWORD_DEFAULT);

		$this->db->BeginTransaction();

		$this->db->Prepare('INSERT INTO Users (Email, AuthTypeID, UserName, DisplayName, Password, EmailToken, SessionToken, SessionLastUpdated, LastLoginTime, CreationDate, CreatedIPAddress) VALUES'
					.'  (:Email, :AuthTypeID, :UserName, :DisplayName, :Password, :EmailToken, :SessionToken, NOW(), NOW(), NOW(), :CreatedIPAddress)');

		$params = array(':Email' => $email,
						':AuthTypeID' => $authType,
						':UserName' => $userName,
						':DisplayName' => $displayName,
						':Password' => $passwordHash,
						':EmailToken' => $emailTokenHash,
						':SessionToken' => $sessionToken,
						':CreatedIPAddress' => $_SERVER['REMOTE_ADDR']);
				
		$this->db->Execute($params);

		$userID = $this->db->LastInsertID();

		if (!$this->CreateNewUserData($userID))
		{
			$this->db->Rollback();
			return 3;
		}

		$this->db->Commit();
		
		if (isset($email))
		{
			$sendMail = new VerificationEmail($this->db);
			$sendMail->Send($email, $userName, $emailToken);
		}
		
		return 0;
	}
	
	// Creates the starting data for the user.
	// NOTE: Wrap this in a transaction with the main Users insert.
	private function CreateNewUserData($userID)
	{
		return true;
	}
}

?>