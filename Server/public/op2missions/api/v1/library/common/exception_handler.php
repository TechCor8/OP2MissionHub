<?php
////////////////////////////////////////////////////////////////////////////////////////////////////
// exception_handler.php
// Sets the default exception handler.
// This is primarily used for gracefully handling fatal errors by reporting a 
// generic error message to the user.
////////////////////////////////////////////////////////////////////////////////////////////////////

// exception_error_handler converts errors to exceptions
function exception_error_handler($severity, $message, $file, $line)
{
	if (!(error_reporting() & $severity))
	{
		// This error code is not included in error_reporting
		return;
	}
	throw new ErrorException($message, 0, $severity, $file, $line);
}

set_error_handler('exception_error_handler');

function global_exception_handler($exception)
{
	error_log($exception);
	
	echo CreateErrorResponse(-1, 'Unhandled exception');	// Production
	//echo CreateErrorResponse(-1, $exception->__toString());	// Development
}

set_exception_handler('global_exception_handler');

?>