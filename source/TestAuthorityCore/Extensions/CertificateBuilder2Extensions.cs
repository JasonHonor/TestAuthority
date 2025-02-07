﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nelibur.Sword.Extensions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;
using TestAuthorityCore.Model;
using TestAuthorityCore.X509;

namespace TestAuthorityCore.Extensions
{
    public static class CertificateBuilder2Extensions
    {
        public static X509Name GetX509Name(Dictionary<DerObjectIdentifier, string> nameComponents)
        {
            DerObjectIdentifier[] keys = nameComponents.Keys.ToArray();
            string[] values = nameComponents.Values.ToArray();

            return new X509Name(keys, values);
        }

        public static ICertificateBuilder WithAuthorityKeyIdentifier(this ICertificateBuilder builder, AsymmetricCipherKeyPair authorityKeyPair)
        {
            SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(authorityKeyPair.Public);
            builder.AddExtension(X509Extensions.AuthorityKeyIdentifier.Id, false, new AuthorityKeyIdentifier(subjectPublicKeyInfo));

            return builder;
        }

        public static ICertificateBuilder WithExtendedKeyUsage(this ICertificateBuilder builder,bool isServer)
        {
            if (isServer)
                return WithServerKeyUsage(builder);
            else
                return WithClientKeyUsage(builder);
        }

        public static ICertificateBuilder WithClientKeyUsage(this ICertificateBuilder builder)
        {
            var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.IdKPClientAuth);
            builder.AddExtension(X509Extensions.ExtendedKeyUsage.Id, false, extendedKeyUsage);

            return builder;
        }

        public static ICertificateBuilder WithServerKeyUsage(this ICertificateBuilder builder)
        {
            var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth);
            builder.AddExtension(X509Extensions.ExtendedKeyUsage.Id, false, extendedKeyUsage);

            return builder;
        }

        public static ICertificateBuilder WithKeyUsage(this ICertificateBuilder builder)
        {
            builder.AddExtension(X509Extensions.KeyUsage.Id, true, new KeyUsage(KeyUsage.CrlSign | KeyUsage.KeyCertSign | KeyUsage.DigitalSignature | KeyUsage.NonRepudiation));

            return builder;
        }

        public static ICertificateBuilder WithSubjectAlternativeName(this ICertificateBuilder builder, List<string> hostnames = null, List<string> ipAddresses = null)
        {
            var result = new List<Asn1Encodable>();

            hostnames?.Select(x => new GeneralName(GeneralName.DnsName, x))
                .Select(x => x as Asn1Encodable)
                .ToList()
                .ForEach(result.Add);

            ipAddresses?.Select(x => new GeneralName(GeneralName.IPAddress, x))
                .Select(x => x as Asn1Encodable)
                .ToList()
                .ForEach(result.Add);

            if (result.IsEmpty())
            {
                return builder;
            }

            var extension = new DerSequence(result.ToArray());
            builder.AddExtension(X509Extensions.SubjectAlternativeName.Id, false, extension);
            return builder;
        }

        public static ICertificateBuilder WithSubjectCommonName(this ICertificateBuilder builder, string commonName)
        {
            var subjectComponents = new Dictionary<DerObjectIdentifier, string>
            {
                { X509Name.CN, commonName }
            };

            X509Name subject = GetX509Name(subjectComponents);

            builder.SetSubject(subject);

            return builder;
        }

        public static ICertificateBuilder WithSubjectKeyIdentifier(this ICertificateBuilder builder)
        {
            SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(builder.PublicKeyInfo);
            builder.AddExtension(X509Extensions.SubjectKeyIdentifier.Id, false, new SubjectKeyIdentifier(subjectPublicKeyInfo));
            return builder;
        }

        public static ICertificateBuilder WithSerialNo(this ICertificateBuilder builder)
        {
            //builder.AddExtension(X509Extensions.KeyUsage.Id, true, new KeyUsage(KeyUsage.CrlSign | KeyUsage.KeyCertSign | KeyUsage.DigitalSignature | KeyUsage.NonRepudiation));
            builder.SerialNo = BigInteger.ValueOf((Int64)Store.GetMaxId()+1);
            
            return builder;
        }
    }
}
