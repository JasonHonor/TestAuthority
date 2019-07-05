using System;
using System.Security.Cryptography.X509Certificates;
using Nelibur.Sword.Extensions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace TestAuthorityCore.X509
{
    public class CertificateBuilder2 : ICertificateBuilder
    {
        private const string SignatureAlgorithm = "SHA256WithRSA";
        private readonly X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
        private readonly SecureRandom random;
        private AsymmetricCipherKeyPair keyPair;

        BigInteger mSerialNo;

        public CertificateBuilder2(IServiceProvider provider,SecureRandom random, BigInteger serialNumber=null,int keyStrength = 2048)
        {
            KeyStrength = keyStrength;
            this.random = random;

            ConfigService cfg = (ConfigService)provider.GetService(typeof(ConfigService));
            KeyStrength = cfg.GetInt("KeyStrength");

            //change the serial number;
            if(null==serialNumber)
                serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);

            //SerialNo = serialNumber;
            certificateGenerator.SetSerialNumber(serialNumber);
        }

        public X509Name Issuer { get; set; }

        public int KeyStrength { get; }

        public AsymmetricKeyParameter PublicKeyInfo => keyPair.Public;

        public X509Name Subject { get; set; }

        public BigInteger SerialNo { get { return mSerialNo; } set { mSerialNo = value; certificateGenerator.SetSerialNumber(mSerialNo); } } 

        public static AsymmetricCipherKeyPair GenerateKeyPair(int keyStrength, SecureRandom random)
        {
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            AsymmetricCipherKeyPair subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            return subjectKeyPair;
        }

        public ICertificateBuilder AddExtension(string oid, bool isCritical, Asn1Encodable value)
        {
            certificateGenerator.AddExtension(oid, isCritical, value);
            return this;
        }

        public CertificateWithKey Generate()
        {
            return Generate(keyPair);
        }

        public CertificateWithKey Generate(AsymmetricCipherKeyPair signerKeyPair)
        {
            Validate();

            ISignatureFactory signatureFactory = new Asn1SignatureFactory(SignatureAlgorithm, signerKeyPair.Private, random);
            certificateGenerator.SetPublicKey(keyPair.Public);

            X509Certificate certificate = certificateGenerator.Generate(signatureFactory);
            certificate.Verify(signerKeyPair.Public);
            var x509 = new X509Certificate2(certificate.GetEncoded());

            var result = new CertificateWithKey
            {
                Certificate = x509,
                KeyPair = keyPair
            };
            return result;
        }

        public ICertificateBuilder SetIssuer(X509Name issuer)
        {
            Issuer = issuer;
            certificateGenerator.SetIssuerDN(issuer);
            return this;
        }

        public ICertificateBuilder SetNotAfter(DateTimeOffset notAfter)
        {
            certificateGenerator.SetNotAfter(notAfter.UtcDateTime);
            return this;
        }

        public ICertificateBuilder SetNotBefore(DateTimeOffset notBefore)
        {
            certificateGenerator.SetNotBefore(notBefore.UtcDateTime);
            return this;
        }

        public ICertificateBuilder SetSubject(X509Name subject)
        {
            Subject = subject;
            certificateGenerator.SetSubjectDN(subject);
            return this;
        }

        public ICertificateBuilder WithBasicConstraints(BasicConstrainsConstants constrains)
        {
            if (constrains == BasicConstrainsConstants.EndEntity)
            {
                certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
                return this;
            }

            certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(1));
            return this;
        }

        public ICertificateBuilder WithKeyPair(AsymmetricCipherKeyPair keyPair)
        {
            this.keyPair = keyPair;
            return this;
        }

        private void Validate()
        {
            if (Issuer.IsNull())
            {
                throw new InvalidOperationException("Issuer is empty");
            }

            if (Subject.IsNull())
            {
                throw new InvalidOperationException("Issuer is empty");
            }

            if (PublicKeyInfo.IsNull())
            {
                throw new InvalidOperationException("PublicKeyInfo is empty");
            }
        }
    }
}
