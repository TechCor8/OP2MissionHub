<?php

// Removes special characters used for fetching data.
function sanitize($text)
{
	$text = str_replace('|', '', $text);
	$text = str_replace('@', '', $text);
	$text = str_replace('^', '', $text);
	
	return $text;
}

function sanitizeLike($text, $escapeChar)
{
	$search = array($escapeChar, '_', '%');
	$replace = array($escapeChar.$escapeChar, $escapeChar.'_', $escapeChar.'%');
	
	return str_replace($search, $replace, $text);
}

?>