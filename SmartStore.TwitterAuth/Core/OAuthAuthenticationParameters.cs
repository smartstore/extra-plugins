//Contributor:  Nicholas Mayne
using System;
using System.Collections.Generic;
using SmartStore.Services.Authentication.External;

namespace SmartStore.TwitterAuth.Core
{
    [Serializable]
    public class OAuthAuthenticationParameters : OpenAuthenticationParameters
    {
        private readonly string _providerSystemName;
        private IList<UserClaims> _claims;

        public OAuthAuthenticationParameters(string providerSystemName)
        {
            _providerSystemName = providerSystemName;
        }

        public override string ProviderSystemName => _providerSystemName;

        public override IList<UserClaims> UserClaims => _claims;

        public void AddClaim(UserClaims claim)
        {
            if (_claims == null)
            {
                _claims = new List<UserClaims>();
            }

            _claims.Add(claim);
        }
    }
}