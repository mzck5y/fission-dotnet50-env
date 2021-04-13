using Fission.DotNetCore.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fission.DotNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HmacSha1Attribute : Attribute
    {
        #region Properties

        public string SecretKey { get; set; }
        public string SignatureHeaderName { get; set; }

        #endregion

        #region Constructurs

        public HmacSha1Attribute(string signatureHeader, string secret)
        {
            SecretKey = secret;
            SignatureHeaderName = signatureHeader;
        }

        #endregion

        #region Public Methods

        public async Task<bool> IsSignatureValidAsync(FissionContext context)
        {
            string signature = context.Request.Headers[SignatureHeaderName];
            if (SecretKey == null || signature == null)
            {
                return false;
            }

            context.Request.EnableBuffering();
            StreamReader reader = new StreamReader(context.Request.Body);
            string payload = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            string hash = GetHash(signature);
            string expectedHash = ComputeHash(payload ?? string.Empty, SecretKey);

            return hash == expectedHash;
        }

        #endregion

        #region Private Methods

        private static string GetHash(string hash)
        {
            if (hash != null && hash.StartsWith("sha1="))
            {
                return hash.Remove(0, 5);
            }

            return hash;
        }

        public static string ComputeHash(string message, string secret)
        {
            if (message == null || secret == null)
            {
                return null;
            }

            byte[] secretByteArray = Encoding.UTF8.GetBytes(secret);
            byte[] messageByteArray = Encoding.UTF8.GetBytes(message);

            using HMACSHA1 hmac = new HMACSHA1(secretByteArray);
            var data = hmac.ComputeHash(messageByteArray);

            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        #endregion
    }
}
