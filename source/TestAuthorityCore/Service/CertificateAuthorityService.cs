﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using TestAuthorityCore.Extensions;
using TestAuthorityCore.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace TestAuthorityCore.Service
{
    public class CertificateAuthorityService
    {
        private readonly Func<SecureRandom, CertificateWithKey, CertificateBuilder2> builderFactory;
        private readonly RandomService randomService;
        private readonly CertificateWithKey SignerCertificate;

        public CertificateAuthorityService(IServiceProvider provider,CertificateWithKey signerCertificate, RandomService randomService)
        {
            SignerCertificate = signerCertificate;
            this.randomService = randomService;

            builderFactory = (random, issuer) => new CertificateBuilder2(provider,random);
        }

        public byte[] GenerateSslCertificate(PfxCertificateRequest request,ref BigInteger serialNo,bool isServer=false)
        {
            DateTimeOffset notBefore = DateTimeOffset.UtcNow.AddHours(-2);
            DateTimeOffset notAfter = DateTimeOffset.UtcNow.AddYears(3);
            SecureRandom random = randomService.GenerateRandom();

            CertificateBuilder2 builder = builderFactory(random, SignerCertificate);

            AsymmetricCipherKeyPair keyPair = CertificateBuilder2.GenerateKeyPair(2048, random);

            X509Name signerSubject = new X509CertificateParser().ReadCertificate(SignerCertificate.Certificate.RawData).IssuerDN;

            CertificateWithKey certificate = builder.WithSerialNo()
                .WithSubjectCommonName(request.CommonName)
                .WithKeyPair(keyPair)
                .SetIssuer(signerSubject)
                .SetNotAfter(notAfter)
                .SetNotBefore(notBefore)
                .WithSubjectAlternativeName(request.Hostnames, request.IpAddresses)
                .WithBasicConstraints(BasicConstrainsConstants.EndEntity)
                .WithExtendedKeyUsage(isServer)               
                .WithAuthorityKeyIdentifier(SignerCertificate.KeyPair)
                .Generate(SignerCertificate.KeyPair);

            serialNo = builder.SerialNo;

            return ConvertToPfx(certificate.Certificate, (RsaPrivateCrtKeyParameters)keyPair.Private, request.Password);
        }

        private byte[] ConvertToPfx(X509Certificate2 x509, RsaPrivateCrtKeyParameters rsaParams, string pfxPassword)
        {
            var store = new Pkcs12Store();
            SecureRandom random = randomService.GenerateRandom();
            X509Certificate cert = DotNetUtilities.FromX509Certificate(x509);
            string friendlyName = cert.SubjectDN.ToString();
            var certificateEntry = new X509CertificateEntry(cert);

            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName,
                new AsymmetricKeyEntry(rsaParams),
                new[]
                {
                    certificateEntry
                });

            using (var stream = new MemoryStream())
            {
                store.Save(stream, pfxPassword.ToCharArray(), random);
                return stream.ToArray();
            }
        }
    }
}
