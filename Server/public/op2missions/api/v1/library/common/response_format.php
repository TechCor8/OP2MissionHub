<?php
// Standardized format for returning responses to the caller

// Returns an error code with error details
function CreateErrorResponse($code, $message)
{
	return json_encode(array('Code' => $code,
							'Message' => $message));
}

// Returns a success code with optional success message
function CreateSuccessResponse($code, $message='Success')
{
	return json_encode(array('Code' => $code,
							'Message' => $message));
}

// Adds code and message to response data
function CreateResponse($code, $data, $message='Success')
{
	$data['Code'] = $code;
	$data['Message'] = $message;
	return json_encode($data);
}

?>