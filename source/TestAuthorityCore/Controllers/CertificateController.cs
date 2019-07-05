using System;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Nelibur.Sword.Extensions;
using Org.BouncyCastle.Math;
using TestAuthorityCore.Model;
using TestAuthorityCore.Service;
using TestAuthorityCore.X509;

namespace TestAuthorityCore.Controllers
{
    [Route("api/certificate")]
    public class CertificateController : Controller
    {
        private readonly RootCertificateService rootCertificateService;
        private readonly CertificateAuthorityService service;

        public CertificateController(CertificateAuthorityService service, RootCertificateService rootCertificateService)
        {
            this.service = service;
            this.rootCertificateService = rootCertificateService;
        }

        [HttpGet("/api/certificate/root")]
        public IActionResult GetRootCertificate()
        {
            byte[] result = rootCertificateService.GetRootCertificate().Certificate.RawData;
            return File(result, MediaTypeNames.Application.Octet, "root.cer");
        }

        /// <summary>
        /// Issue a certificate. Export in PFX format.
        /// </summary>
        /// <param name="commonName">CommonName used in certificate.</param>
        /// <param name="password">Password that is used by Pfx container.</param>
        /// <param name="hostname">Hostnames that will be added </param>
        /// <param name="ipAddress"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpGet("/api/certificate/server/issue")]
        public IActionResult IssueCertificate([FromQuery] string commonName, [FromQuery] string password, [FromQuery] string[] hostname, [FromQuery] string[] ipAddress)
        {
            if (hostname.IsNullOrEmpty())
            {
                return BadRequest("At least one hostname is required");
            }

            if (commonName.IsNullOrEmpty())
            {
                commonName = $"SSL Certificate ({hostname.First()})";
            }

            string filename = commonName + ".pfx";

            if (password.IsNullOrEmpty())
            {
                password = "123123123";
            }

            if (ipAddress.IsNullOrEmpty())
            {
                ipAddress = new string[0];
            }

            var request = new PfxCertificateRequest
            {
                CommonName = commonName,
                Hostnames = hostname.ToList(),
                IpAddresses = ipAddress.ToList(),
                Password = password
            };

            BigInteger serialNo = BigInteger.Zero;

            byte[] certificate = service.GenerateSslCertificate(request,ref serialNo,true);

            Store.Insert(string.Format("{0}",serialNo.IntValue),Convert.ToBase64String(certificate), commonName, password);

            return File(certificate, MediaTypeNames.Application.Octet, filename);
        }

        /// <summary>
        /// Issue a certificate. Export in PFX format.
        /// </summary>
        /// <param name="commonName">CommonName used in certificate.</param>
        /// <param name="password">Password that is used by Pfx container.</param>
        /// <param name="hostname">Hostnames that will be added </param>
        /// <param name="ipAddress"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpGet("/api/certificate/client/issue")]
        public IActionResult IssueClientCertificate([FromQuery] string commonName, [FromQuery] string password)
        {
            if (commonName.IsNullOrEmpty())
            {
                return BadRequest("CommonName is required");
            }

            string filename = commonName+".pfx";

            if (password.IsNullOrEmpty())
            {
                password = "123123123";
            }

            var request = new PfxCertificateRequest
            {
                CommonName = commonName,
                Password = password
            };

            BigInteger serialNo= BigInteger.Zero;

            byte[] certificate = service.GenerateSslCertificate(request,ref serialNo);

            Store.Insert(string.Format("{0}", serialNo.IntValue), Convert.ToBase64String(certificate), commonName, password);

            return File(certificate, MediaTypeNames.Application.Octet, filename);
        }
    }
}
