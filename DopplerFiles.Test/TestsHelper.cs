using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace DopplerFiles.Test
{
    public static class TestsHelper
    {
        private static string _xml_rsa_Keys;
        private const int TOKEN_EXPIRATION_TIME_MINUTES = 60;

        static TestsHelper()
        {
            _xml_rsa_Keys = File.ReadAllText("private-keys-dev/dev.priv.xml");
        }
        public static string GetKeys()
        {
            return _xml_rsa_Keys;
        }

        public static string GetSuperUserAuthenticationToken()
        {
            var payload = new Dictionary<string, object>
            {
                { "isSU", "true" },
                { "exp", DateTimeOffset
                    .UtcNow
                    .AddMinutes(TOKEN_EXPIRATION_TIME_MINUTES).ToUnixTimeSeconds()}
            };

            return CreateToken(payload);
        }

        public static string GetAnotherUserAuthenticationToken(string idUser)
        {
            var payload = new Dictionary<string, object>
            {
                { "nameid", idUser },
                { "unique_name", idUser },
                { "isSU", false },
                { "sub", idUser },
                { "role", "USER" },
                { "exp", DateTimeOffset
                    .UtcNow
                    .AddMinutes(TOKEN_EXPIRATION_TIME_MINUTES).ToUnixTimeSeconds()}
            };

            return CreateToken(payload);
        }

        private static string CreateToken(Dictionary<string, object> payload = null)
        {
            using var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(_xml_rsa_Keys);
            var rsaParameters = rsa.ExportParameters(true);
            rsa.ImportParameters(rsaParameters);

            return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256);
        }
    }
}
