<?php

function generate_password($length = 24) {

    if(function_exists('openssl_random_pseudo_bytes')) {
        $password = base64_encode(openssl_random_pseudo_bytes($length, $strong));
        if($strong == TRUE)
            return substr($password, 0, $length); //base64 is about 33% longer, so we need to truncate the result
    }

    # fallback to mt_rand if php < 5.3 or no openssl available
    $characters = '0123456789';
    $characters .= 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/+'; 
    $charactersLength = strlen($characters)-1;
    $password = '';

    # select some random characters
    for ($i = 0; $i < $length; $i++) {
        $password .= $characters[mt_rand(0, $charactersLength)];
    }

    return $password;
}

function generate_random_serial($length = 24)
{
	$characters = '0123456789';
    $characters .= 'BCDFGHJKLMNPQRSTVWXYZ';
    $charactersLength = strlen($characters)-1;
    $randStr = '';

    if(function_exists('openssl_random_pseudo_bytes')) {
        $byteStr = openssl_random_pseudo_bytes($length, $strong);
        if($strong == TRUE)
		{
			for ($i = 0; $i < $length; ++$i)
				$randStr .= $characters[ord($byteStr[$i]) % $charactersLength];
			
			return $randStr;
		}
    }

    # fallback to mt_rand if php < 5.3 or no openssl available

    # select some random characters
    for ($i = 0; $i < $length; $i++) {
        $randStr .= $characters[mt_rand(0, $charactersLength)];
    }        

    return $randStr;
}

?>