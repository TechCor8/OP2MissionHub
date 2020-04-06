<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// reset_password_form.php
// Displays the password form for a user that wants to reset their password.
// Called through an email link.
////////////////////////////////////////////////////////////////////////////////////////////////////

$appName = $_GET['title']	?? $_POST['title']		?? null;
$userName = $_GET['u']		?? $_POST['u']			?? null;
$token = $_GET['token']		?? $_POST['token']		?? null;
$resetUrl = $_GET['url']	?? $_POST['url']		?? null;

$password = $_GET['p']		?? $_POST['p']			?? null;
$password2 = $_GET['p2']	?? $_POST['p2']			?? null;

$showForm = true;

if (!isset($userName))		$showForm = false;
if (!isset($token))			$showForm = false;
if (!isset($resetUrl))		$showForm = false;

if (!$showForm)
	$error = 'Bad Request';

if ($showForm)
{
	if (isset($password))
	{
		if ($password === $password2)
		{
			// Attempt to set password
			$myVars = 'u='.rawurlencode($userName).'&token='.rawurlencode($token).'&p='.$password;
			
			$ch = curl_init($resetUrl);
			curl_setopt($ch, CURLOPT_POST, 1);
			curl_setopt($ch, CURLOPT_POSTFIELDS, $myVars);
			curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
			curl_setopt($ch, CURLOPT_HEADER, false);
			curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

			$response = json_decode(curl_exec($ch), true);
			
			curl_close($ch);
			
			if (json_last_error() !== JSON_ERROR_NONE)
			{
				$showForm = false;
				$error = 'Bad Request';
			}
			
			if ($response['Code'] != 1)
			{
				if ($response['Code'] == 2)
					$showForm = false;
				
				$error = $response['Message'];
				//$error = 'We\'re sorry, that token is invalid or has expired.';
			}
			else
			{
				$showForm = false;
				$successMessage = $userName . '\'s password has been reset successfully.';
			}
		}
		else
		{
			$error = 'Passwords do not match';
		}
	}
	else
	{
		// Check if request is valid
		$myVars = 'u='.rawurlencode($userName).'&token='.rawurlencode($token).'&check=1';
		
		$ch = curl_init($resetUrl);
		curl_setopt($ch, CURLOPT_POST, 1);
		curl_setopt($ch, CURLOPT_POSTFIELDS, $myVars);
		curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
		curl_setopt($ch, CURLOPT_HEADER, false);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

		$response = json_decode(curl_exec($ch), true);
		
		curl_close($ch);
		
		if (json_last_error() !== JSON_ERROR_NONE)
		{
			$showForm = false;
			$error = 'Bad Request';
		}
		
		if ($response['Code'] != 1)
		{
			$showForm = false;
			//$error = $response['Message'];
			$error = 'We\'re sorry, that token is invalid or has expired.';
		}
	}
}

?>


<!doctype html>

<html lang="en">
<head>
  <meta charset="utf-8">

  <title>Reset Password: <?php echo $appName; ?></title>
  <meta name="description" content="Reset Password">

</head>

<body>

<main>

	<h4>
	<?php echo $appName; ?>
	</h4>
	<p>

<?php
// Display error if one is set
if (isset($error))
{
	echo '<font color="red">' . $error . '</font>';
	echo '<br><br>';
}
else if (isset($successMessage))
{
	echo $successMessage;
	echo '<br><br>';
}

if (!$showForm)
{
	// End page before printing password form
	echo '</p>';
	
	echo '</main>';
	
	echo '</body>';
	echo '</html>';
}
else
{
?>
	
Create a new password for <?php echo $userName; ?>.<br><br>
<form action="<?php echo $_SERVER['PHP_SELF'] ?>" method="POST">
<input type="hidden" name="title" value="<?php echo $appName; ?>">
<input type="hidden" name="u" value="<?php echo $userName; ?>">
<input type="hidden" name="token" value="<?php echo $token; ?>">
<input type="hidden" name="url" value="<?php echo $resetUrl; ?>">
New Password:<br><input type="password" name="p"><br>
Confirm New Password:<br><input type="password" name="p2"><br>
<br><input type="submit" value="Submit">
</form>

</p>

</main>

</body>
</html>

<?php
}
?>