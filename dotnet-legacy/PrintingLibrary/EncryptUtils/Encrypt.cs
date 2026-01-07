
using System.Security.Cryptography;
using System.Text;

namespace PrintingLibrary.EncryptUtils;

public static class EncryptUtil {

  private const int SaltSize = 32; // 256 bits
  private const int IvSize = 16; // 128 bits for AES
  private const int KeySize = 32; // 256 bits for AES-256
  private const int Iterations = 100000; // PBKDF2 iterations

  /// <summary>
  /// Encrypts a file using AES-256-CBC with PBKDF2 key derivation.
  /// The encrypted file format: [Salt(32 bytes)][IV(16 bytes)][Encrypted Data]
  /// </summary>
  public static void EncryptFile(string inputPath, string outputPath, string password) {
    if (!File.Exists(inputPath))
      throw new FileNotFoundException("Input file not found", inputPath);

    using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
    EncryptStreamToFile(inputStream, outputPath, password);
  }

  public static void EncryptStreamToFile(Stream inputStream, string outputPath, string password) {
    if (string.IsNullOrEmpty(password))
      throw new ArgumentException("Password cannot be empty", nameof(password));

    // Generate random salt and IV
    byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
    byte[] iv = RandomNumberGenerator.GetBytes(IvSize);

    // Derive key from password using PBKDF2
    using var keyDerivation = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
    byte[] key = keyDerivation.GetBytes(KeySize);

    // Read input stream
    byte[] plainBytes;
    if (inputStream is MemoryStream ms) {
      plainBytes = ms.ToArray();
    } else {
      using var tempMs = new MemoryStream();
      inputStream.CopyTo(tempMs);
      plainBytes = tempMs.ToArray();
    }

    // Encrypt the data
    using var aes = Aes.Create();
    aes.Key = key;
    aes.IV = iv;
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;

    using var encryptor = aes.CreateEncryptor();
    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

    // Write to output file: [Salt][IV][Encrypted Data]
    using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
    outputStream.Write(salt, 0, salt.Length);
    outputStream.Write(iv, 0, iv.Length);
    outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
  }

  /// <summary>
  /// Decrypts a file that was encrypted with EncryptFile.
  /// </summary>
  public static void DecryptFile(string inputPath, string outputPath, string password) {
    if (!File.Exists(inputPath))
      throw new FileNotFoundException("Input file not found", inputPath);

    if (string.IsNullOrEmpty(password))
      throw new ArgumentException("Password cannot be empty", nameof(password));

    // Read encrypted file
    byte[] encryptedData = File.ReadAllBytes(inputPath);

    if (encryptedData.Length < SaltSize + IvSize)
      throw new InvalidDataException("File is too small to be an encrypted file");

    // Extract salt and IV
    byte[] salt = new byte[SaltSize];
    byte[] iv = new byte[IvSize];
    Array.Copy(encryptedData, 0, salt, 0, SaltSize);
    Array.Copy(encryptedData, SaltSize, iv, 0, IvSize);

    // Extract encrypted content
    int encryptedContentLength = encryptedData.Length - SaltSize - IvSize;
    byte[] encryptedBytes = new byte[encryptedContentLength];
    Array.Copy(encryptedData, SaltSize + IvSize, encryptedBytes, 0, encryptedContentLength);

    // Derive key from password using the same salt
    using var keyDerivation = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
    byte[] key = keyDerivation.GetBytes(KeySize);

    // Decrypt the data
    using var aes = Aes.Create();
    aes.Key = key;
    aes.IV = iv;
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;

    try {
      using var decryptor = aes.CreateDecryptor();
      byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

      // Write decrypted data to output file
      File.WriteAllBytes(outputPath, decryptedBytes);
    } catch (CryptographicException) {
      throw new CryptographicException("Decryption failed. The password may be incorrect or the file may be corrupted.");
    }
  }

  /// <summary>
  /// Encrypts a file in place (replaces the original file with encrypted version).
  /// Creates a temporary file during encryption to avoid data loss on failure.
  /// </summary>
  public static void EncryptFileInPlace(string filePath, string password) {
    if (!File.Exists(filePath))
      throw new FileNotFoundException("File not found", filePath);

    string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    try {
      EncryptFile(filePath, tempFile, password);
      File.Delete(filePath);
      File.Move(tempFile, filePath);
    } catch {
      // Clean up temp file if it exists
      if (File.Exists(tempFile))
        File.Delete(tempFile);
      throw;
    }
  }

  /// <summary>
  /// Decrypts a file in place (replaces the encrypted file with decrypted version).
  /// Creates a temporary file during decryption to avoid data loss on failure.
  /// </summary>
  public static void DecryptFileInPlace(string filePath, string password) {
    if (!File.Exists(filePath))
      throw new FileNotFoundException("File not found", filePath);

    string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    try {
      DecryptFile(filePath, tempFile, password);
      File.Delete(filePath);
      File.Move(tempFile, filePath);
    } catch {
      // Clean up temp file if it exists
      if (File.Exists(tempFile))
        File.Delete(tempFile);
      throw;
    }
  }

  public static string GenerateMetaHash(string filePath) {
    byte[] fileBytes = File.ReadAllBytes(filePath);
    var md5Hash = MD5.HashData(fileBytes);
    var sha1Hash = SHA1.HashData(fileBytes);
    var sha256Hash = SHA256.HashData(fileBytes);

    // Concatenate all hashes
    var combined = md5Hash.Concat(sha1Hash).Concat(sha256Hash).ToArray();

    // Hash the combined hashes for fixed output size
    var result = SHA256.HashData(combined);

    return Convert.ToHexString(result);
  }


  public static string GetFileHash(string filePath, string secret) {
    var keyBytes = Encoding.UTF8.GetBytes(secret);
    using var hmac = new HMACSHA256(keyBytes);
    using var stream = System.IO.File.OpenRead(filePath);
    var hashBytes = hmac.ComputeHash(stream);
    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
  }
}