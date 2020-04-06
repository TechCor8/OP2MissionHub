<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// dbconnect.php
// Creates the AppDatabase object and sets it to use the configuration in the required private file.
////////////////////////////////////////////////////////////////////////////////////////////////////
require INCLUDE_PATH_PRIVATE . '/dbconfig.php';

class AppDatabase
{
	private $pdo;
	private $statement;
	
	function Connect()
	{
		$this->pdo = ConnectDB();
		
		if ($this->pdo === false)
			throw new Exception('Failed to connect to database.');
		
		return true;
	}
	
	function Prepare($query)
	{
		$this->statement = $this->pdo->prepare($query);
	}
	
	function Execute($params = null)
	{
		return $this->statement->execute($params);
	}
	
	function FetchArray()
	{
		return $this->statement->fetch(PDO::FETCH_NUM);
	}
	
	function FetchDictionary()
	{
		return $this->statement->fetch(PDO::FETCH_ASSOC);
	}
	
	function LastInsertID()
	{
		return $this->pdo->lastInsertID();
	}
	
	function RowCount()
	{
		return $this->statement->rowCount();
	}
	
	function BeginTransaction()
	{
		return $this->pdo->beginTransaction();
	}
	
	function Commit()
	{
		return $this->pdo->commit();
	}
	
	function Rollback()
	{
		return $this->pdo->rollback();
	}
}

?>