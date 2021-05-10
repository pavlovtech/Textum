using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ocelot.Authorization;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Responses;

namespace TextumReader.Gateway.Security
{
    public class DelimitedScopesAuthorizer : IScopesAuthorizer
    {
        private readonly IClaimsParser _claimsParser;
        private readonly string _scope = "scope";

        public DelimitedScopesAuthorizer(IClaimsParser claimsParser)
        {
            _claimsParser = claimsParser;
        }

        public Response<bool> Authorize(ClaimsPrincipal claimsPrincipal, List<string> routeAllowedScopes)
        {
            if (routeAllowedScopes == null || routeAllowedScopes.Count == 0)
            {
                return new OkResponse<bool>(true);
            }

            var values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, _scope);

            if (values.IsError)
            {
                return new ErrorResponse<bool>(values.Errors);
            }

            var userScopes = new List<string>();
            foreach (var item in values.Data)
            {
                if (item.Contains(' '))
                {
                    var scopes = item.Split(' ').ToList();
                    userScopes.AddRange(scopes);
                }
                else
                {
                    userScopes.Add(item);
                }
            }

            var matchesScopes = routeAllowedScopes.Intersect(userScopes).ToList();

            if (matchesScopes.Count == 0)
            {
                return new ErrorResponse<bool>(
                    new ScopeNotAuthorizedError($"no one user scope: '{string.Join(",", userScopes)}' match with some allowed scope: '{string.Join(",", routeAllowedScopes)}'"));
            }

            return new OkResponse<bool>(true);
        }
    }
}
