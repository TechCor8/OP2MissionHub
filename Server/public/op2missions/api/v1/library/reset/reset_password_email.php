<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// reset_password_email.php
// Sends a reset password email to a user.
////////////////////////////////////////////////////////////////////////////////////////////////////
require_once INCLUDE_PATH_PUBLIC . '/library/constants/app_constants.php';

class ResetPasswordEmail
{
	private $db = null;
	
	private $appName = null;
	private $appUrl = null;
	
	private $publisherName = null;
	private $publisherUrl = null;
	private $publisherEmail = null;
	
	private $resetUrl = null;
	private $formUrl = null;
	
	
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
		$resetPath = realpath(INCLUDE_PATH_PUBLIC . '/reset/reset_password.php');
		$this->resetUrl = str_replace($_SERVER['DOCUMENT_ROOT'], SERVER_URL, $resetPath);
		
		$formPath = realpath(INCLUDE_PATH_PUBLIC . '/reset/reset_password_form.php');
		$this->formUrl = str_replace($_SERVER['DOCUMENT_ROOT'], SERVER_URL, $formPath);
	}
			
	public function Send($email, $userName, $token)
	{
		$urlUserName = rawurlencode($userName);
		$token = rawurlencode($token);
		$resetUrl = rawurlencode($this->resetUrl);
		
		$header  = 'MIME-Version: 1.0' . "\r\n";
		$header .= 'Content-type: text/html; charset=iso-8859-1' . "\r\n";
		$header .= 'From: '.$this->appName.' <'.$this->publisherEmail.'>' . "\r\n";
		
		$subject = 'Reset Password Request: '.$this->appName;

		$message = '
<html>
<head>
<title>' . $subject . '</title>
<body>
Hello ' . $userName . ',
<br/><br/>
We have received a request to reset your '.$this->appName.' password.
<br/><br/>
<b>You can reset your password by clicking the link below:
<br/>
<a href="'.$this->formUrl .'?u='.$urlUserName .'&token='.$token .'&url='.$resetUrl .'&title='.$this->appName .'"
target="_blank" rel="nofollow">
'.$this->formUrl .'?u='.$urlUserName .'&token='.$token .'&url='.$resetUrl .'&title='.$this->appName .'
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
