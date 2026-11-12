#!/usr/bin/env dotnet
#:property LangVersion=preview
// .NET 11: post-quantum cryptography
//
// "Harvest now, decrypt later": attackers can store encrypted traffic today and decrypt it
// once quantum computers exist. ML-KEM is a key exchange that holds up against that.
//
//   1. ML-KEM manually: Encapsulate/Decapsulate a shared secret -> HKDF -> AES-GCM
//   2. Composite ML-KEM: ML-KEM combined with a classic algorithm (X25519/ECDH/RSA)
//   3. HPKE: KEM + KDF + AEAD in one API, handles nonces, integrity and message order
//
// Run: dotnet run PostQuantum.cs
//
// Needs OpenSSL 3.5+ on Linux (or a recent Windows) for ML-KEM.
// CompositeMLKem and Hpke are [Experimental] in .NET 11, flags expected to go away in .NET 12.
#pragma warning disable SYSLIB5006 // CompositeMLKem
#pragma warning disable SYSLIB5009 // Hpke

using System.Security.Cryptography;
using System.Text;
using static System.Console;

if (!MLKem.IsSupported)
{
    WriteLine("ML-KEM is not supported on this platform (needs OpenSSL 3.5+).");
    return;
}

byte[] message = Encoding.UTF8.GetBytes("Harvest now, decrypt later? Not this one.");

// 1. Plain ML-KEM: Encapsulate/Decapsulate -> KDF -> AES (the manual flow from the talk)
WriteLine("== ML-KEM-768 + HKDF + AES-GCM (manual)");
using (MLKem recipientKey = MLKem.GenerateKey(MLKemAlgorithm.MLKem768))
{
    // Sender only has the public encapsulation key
    byte[] publicKey = recipientKey.ExportEncapsulationKey();
    using MLKem senderView = MLKem.ImportEncapsulationKey(MLKemAlgorithm.MLKem768, publicKey);

    senderView.Encapsulate(out byte[] encapsulated, out byte[] senderSecret);
    var (nonce, ciphertext, tag) = Encrypt(senderSecret, message);

    // Recipient recovers the same shared secret from the encapsulated bytes
    byte[] recipientSecret = recipientKey.Decapsulate(encapsulated);
    WriteLine($"Public key: {publicKey.Length} bytes, encapsulated secret: {encapsulated.Length} bytes");
    WriteLine($"Shared secrets match: {senderSecret.AsSpan().SequenceEqual(recipientSecret)}");
    WriteLine($"Decrypted: {Encoding.UTF8.GetString(Decrypt(recipientSecret, nonce, ciphertext, tag))}");
}
WriteLine();

// 2. Composite ML-KEM: post-quantum + classic in one key, safe even if one of them is broken
WriteLine("== Composite ML-KEM (ML-KEM + classic)");
using (var composite = CompositeMLKem.GenerateKey(CompositeMLKemAlgorithm.MLKem768WithX25519))
{
    composite.Encapsulate(out byte[] encapsulated, out byte[] secret);
    byte[] decapsulated = composite.Decapsulate(encapsulated);
    WriteLine($"{composite.Algorithm.Name}: secrets match = {secret.AsSpan().SequenceEqual(decapsulated)}");
}
WriteLine();

// 3. HPKE: KEM + KDF + AEAD in one API, nonces and ordering handled for you
// A suite = KEM (key exchange) + KDF (key derivation) + AEAD (encryption).
// Prefer the post-quantum hybrid suite. ML-KEM in HPKE isn't supported on every
// platform yet (not on Linux/OpenSSL in my tests), so fall back to classic X25519.
WriteLine("== HPKE");
var suite = new HpkeSuite(HpkeKem.MLKEM768_P256, HpkeKdf.HKDF_SHA256, HpkeAead.AES_256_GCM);
if (!Hpke.IsSupported(suite))
{
    WriteLine($"{suite.Name} not supported here, falling back to classic X25519");
    suite = new HpkeSuite(HpkeKem.DHKEM_X25519_HKDF_SHA256, HpkeKdf.HKDF_SHA256, HpkeAead.AES_256_GCM);
}
WriteLine($"Suite: {suite.Name}");

using Hpke recipient = Hpke.GenerateKey(suite);
using Hpke sender = Hpke.ImportEncapsulationKey(suite, recipient.ExportEncapsulationKey());
byte[] info = Encoding.UTF8.GetBytes("dotnetconf2026 demo");
byte[] aad = Encoding.UTF8.GetBytes("header-v1");

// Single shot
sender.Seal(message, out byte[] enc, out byte[] sealedMessage, aad, info);
WriteLine($"Single shot: {Encoding.UTF8.GetString(recipient.Open(enc, sealedMessage, aad, info))}");

// Multiple messages over one context: sequence numbers/nonces are managed internally
using HpkeSender stream = sender.CreateSender(out byte[] streamEnc, info);
using HpkeRecipient reader = recipient.CreateRecipient(streamEnc, info);
foreach (var part in new[] { "first", "second", "third" })
{
    byte[] sealedPart = stream.Seal(Encoding.UTF8.GetBytes(part), aad);
    WriteLine($"Stream: {Encoding.UTF8.GetString(reader.Open(sealedPart, aad))}");
}

// Tampering is detected
sealedMessage[^1] ^= 0xFF;
try
{
    recipient.Open(enc, sealedMessage, aad, info);
}
catch (CryptographicException)
{
    WriteLine("Tampered message rejected");
}

static (byte[] Nonce, byte[] Ciphertext, byte[] Tag) Encrypt(byte[] sharedSecret, byte[] plaintext)
{
    byte[] key = HKDF.DeriveKey(HashAlgorithmName.SHA256, sharedSecret, 32, info: "aes key"u8.ToArray());
    byte[] nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
    byte[] ciphertext = new byte[plaintext.Length];
    byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];
    using var aes = new AesGcm(key, tag.Length);
    aes.Encrypt(nonce, plaintext, ciphertext, tag);
    return (nonce, ciphertext, tag);
}

static byte[] Decrypt(byte[] sharedSecret, byte[] nonce, byte[] ciphertext, byte[] tag)
{
    byte[] key = HKDF.DeriveKey(HashAlgorithmName.SHA256, sharedSecret, 32, info: "aes key"u8.ToArray());
    byte[] plaintext = new byte[ciphertext.Length];
    using var aes = new AesGcm(key, tag.Length);
    aes.Decrypt(nonce, ciphertext, tag, plaintext);
    return plaintext;
}
