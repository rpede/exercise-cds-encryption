# Encryption exercise

## Overview

The goal of this exercise is to implement a simple API that stores a secret
message using encryption.
The message can only be read by someone who knows the password.

## Getting started

```sh
dotnet watch
```

Open <http://localhost:5097/swagger/index.html>

Implement the methods in [SecretController](Controllers/SecretController.cs).

## Theory

### Encryption

[Advanced Encryption Standard (AES)](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard)
is a widely used [symmetric-key
algorithm](https://en.wikipedia.org/wiki/Symmetric-key_algorithm).
The security of which has hold up to intense scrutiny.

AES is a block cipher.
Meaning it can only encrypt a block(/chunk) of data of a
fixed length (128 bits).

It can however be used to encrypt data larger than 128 bits (16 bytes) by
chaining blocks together.
The method of chaining blocks together is called the [mode of
operation](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation).
[GCM](https://en.wikipedia.org/wiki/Galois/Counter_Mode) and [CCM](https://en.wikipedia.org/wiki/CCM_mode) are the preferred modes.
If they are not available then
[CTR](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation#Counter_%28CTR%29)
or
[CBC](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation#Cipher_Block_Chaining_%28CBC%29).

All the mentioned modes requires a long completely random value to be mixed
into the ciphering.
A random value used in this context is commonly referred to as a Salt, Nonce or
Initialization Vector (IV).
Just different names for the same thing.

It is very important, that a different IV is used each time something is
encrypted with the same key.
The level of randomness matters too.
Not all random number generators are suitable for use in cryptography.
It is therefore important to use a [cryptographically secure pseudorandom
number generator
(CSPRNG)](https://en.wikipedia.org/wiki/Cryptographically_secure_pseudorandom_number_generator).
If in doubt, just search for "secure random" + your programming language.

The perfect encryption algorithm is only as secure as the key.

### Key

In general, a good encryption key is just a completely random value of a given
length matching the encryption algorithm.
For AES the maximum key size is 256 bits.

Humans are not very good at remembering long random values such as
`+hPinCWAAFMubNm6jkh0iypxR1lG+wWNViN7J4Zh8dtNTIMyKUqTwJ7jn2QjflRq`.

A specialized hash function called [key derivation function
(KDF)](https://en.wikipedia.org/wiki/Key_derivation_function) can be used
derive an encryption key from a password.
A computational expensive KDF should be used to make it difficult to
[crack](https://en.wikipedia.org/wiki/Password_cracking) the password.

With a 256 bit key there are 1.157920892×10⁷⁷ possible values.
It is not practical for an attacker to go through all possibilities in order to
guess the encryption key.
Therefore, an attacker in this setup will be likely to attempt to guess the
password instead.

By using a slow KDF, the attacker will be forced to use more computing
resources for each guess.
If the resource usage is high enough, then it becomes impractical for them to
attempt to guess the password.

For simplicity, we will use [PBKDF2](https://en.wikipedia.org/wiki/PBKDF2) as a
slow KDF since an implementation is already available in .NET.

It is worth noting that there are other algorithms providing more security.
Here is a ranking:

1. [Argon2id](https://en.wikipedia.org/wiki/Argon2)
2. [scrypt](https://en.wikipedia.org/wiki/Scrypt) if Argon2id is unavailable
3. [bcrypt](https://en.wikipedia.org/wiki/Bcrypt) for legacy systems
4. [PBKDF2](https://en.wikipedia.org/wiki/PBKDF2) if none of the other options
   are available.

_Ranking is based on [OWASP recommendations](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)_

## Implementation

Implement the missing methods in `SecretController`.
Here are the steps to safely store a message.

Use
[RandomNumberGenerator](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator)
to generate salt.

```cs
var salt = RandomNumberGenerator.GetBytes(256 / 8);
```

Then use
[KeyDerivation.Pbkdf2](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.cryptography.keyderivation.keyderivation.pbkdf2)
to generate a key from the password.

```cs
var key = KeyDerivation.Pbkdf2(
  password,
  salt,
  KeyDerivationPrf.HMACSHA256,
  iterationCount: 600_000,
  numBytesRequested: 256 / 8
);
```

It basically means hash `password` + `salt` over and over, 600.000 times (to
make it slow) using HMAC-SHA256 as hash function.

Finally encrypt with AES-GCM.
See "[Authenticated Encryption in .NET with AES-GCM](https://www.scottbrady91.com/c-sharp/aes-gcm-dotnet)" for instructions.

That gives you the following values you need to store:

- Salt
- Nonce
- Cipher text
- Tag

Use a class or a record to hold the values.

```cs
public class EncryptedMessage
{
    public required byte[] Salt { get; set; }
    public required byte[] Nonce { get; set; }
    public required byte[] CipherText { get; set; }
    public required byte[] Tag { get; set; }
}
```

JSON serialize with `JsonSerializer.Serialize`.
Then store the encrypted and serialized message in a file.

- [How to: Write text to a file](https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-write-text-to-a-file)
- [How to: Read text from a file](https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-read-text-from-a-file)

## Workaround for macOS

It has been a while since I last tested on Mac, so don't know if this still
applies.

`AesGcm` depends on platform provided crypto libraries.
macOS comes with an older version of OpenSSL that doesn't
support GCM.

You can either use Bouncy Castle library as descriped in the "Bonus: AES-GCM
Encryption using Bouncy Castle" section of [this
article](https://www.scottbrady91.com/c-sharp/aes-gcm-dotnet).

Or, you can install a newer version of OpenSSL using homebrew.

Install homebrew if you don't have it already:

```sh
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

Use brew (short for homebrew) to install openssl:

```sh
brew install openssl@3
```

Add following line to `.zshrc` so it knows where to look for the lib.

```sh
export DYLD_LIBRARY_PATH=/opt/homebrew/opt/openssl@3/lib
```
