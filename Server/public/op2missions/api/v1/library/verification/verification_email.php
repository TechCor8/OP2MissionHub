<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// verification_email.php
// Sends a confirmation email to a user.
////////////////////////////////////////////////////////////////////////////////////////////////////
require_once INCLUDE_PATH_PUBLIC . '/library/constants/app_constants.php';

class VerificationEmail
{
	private $db = null;
	
	private $appName = null;
	private $appUrl = null;
	
	private $publisherName = null;
	private $publisherUrl = null;
	private $publisherEmail = null;
	
	private $verifyUrl = null;
	
	
	public function __construct($db)
	{
		$this->db = $db;
		
		// Get app name and url
		$appConstants = new AppConstants($this->db);
		
		$constants = $appConstants->GetConstants(array(AppConstants::APP_NAME,
														AppConstants::APP_URL,
														AppConstants::PUBLISHER_NAME,
														AppConstants::PUBLISHER_URL,
														AppConstants::PUBLISHER_EMAIL));
														
		$this->appName = $constants[AppConstants::APP_NAME];
		$this->appUrl = $constants[AppConstants::APP_URL];
		$this->publisherName = $constants[AppConstants::PUBLISHER_NAME];
		$this->publisherUrl = $constants[AppConstants::PUBLISHER_URL];
		$this->publisherEmail = $constants[AppConstants::PUBLISHER_EMAIL];
		
		// Get full path of file to put into email.
		// Then replace the root path with the service url.
		$verifyPath = realpath(INCLUDE_PATH_PUBLIC . '/verification/verify_email.php');
		$this->verifyUrl = str_replace($_SERVER['DOCUMENT_ROOT'], SERVER_URL, $verifyPath);
	}
			
	public function Send($email, $userName, $token)
	{
		$urlUserName = rawurlencode($userName);
		$token = rawurlencode($token);
		
		$header  = 'MIME-Version: 1.0' . "\r\n";
		$header .= 'Content-type: text/html; charset=iso-8859-1' . "\r\n";
		$header .= 'From: '.$this->appName.' <'.$this->publisherEmail.'>' . "\r\n";
		
		$subject = 'Email Confirmation: '.$this->appName;

		$message = '
<html>
<head>
<title>' . $subject . '</title>
<body>
Hello ' . $userName . ',
<br/><br/>
Thank you for creating an account for '.$this->appName.'!
<br/><br/>
<b>Please confirm your email address by clicking the link below:
<br/>
<a href="'.$this->verifyUrl . '?u=' . $urlUserName . '&token=' . $token . '"
target="_blank" rel="nofollow">
'.$this->verifyUrl . '?u=' . $urlUserName . '&token=' . $token . '
</a></b>
<br/><br/>
If you have any questions or comments, please contact the developer.
<br/><br/>
Thank you,
<br/><br/>
The '.$this->publisherName.' Team
<br/><br/>
--
<br/>
<a href="'.$this->appUrl.'" target="_blank" rel="nofollow">
'.$this->appName.'
</a>
|
<a href="'.$this->publisherUrl.'" target="_blank" rel="nofollow">
'.$this->publisherName.'
</a>
</body>
</html>
';

		mail($email, $subject, $message, $header);
	}
}


?>
